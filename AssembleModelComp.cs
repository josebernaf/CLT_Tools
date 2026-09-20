using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CLT_Tools
{
    public class AssembleModelComp : GH_Component
    {
        public AssembleModelComp()
          : base("Assemble Model", "Assemble", "Creates the CLT model for further calculations.", "CLT Tools", "1 :: Model")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Element(s)", "Elems", "Input shell(s) of the model.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Support(s)", "Sup", "Input support(s) of the model..", GH_ParamAccess.list);
            pManager.AddGenericParameter("Load(s)", "Load", "Input load(s) of the model.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Joint(s)", "Joint", "Input joint(s) of the model.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material(s)", "Mats", "Material(s) of the model.", GH_ParamAccess.list);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "CLT model based on inputs.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Mass", "Mass", "Mass structure in [kg] as a string.", GH_ParamAccess.item);
            pManager.AddPointParameter("Centre of gravity", "COG", "Centre of gravity of the model.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_ObjectWrapper> eW = new List<GH_ObjectWrapper>();
            List<GH_ObjectWrapper> sW = new List<GH_ObjectWrapper>();
            List<GH_ObjectWrapper> lW = new List<GH_ObjectWrapper>();
            List<GH_ObjectWrapper> jW = new List<GH_ObjectWrapper>();
            List<GH_ObjectWrapper> mW = new List<GH_ObjectWrapper>();

            DA.GetDataList(0, eW);
            DA.GetDataList(1, sW);
            DA.GetDataList(2, lW);
            DA.GetDataList(3, jW);
            DA.GetDataList(4, mW);

            List<CLTElement> elems = new List<CLTElement>();
            foreach (var w in eW) if (w?.Value is CLTElement e) elems.Add(e);

            List<CLTSupportData> sups = new List<CLTSupportData>();
            foreach (var w in sW) if (w?.Value is CLTSupportData s) sups.Add(s);

            List<CLTLoadData> loads = new List<CLTLoadData>();
            foreach (var w in lW) if (w?.Value is CLTLoadData l) loads.Add(l);

            List<CLTJointData> joints = new List<CLTJointData>();
            foreach (var w in jW) if (w?.Value is CLTJointData j) joints.Add(j);

            Dictionary<string, CLTMaterial> matLibrary = new Dictionary<string, CLTMaterial>();
            foreach (var w in mW)
            {
                if (w?.Value is CLTMaterial mat && !string.IsNullOrEmpty(mat.Name))
                    if (!matLibrary.ContainsKey(mat.Name)) matLibrary.Add(mat.Name, mat);
            }


            // 2. Reconstrucción de Cuadrícula
            try
            {
                foreach (var el in elems)
                    if (el.Geometry is Mesh m) m.UserDictionary.Clear();
                Conformalize_Regrid(elems, 0.001);
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Error in Regridding: {ex.Message}");
            }

            // 3. Unidades y Materiales
            var docUnits = RhinoDoc.ActiveDoc.ModelUnitSystem;
            double areaFactor = 1.0;
            if (docUnits == UnitSystem.Millimeters) areaFactor = 1e-6;
            else if (docUnits == UnitSystem.Centimeters) areaFactor = 1e-4;
            else if (docUnits == UnitSystem.Meters) areaFactor = 1.0;

            for (int i = 0; i < elems.Count; i++)
            {
                CLTElement el = elems[i];
                if (el.Material == null)
                {
                    string needed = el.MaterialName;
                    if (!string.IsNullOrEmpty(needed) && matLibrary.ContainsKey(needed))
                        el.Material = matLibrary[needed];
                }
            }

            // 4. Cálculo de Masa
            double totalMass = 0;
            double sumX = 0, sumY = 0, sumZ = 0;

            for (int i = 0; i < elems.Count; i++)
            {
                CLTElement el = elems[i];
                if (el.Geometry == null) continue;

                AreaMassProperties amp = AreaMassProperties.Compute(el.Geometry);
                double rhinoArea = (amp != null) ? amp.Area : 0;
                double thick_mm = (el.Properties != null && el.Properties.Thicknesses != null)
                                  ? el.Properties.Thicknesses.Sum() : 0;

                double area_m2 = rhinoArea * areaFactor;
                double thick_m = thick_mm / 1000.0;
                double vol_m3 = area_m2 * thick_m;
                double rho = (el.Material != null) ? el.Material.Rho : 0;

                double mass = vol_m3 * rho;
                totalMass += mass;

                if (mass > 0)
                {
                    Point3d c = el.Geometry.GetBoundingBox(true).Center;
                    sumX += c.X * mass; sumY += c.Y * mass; sumZ += c.Z * mass;
                }
            }

            Point3d cog = Point3d.Origin;
            if (totalMass > 0) cog = new Point3d(sumX / totalMass, sumY / totalMass, sumZ / totalMass);

            // 5. Salidas
            DA.SetData(0, new CLTModel(elems, loads, sups, joints));
            DA.SetData(1, totalMass);
            DA.SetData(2, cog);

            this.Message = $"{elems.Count} Elms\n{totalMass:F0} kg";
        }

        // --- ALGORITMO: REGRIDDING (RECONSTRUCCIÓN DE REJILLA) ---
        private int Conformalize_Regrid(List<CLTElement> elements, double tolerance)
        {
            // 1. Nube de puntos global
            List<Point3d> globalPts = new List<Point3d>();
            foreach (var el in elements)
            {
                if (el.Geometry is Mesh m) globalPts.AddRange(m.Vertices.ToPoint3dArray());
            }

            // RTree para velocidad
            RTree cloud = new RTree();
            for (int i = 0; i < globalPts.Count; i++) cloud.Insert(globalPts[i], i);

            int rebuildCount = 0;

            foreach (var el in elements)
            {
                if (!(el.Geometry is Mesh m)) continue;
                if (m.Vertices.Count < 3) continue;

                Point3d[] corners = m.Vertices.ToPoint3dArray();
                Plane plane;

                if (Plane.FitPlaneToPoints(corners, out plane) != PlaneFitResult.Success) continue;

                List<double> xCoords = new List<double>();
                List<double> yCoords = new List<double>();

                foreach (Point3d v in corners)
                {
                    double u, vCoord;
                    plane.ClosestParameter(v, out u, out vCoord);
                    xCoords.Add(u);
                    yCoords.Add(vCoord);
                }

                xCoords = CleanCoords(xCoords, tolerance);
                yCoords = CleanCoords(yCoords, tolerance);

                if (xCoords.Count == 0 || yCoords.Count == 0) continue;

                double minX = xCoords.Min(); double maxX = xCoords.Max();
                double minY = yCoords.Min(); double maxY = yCoords.Max();

                BoundingBox searchBox = m.GetBoundingBox(true);
                searchBox.Inflate(tolerance);

                bool newSplitsFound = false;

                cloud.Search(searchBox, (sender, args) =>
                {
                    Point3d pt = globalPts[args.Id];

                    double u, vCoord;
                    plane.ClosestParameter(pt, out u, out vCoord);

                    if (Math.Abs(plane.DistanceTo(pt)) > tolerance) return;

                    bool onXEdge = (Math.Abs(u - minX) < tolerance || Math.Abs(u - maxX) < tolerance);
                    bool onYEdge = (Math.Abs(vCoord - minY) < tolerance || Math.Abs(vCoord - maxY) < tolerance);

                    bool withinX = (u > minX - tolerance && u < maxX + tolerance);
                    bool withinY = (vCoord > minY - tolerance && vCoord < maxY + tolerance);

                    if (onYEdge && withinX)
                    {
                        if (!IsValueInList(xCoords, u, tolerance))
                        {
                            xCoords.Add(u);
                            newSplitsFound = true;
                        }
                    }

                    if (onXEdge && withinY)
                    {
                        if (!IsValueInList(yCoords, vCoord, tolerance))
                        {
                            yCoords.Add(vCoord);
                            newSplitsFound = true;
                        }
                    }
                });

                if (newSplitsFound)
                {
                    xCoords.Sort();
                    yCoords.Sort();

                    Mesh newMesh = new Mesh();

                    for (int y = 0; y < yCoords.Count; y++)
                    {
                        for (int x = 0; x < xCoords.Count; x++)
                        {
                            Point3d pt = plane.PointAt(xCoords[x], yCoords[y]);
                            newMesh.Vertices.Add(pt);
                        }
                    }

                    int width = xCoords.Count;
                    for (int y = 0; y < yCoords.Count - 1; y++)
                    {
                        for (int x = 0; x < xCoords.Count - 1; x++)
                        {
                            int i = y * width + x;
                            newMesh.Faces.AddFace(i, i + 1, i + width + 1, i + width);
                        }
                    }

                    newMesh.Normals.ComputeNormals();
                    newMesh.FaceNormals.ComputeFaceNormals();
                    newMesh.Compact();
                    newMesh.UserDictionary.Clear();

                    if (newMesh.IsValid)
                    {
                        el.Geometry = newMesh;
                        rebuildCount++;
                    }
                }
            }

            return rebuildCount;
        }

        // Helpers
        private List<double> CleanCoords(List<double> vals, double tol)
        {
            vals.Sort();
            List<double> clean = new List<double>();
            if (vals.Count == 0) return clean;
            clean.Add(vals[0]);
            for (int i = 1; i < vals.Count; i++)
            {
                if (Math.Abs(vals[i] - vals[i - 1]) > tol) clean.Add(vals[i]);
            }
            return clean;
        }

        private bool IsValueInList(List<double> list, double val, double tol)
        {
            foreach (double v in list) if (Math.Abs(v - val) < tol) return true;
            return false;
        }

        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.AssembleModel_icon;
        public override Guid ComponentGuid => new Guid("eeee9999-1111-2222-3333-555555555555");
    }
}