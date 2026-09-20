using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Display;
using System.Linq;

namespace CLT_Tools
{
    public class ForcesViewerComp : GH_Component
    {
        // --- ESTADO PERSISTENTE (UI) ---
        public bool ExpandCaseMenu { get; set; } = true;
        public bool ExpandCompMenu { get; set; } = true;
        public bool ExpandValuesMenu { get; set; } = true;

        public int SelectedCaseIndex { get; set; } = 0;
        public int SelectedComponentIndex { get; set; } = 3; // Por defecto Bending X (Mxx)

        // Opciones de visualización de texto
        public bool ShowValues { get; set; } = false;
        public double ValueThreshold { get; set; } = 0.0;
        public int TextSize { get; set; } = 12;

        public double CurrentMaxVal { get; set; } = 0.0;

        // Listas de datos UI
        public List<string> CaseNames { get; set; } = new List<string>();

        public readonly List<string> ComponentNames = new List<string>()
        {
            "Membrane X (Nxx)", "Membrane Y (Nyy)", "Membrane XY (Nxy)",
            "Membrane Max (FMax)", "Membrane Min (FMin)", "Membrane Angle (FAngle)",
            "Bending X (Mxx)", "Bending Y (Myy)", "Torsion (Mxy)",
            "Bending Max (MMax)", "Bending Min (MMin)", "Bending Angle (MAngle)",
            "Shear XZ (Vxz)", "Shear YZ (Vyz)", "Shear Max (VMax)"
        };

        // Info visual
        public string MaxText { get; set; } = "Max. Value: -";
        public string MinText { get; set; } = "Min. Value: -";

        // CACHÉ PARA DIBUJAR TEXTOS EN PANTALLA
        private List<Point3d> _cachedTagLocs = new List<Point3d>();
        private List<string> _cachedTagTexts = new List<string>();

        // --- CACHE LEYENDA 3D ---
        private Mesh _bakeLegendMesh = new Mesh();
        private List<Curve> _bakeLegendWires = new List<Curve>();
        private class BakeTextData { public string Text; public Plane Plane; public double Height; }
        private List<BakeTextData> _bakeLegendTextData = new List<BakeTextData>();
        private List<Rhino.Display.Text3d> _previewLegendTexts = new List<Rhino.Display.Text3d>();
        private Mesh _cachedVisMesh = new Mesh();

        public ForcesViewerComp()
          : base("Forces Viewer", "ForcesView", "Forces visualizer that allows inspecting and analyzing internal forces and moments on the structural mesh in real time using a color map and interactive controls.", "CLT Tools", "5.Results")
        {
        }
        public override void CreateAttributes()
        {
            m_attributes = new ForcesViewAttributes(this);
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Analysed Model", "Mesh", "Malla analizada", GH_ParamAccess.item);
            pManager.AddVectorParameter("Membrane Forces (N)", "VecN", "Árbol de fuerzas N", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Bending Moments (M)", "VecM", "Árbol de momentos M", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Shear Forces (V)", "VecV", "Árbol de cortantes V", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Principal Membrane", "PrinN", "Árbol de FMax, FMin, FAngle", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Principal Bending", "PrinM", "Árbol de MMax, MMin, MAngle", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Filter Selection", "Filt", "Filter (Text ID or Geometry).", GH_ParamAccess.list);
            pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("VisMesh", "VisM", "Malla coloreada (Contorno Suave)", GH_ParamAccess.item);
            pManager.AddTextParameter("Legend Values", "LegV", "List of the 5 values (Max to Min) that form the Legend.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = null;
            GH_Structure<GH_Vector> treeN = null;
            GH_Structure<GH_Vector> treeM = null;
            GH_Structure<GH_Vector> treeV = null;
            GH_Structure<GH_Vector> treePrinN = null;
            GH_Structure<GH_Vector> treePrinM = null;

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataTree(1, out treeN)) return;
            if (!DA.GetDataTree(2, out treeM)) return;
            if (!DA.GetDataTree(3, out treeV)) return;
            if (!DA.GetDataTree(4, out treePrinN)) return;
            if (!DA.GetDataTree(5, out treePrinM)) return;
            List<object> filterList = new List<object>();
            DA.GetDataList(6, filterList);

            _cachedTagLocs.Clear();
            _cachedTagTexts.Clear();
            _bakeLegendMesh = new Mesh();
            _bakeLegendWires.Clear();
            _bakeLegendTextData.Clear();
            _previewLegendTexts.Clear();
            _cachedVisMesh = new Mesh();
            CurrentMaxVal = 0.0;

            // 1. UI: GESTIÓN DE CASOS
            CaseNames.Clear();
            if (mesh.UserDictionary.ContainsKey("CLT_CaseNames"))
            {
                var val = mesh.UserDictionary["CLT_CaseNames"];
                if (val is System.Collections.IEnumerable list)
                    foreach (var item in list) CaseNames.Add(item.ToString());
            }

            List<string> elemIDs = new List<string>();
            if (mesh.UserDictionary.ContainsKey("CLT_ElementIDs"))
            {
                var valID = mesh.UserDictionary["CLT_ElementIDs"];
                if (valID is System.Collections.IEnumerable listID)
                    foreach (var item in listID) elemIDs.Add(item.ToString());
            }

            if (CaseNames.Count == 0) CaseNames.Add("LoadCase 0");
            while (CaseNames.Count < treeN.PathCount) CaseNames.Add($"Case {CaseNames.Count}");

            CaseNames.Add("Envelope (Max Abs)");

            if (SelectedCaseIndex >= CaseNames.Count) SelectedCaseIndex = 0;
            if (SelectedCaseIndex < 0) SelectedCaseIndex = 0;

            // 2. EXTRAER VALORES POR CARA
            int numFaces = mesh.Faces.Count;
            double[] faceValues = new double[numFaces];
            bool useFilter = filterList.Count > 0 && filterList[0] != null;
            bool[] isFaceVisible = new bool[numFaces];

            if (!useFilter)
            {
                for (int i = 0; i < isFaceVisible.Length; i++) isFaceVisible[i] = true;
            }
            else
            {
                HashSet<string> targetIDs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                List<GeometryBase> targetGeoms = new List<GeometryBase>();

                foreach (var obj in filterList)
                {
                    string textVal = null;
                    if (obj is GH_String gs) textVal = gs.Value;
                    else if (obj is string sStr) textVal = sStr;

                    if (textVal != null)
                    {
                        // Limpiamos los saltos de línea y espacios que suele añadir el Panel
                        string[] splits = textVal.Split(new char[] { '\n', '\r', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string s in splits)
                        {
                            string trimmed = s.Trim();
                            if (!string.IsNullOrEmpty(trimmed)) targetIDs.Add(trimmed);
                        }
                    }
                    else if (obj is IGH_Goo goo && goo.ScriptVariable() is GeometryBase geo) targetGeoms.Add(geo);
                    else if (obj is GeometryBase rawGeo) targetGeoms.Add(rawGeo);
                }

                for (int i = 0; i < numFaces; i++)
                {
                    bool visible = false;
                    Point3d center = mesh.Faces.GetFaceCenter(i);
                    if (targetIDs.Count > 0 && i < elemIDs.Count)
                    {
                        if (targetIDs.Contains(elemIDs[i])) visible = true;
                    }
                    if (!visible && targetGeoms.Count > 0)
                    {
                        foreach (var g in targetGeoms)
                        {
                            double dist = g.GetBoundingBox(true).ClosestPoint(center).DistanceTo(center);
                            if (dist < 10.0) { visible = true; break; }
                        }
                    }
                    isFaceVisible[i] = visible;
                }
            }

            GH_Structure<GH_Vector> targetTree = null;
            int vectorComponent = 0;
            string units = "";

            switch (SelectedComponentIndex)
            {
                case 0: targetTree = treeN; vectorComponent = 0; units = "kN/m"; break;
                case 1: targetTree = treeN; vectorComponent = 1; units = "kN/m"; break;
                case 2: targetTree = treeN; vectorComponent = 2; units = "kN/m"; break;
                case 3: targetTree = treePrinN; vectorComponent = 0; units = "kN/m"; break;
                case 4: targetTree = treePrinN; vectorComponent = 1; units = "kN/m"; break;
                case 5: targetTree = treePrinN; vectorComponent = 2; units = "Deg"; break;
                case 6: targetTree = treeM; vectorComponent = 0; units = "kNm/m"; break;
                case 7: targetTree = treeM; vectorComponent = 1; units = "kNm/m"; break;
                case 8: targetTree = treeM; vectorComponent = 2; units = "kNm/m"; break;
                case 9: targetTree = treePrinM; vectorComponent = 0; units = "kNm/m"; break;
                case 10: targetTree = treePrinM; vectorComponent = 1; units = "kNm/m"; break;
                case 11: targetTree = treePrinM; vectorComponent = 2; units = "Deg"; break;
                case 12: targetTree = treeV; vectorComponent = 0; units = "kN/m"; break;
                case 13: targetTree = treeV; vectorComponent = 1; units = "kN/m"; break;
                case 14: targetTree = treeV; vectorComponent = 2; units = "kN/m"; break;
            }

            bool isEnvelope = (SelectedCaseIndex == CaseNames.Count - 1);
            double minFaceVal = double.MaxValue;
            double maxFaceVal = double.MinValue;

            for (int i = 0; i < numFaces; i++)
            {
                double val = 0;
                if (isEnvelope)
                {
                    double maxAbs = -1.0;
                    double finalVal = 0;
                    foreach (GH_Path path in targetTree.Paths)
                    {
                        var branch = targetTree.get_Branch(path);
                        if (i < branch.Count)
                        {
                            GH_Vector ghVec = branch[i] as GH_Vector;
                            if (ghVec != null)
                            {
                                double v = (vectorComponent == 0) ? ghVec.Value.X : (vectorComponent == 1) ? ghVec.Value.Y : ghVec.Value.Z;
                                if (Math.Abs(v) > maxAbs) { maxAbs = Math.Abs(v); finalVal = v; }
                            }
                        }
                    }
                    val = finalVal;
                }
                else
                {
                    int idx = Math.Max(0, Math.Min(SelectedCaseIndex, targetTree.PathCount - 1));
                    var branch = targetTree.get_Branch(idx);
                    if (i < branch.Count)
                    {
                        GH_Vector ghVec = branch[i] as GH_Vector;
                        if (ghVec != null)
                            val = (vectorComponent == 0) ? ghVec.Value.X : (vectorComponent == 1) ? ghVec.Value.Y : ghVec.Value.Z;
                    }
                }

                faceValues[i] = val;
                if (useFilter && !isFaceVisible[i]) continue;
                if (val < minFaceVal) minFaceVal = val;
                if (val > maxFaceVal) maxFaceVal = val;
            }

            if (minFaceVal == double.MaxValue || maxFaceVal == double.MinValue)
            {
                minFaceVal = 0.0;
                maxFaceVal = 0.0;
            }

            if (Math.Abs(maxFaceVal - minFaceVal) < 1e-9) { maxFaceVal += 0.1; minFaceVal -= 0.1; }
            CurrentMaxVal = Math.Max(Math.Abs(maxFaceVal), Math.Abs(minFaceVal));

            // Guardar la información para los textos en pantalla (usando el Threshold)
            if (ShowValues)
            {
                for (int i = 0; i < numFaces; i++)
                {
                    if (Math.Abs(faceValues[i]) >= ValueThreshold)
                    {
                        Point3d center = mesh.Faces.GetFaceCenter(i);
                        _cachedTagLocs.Add(center);
                        _cachedTagTexts.Add(faceValues[i].ToString("0.00"));
                    }
                }
            }

            MaxText = $"Max. Value: {maxFaceVal:0.00} {units}";
            MinText = $"Min. Value: {minFaceVal:0.00} {units}";

            // 3. SUAVIZADO NODAL (Nodal Averaging)
            int numNodes = mesh.Vertices.Count;
            double[] nodeValues = new double[numNodes];
            int[] nodeCounts = new int[numNodes];

            for (int i = 0; i < numFaces; i++)
            {
                MeshFace f = mesh.Faces[i];
                double val = faceValues[i];

                nodeValues[f.A] += val; nodeCounts[f.A]++;
                nodeValues[f.B] += val; nodeCounts[f.B]++;
                nodeValues[f.C] += val; nodeCounts[f.C]++;
                if (f.IsQuad) { nodeValues[f.D] += val; nodeCounts[f.D]++; }
            }

            for (int i = 0; i < numNodes; i++)
                if (nodeCounts[i] > 0) nodeValues[i] /= nodeCounts[i];

            // 4. PINTAR LA MALLA
            Mesh visMesh = mesh.DuplicateMesh();
            visMesh.VertexColors.Clear();

            for (int i = 0; i < numNodes; i++)
            {
                double val = nodeValues[i];
                double t = (val - minFaceVal) / (maxFaceVal - minFaceVal);
                visMesh.VertexColors.Add(GetCustomColor(t));
            }
            if (useFilter)
            {
                List<int> facesToDelete = new List<int>();
                for (int i = 0; i < isFaceVisible.Length; i++)
                {
                    if (!isFaceVisible[i]) facesToDelete.Add(i);
                }
                if (facesToDelete.Count > 0)
                {
                    visMesh.Faces.DeleteFaces(facesToDelete);
                    visMesh.Compact();
                }
            }

            // 5. LEYENDA 3D
            BoundingBox originalBox = mesh.GetBoundingBox(true);
            CalculateLegend(originalBox, minFaceVal, maxFaceVal, units, ComponentNames[SelectedComponentIndex]);

            List<string> legendValues = new List<string>();
            legendValues.Add($"{maxFaceVal:0.00} {units}");
            legendValues.Add($"{minFaceVal + 0.75 * (maxFaceVal - minFaceVal):0.00} {units}");
            legendValues.Add($"{minFaceVal + 0.50 * (maxFaceVal - minFaceVal):0.00} {units}");
            legendValues.Add($"{minFaceVal + 0.25 * (maxFaceVal - minFaceVal):0.00} {units}");
            legendValues.Add($"{minFaceVal:0.00} {units}");

            _cachedVisMesh = visMesh; 
            DA.SetData(0, visMesh);
            DA.SetDataList(1, legendValues);
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            base.DrawViewportMeshes(args);
            if (Hidden) return;

            // Malla de esfuerzos principal
            if (_cachedVisMesh != null && _cachedVisMesh.IsValid)
                args.Display.DrawMeshFalseColors(_cachedVisMesh);

            // Barra de gradiente de leyenda
            if (_bakeLegendMesh != null && _bakeLegendMesh.IsValid)
                args.Display.DrawMeshFalseColors(_bakeLegendMesh);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);
            if (Hidden) return;

            // Textos de valor sobre caras
            if (ShowValues && _cachedTagLocs != null && _cachedTagTexts != null && _cachedTagLocs.Count == _cachedTagTexts.Count)
                for (int i = 0; i < _cachedTagLocs.Count; i++)
                    args.Display.Draw2dText(_cachedTagTexts[i], Color.Black, _cachedTagLocs[i], true, TextSize);

            // Textos 3D de la leyenda
            if (_previewLegendTexts != null)
                foreach (var t3d in _previewLegendTexts)
                    args.Display.Draw3dText(t3d, Color.Black);

            // Lineas del borde de la leyenda
            if (_bakeLegendWires != null)
                foreach (var crv in _bakeLegendWires)
                    if (crv != null && crv.IsValid)
                        args.Display.DrawCurve(crv, Color.FromArgb(80, 80, 80), 1);
        }

        public override BoundingBox ClippingBox
        {
            get
            {
                BoundingBox box = base.ClippingBox;
                if (_cachedTagLocs != null && _cachedTagLocs.Count > 0 && ShowValues)
                {
                    BoundingBox ptsBox = new BoundingBox(_cachedTagLocs);
                    box.Union(ptsBox);
                }
                return box;
            }
        }

        public override void BakeGeometry(Rhino.RhinoDoc doc, Rhino.DocObjects.ObjectAttributes att, List<Guid> obj_ids)
        {
            if (att == null) att = doc.CreateDefaultAttributes();


            if (_cachedVisMesh != null && _cachedVisMesh.IsValid)
            {
                var meshAttr = att.Duplicate();
                meshAttr.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;

                string layerName = "Forces_" + CaseNames[SelectedCaseIndex];
                meshAttr.Name = layerName;

                Guid id = doc.Objects.AddMesh(_cachedVisMesh, meshAttr);
                if (id != Guid.Empty) obj_ids.Add(id);
            }

            // 2. Leyenda: malla de gradiente
            if (_bakeLegendMesh != null && _bakeLegendMesh.IsValid)
            {
                var legAttr = att.Duplicate();
                legAttr.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
                legAttr.Name = "Legend_Gradient_Forces";
                Guid id = doc.Objects.AddMesh(_bakeLegendMesh, legAttr);
                if (id != Guid.Empty) obj_ids.Add(id);
            }

            // 3. Leyenda: líneas
            foreach (var crv in _bakeLegendWires)
            {
                if (crv != null && crv.IsValid)
                {
                    Guid id = doc.Objects.AddCurve(crv, att);
                    if (id != Guid.Empty) obj_ids.Add(id);
                }
            }

            // 4. Leyenda: textos
            foreach (var data in _bakeLegendTextData)
            {
                Guid id = doc.Objects.AddText(data.Text, data.Plane, data.Height, "Arial", false, false);
                if (id != Guid.Empty)
                {
                    var obj = doc.Objects.Find(id);
                    if (obj != null) { obj.Attributes = att.Duplicate(); obj.CommitChanges(); }
                    obj_ids.Add(id);
                }
            }
        }

        // --- LEYENDA 3D
        private void CalculateLegend(BoundingBox bbox, double minVal, double maxVal, string units, string componentName)
        {
            double height = bbox.Max.Z - bbox.Min.Z;
            double widthX = bbox.Max.X - bbox.Min.X;
            double widthY = bbox.Max.Y - bbox.Min.Y;
            double maxDim = Math.Max(widthX, widthY);
            if (height < (maxDim * 0.1)) height = maxDim * 0.5;
            if (height < 1.0) height = 10.0;

            double barWidth = height * 0.05;
            double gapX = barWidth * 2;
            Point3d origin = new Point3d(bbox.Max.X + gapX + barWidth, bbox.Min.Y, bbox.Min.Z);

            int steps = 10;
            double stepH = height / steps;

            // A. Malla de color
            for (int i = 0; i <= steps; i++)
            {
                double z = origin.Z + (i * stepH);
                double t = (double)i / steps;
                Color c = GetCustomColor(t);
                _bakeLegendMesh.Vertices.Add(new Point3d(origin.X, origin.Y, z));
                _bakeLegendMesh.Vertices.Add(new Point3d(origin.X + barWidth, origin.Y, z));
                _bakeLegendMesh.VertexColors.Add(c); _bakeLegendMesh.VertexColors.Add(c);
                if (i > 0) { int v = _bakeLegendMesh.Vertices.Count; _bakeLegendMesh.Faces.AddFace(v-4, v-3, v-1, v-2); }
            }
            _bakeLegendMesh.Normals.ComputeNormals(); _bakeLegendMesh.Compact();

            // B. Borde y lineas de tick
            Point3d bl = new Point3d(origin.X, origin.Y, origin.Z);
            Point3d br = new Point3d(origin.X + barWidth, origin.Y, origin.Z);
            Point3d tl = new Point3d(origin.X, origin.Y, origin.Z + height);
            Point3d tr = new Point3d(origin.X + barWidth, origin.Y, origin.Z + height);
            _bakeLegendWires.Add(new Polyline(new Point3d[]{ bl, br, tr, tl, bl }).ToNurbsCurve());

            double txtH = height / 50.0; if (txtH < 0.1) txtH = 1.0;
            for (int i = 0; i <= steps; i++)
            {
                double z = origin.Z + (i * stepH);
                double val = minVal + (maxVal - minVal) * ((double)i / steps);
                string txt = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00} {1}", val, units);
                Point3d p1 = new Point3d(origin.X + barWidth, origin.Y, z);
                Point3d p2 = new Point3d(origin.X + barWidth * 1.2, origin.Y, z);
                _bakeLegendWires.Add(new Line(p1, p2).ToNurbsCurve());
                Point3d txtPos = new Point3d(p2.X + barWidth * 0.1, p2.Y, z);
                Plane plane = new Plane(txtPos, Vector3d.XAxis, Vector3d.ZAxis);
                _bakeLegendTextData.Add(new BakeTextData { Text = txt, Plane = plane, Height = txtH });
                var t3d = new Rhino.Display.Text3d(txt, plane, txtH);
                t3d.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Left;
                t3d.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Middle;
                _previewLegendTexts.Add(t3d);
            }

            // C. Titulo
            string titleTxt = componentName + " [" + units + "]";
            Point3d titlePos = new Point3d(origin.X, origin.Y, origin.Z + height + height * 0.05);
            double titleH = height / 30.0; if (titleH < 0.1) titleH = 1.0;
            Plane titlePl = new Plane(titlePos, Vector3d.XAxis, Vector3d.ZAxis);
            _bakeLegendTextData.Add(new BakeTextData { Text = titleTxt, Plane = titlePl, Height = titleH });
            var titleT3d = new Rhino.Display.Text3d(titleTxt, titlePl, titleH);
            titleT3d.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Left;
            titleT3d.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Bottom;
            _previewLegendTexts.Add(titleT3d);
        }

        private Color GetCustomColor(double t)
        {
            if (t < 0.0) t = 0.0;
            if (t > 1.0) t = 1.0;

            Color[] palette = new Color[]
            {
                Color.FromArgb(41,  98,  173), // 0: Azul oscuro (0%)
                Color.FromArgb(60,  120, 190), // 1
                Color.FromArgb(94,  143, 201), // 2
                Color.FromArgb(130, 175, 215), // 3
                Color.FromArgb(161, 204, 224), // 4: Celeste pálido
                Color.FromArgb(220, 220, 140), // 5: Amarillo suave (50%)
                Color.FromArgb(248, 235, 85),  // 6: Amarillo puro
                Color.FromArgb(248, 185, 59),  // 7: Amarillo anaranjado
                Color.FromArgb(239, 126, 42),  // 8: Naranja
                Color.FromArgb(226, 68,  34),  // 9: Naranja rojizo
                Color.FromArgb(215, 25,  28)   // 10: Rojo intenso (100%)
            };

            double scaled = t * (palette.Length - 1);
            int index = (int)Math.Floor(scaled);
            if (index >= palette.Length - 1) return palette[palette.Length - 1];

            double localT = scaled - index;
            Color c0 = palette[index];
            Color c1 = palette[index + 1];

            int r = (int)(c0.R + localT * (c1.R - c0.R));
            int g = (int)(c0.G + localT * (c1.G - c0.G));
            int b = (int)(c0.B + localT * (c1.B - c0.B));

            return Color.FromArgb(255, r, g, b);
        }

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetBoolean("ExpCase", ExpandCaseMenu);
            writer.SetBoolean("ExpComp", ExpandCompMenu);
            writer.SetBoolean("ExpVal", ExpandValuesMenu);
            writer.SetInt32("SelCase", SelectedCaseIndex);
            writer.SetInt32("SelComp", SelectedComponentIndex);
            writer.SetBoolean("ShowVal", ShowValues);
            writer.SetDouble("ValThres", ValueThreshold);
            writer.SetInt32("TxtSize", TextSize);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (reader.ItemExists("ExpCase")) ExpandCaseMenu = reader.GetBoolean("ExpCase");
            if (reader.ItemExists("ExpComp")) ExpandCompMenu = reader.GetBoolean("ExpComp");
            if (reader.ItemExists("ExpVal")) ExpandValuesMenu = reader.GetBoolean("ExpVal");
            if (reader.ItemExists("SelCase")) SelectedCaseIndex = reader.GetInt32("SelCase");
            if (reader.ItemExists("SelComp")) SelectedComponentIndex = reader.GetInt32("SelComp");
            if (reader.ItemExists("ShowVal")) ShowValues = reader.GetBoolean("ShowVal");
            if (reader.ItemExists("ValThres")) ValueThreshold = reader.GetDouble("ValThres");
            if (reader.ItemExists("TxtSize")) TextSize = reader.GetInt32("TxtSize");
            return base.Read(reader);
        }

        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.ForcesViewer_icon;
        public override Guid ComponentGuid => new Guid("11112222-3333-4444-5555-666677778888");
    }

    // --- CLASE DE ATRIBUTOS (UI) ---
    public class ForcesViewAttributes : GH_ComponentAttributes
    {
        private ForcesViewerComp _owner;

        // Rectángulos
        private RectangleF _recBtnCase, _recListCase;
        private RectangleF _recBtnComp, _recListComp;
        private RectangleF _recBtnVals, _recListVals;
        private RectangleF _recToggleShow, _recSliderThres, _recSliderSize;
        private RectangleF _recInfoText;

        private const float HeaderH = 20;
        private const float ItemH = 20;
        private const float SliderH = 20;
        private const float InfoH = 38;
        private const float FixedWidth = 160;

        private readonly Pen _borderPen = new Pen(Color.FromArgb(51, 51, 51));
        private readonly Brush _selBrush = new SolidBrush(Color.FromArgb(247, 165, 56));

        private bool _isDraggingSlider = false;
        private RectangleF _activeSliderRec;
        private int _activeSliderID = -1;

        public ForcesViewAttributes(ForcesViewerComp owner) : base(owner) { _owner = owner; }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left && _owner.ExpandValuesMenu)
            {
                if (_recSliderThres.Contains(e.CanvasLocation))
                {
                    _isDraggingSlider = true; _activeSliderRec = _recSliderThres; _activeSliderID = 1;
                    UpdateSliderValue(e.CanvasLocation.X); sender.Refresh(); return GH_ObjectResponse.Capture;
                }
                if (_recSliderSize.Contains(e.CanvasLocation))
                {
                    _isDraggingSlider = true; _activeSliderRec = _recSliderSize; _activeSliderID = 2;
                    UpdateSliderValue(e.CanvasLocation.X); sender.Refresh(); return GH_ObjectResponse.Capture;
                }
            }
            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (_isDraggingSlider && e.Button == MouseButtons.Left)
            {
                UpdateSliderValue(e.CanvasLocation.X);
                _owner.ExpireSolution(true);
                sender.Refresh();
                return GH_ObjectResponse.Capture;
            }
            return base.RespondToMouseMove(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (_isDraggingSlider)
            {
                _isDraggingSlider = false;
                _activeSliderID = -1;
                return GH_ObjectResponse.Release;
            }

            if (e.Button == MouseButtons.Left)
            {
                // Toggle Menus
                if (_recBtnCase.Contains(e.CanvasLocation)) { _owner.ExpandCaseMenu = !_owner.ExpandCaseMenu; _owner.ExpireSolution(true); return GH_ObjectResponse.Handled; }
                if (_recBtnComp.Contains(e.CanvasLocation)) { _owner.ExpandCompMenu = !_owner.ExpandCompMenu; _owner.ExpireSolution(true); return GH_ObjectResponse.Handled; }
                if (_recBtnVals.Contains(e.CanvasLocation)) { _owner.ExpandValuesMenu = !_owner.ExpandValuesMenu; _owner.ExpireSolution(true); return GH_ObjectResponse.Handled; }

                // List Clicks
                if (_owner.ExpandCaseMenu && _recListCase.Contains(e.CanvasLocation))
                {
                    int idx = (int)((e.CanvasLocation.Y - _recListCase.Y) / ItemH);
                    if (idx >= 0 && idx < _owner.CaseNames.Count) { _owner.SelectedCaseIndex = idx; _owner.ExpireSolution(true); }
                    return GH_ObjectResponse.Handled;
                }

                if (_owner.ExpandCompMenu && _recListComp.Contains(e.CanvasLocation))
                {
                    int idx = (int)((e.CanvasLocation.Y - _recListComp.Y) / ItemH);
                    if (idx >= 0 && idx < _owner.ComponentNames.Count) { _owner.SelectedComponentIndex = idx; _owner.ExpireSolution(true); }
                    return GH_ObjectResponse.Handled;
                }

                // Values Menu Interactions
                if (_owner.ExpandValuesMenu && _recToggleShow.Contains(e.CanvasLocation))
                {
                    _owner.ShowValues = !_owner.ShowValues;
                    _owner.ExpireSolution(true);
                    return GH_ObjectResponse.Handled;
                }
            }
            return base.RespondToMouseUp(sender, e);
        }

        private void UpdateSliderValue(float mouseX)
        {
            float p = (mouseX - _activeSliderRec.X) / _activeSliderRec.Width;
            if (p < 0) p = 0; if (p > 1) p = 1;

            if (_activeSliderID == 1)
            {
                double max = _owner.CurrentMaxVal > 0.001 ? _owner.CurrentMaxVal : 10.0;
                _owner.ValueThreshold = 0 + (max - 0) * p;
            }
            else if (_activeSliderID == 2)
            {
                _owner.TextSize = (int)(8 + (30 - 8) * p);
            }
        }

        protected override void Layout()
        {
            base.Layout();
            float width = Math.Max(Bounds.Width, FixedWidth);
            RectangleF b = Bounds;
            float currentY = b.Bottom;

            // Bloque Case
            _recBtnCase = new RectangleF(b.X, currentY, width, HeaderH); currentY += HeaderH;
            float hCase = _owner.ExpandCaseMenu ? (_owner.CaseNames.Count * ItemH) : 0;
            _recListCase = new RectangleF(b.X, currentY, width, hCase);
            if (_owner.ExpandCaseMenu) currentY += hCase + 4;

            // Bloque Comp
            _recBtnComp = new RectangleF(b.X, currentY, width, HeaderH); currentY += HeaderH;
            float hComp = _owner.ExpandCompMenu ? (_owner.ComponentNames.Count * ItemH) : 0;
            _recListComp = new RectangleF(b.X, currentY, width, hComp);
            if (_owner.ExpandCompMenu) currentY += hComp + 4;

            // Bloque Values
            _recBtnVals = new RectangleF(b.X, currentY, width, HeaderH); currentY += HeaderH;
            if (_owner.ExpandValuesMenu)
            {
                float innerY = currentY + 5;
                _recToggleShow = new RectangleF(b.X + 5, innerY, width - 10, SliderH); innerY += SliderH + 5;
                _recSliderThres = new RectangleF(b.X + 5, innerY, width - 10, SliderH); innerY += SliderH + 2;
                _recSliderSize = new RectangleF(b.X + 5, innerY, width - 10, SliderH); innerY += SliderH + 5;
                _recListVals = new RectangleF(b.X, currentY, width, innerY - currentY); currentY = innerY;
            }
            else { _recListVals = RectangleF.Empty; _recToggleShow = RectangleF.Empty; _recSliderThres = RectangleF.Empty; _recSliderSize = RectangleF.Empty; }

            // Bloque Info
            currentY += 5;
            _recInfoText = new RectangleF(b.X, currentY, width, InfoH); currentY += InfoH;

            Bounds = new RectangleF(b.X, b.Y, width, currentY - b.Y);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);
            if (channel != GH_CanvasChannel.Objects) return;

            // CASE
            DrawHeader(graphics, _recBtnCase, _owner.ExpandCaseMenu ? "Select Case" : $"Case: {(_owner.CaseNames.Count > 0 ? _owner.CaseNames[_owner.SelectedCaseIndex] : "None")}", _owner.ExpandCaseMenu);
            if (_owner.ExpandCaseMenu)
            {
                graphics.FillRectangle(Brushes.White, _recListCase);
                StringFormat fmt = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap, Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                for (int i = 0; i < _owner.CaseNames.Count; i++)
                {
                    RectangleF r = new RectangleF(_recListCase.X, _recListCase.Y + i * ItemH, _recListCase.Width, ItemH);
                    bool sel = (i == _owner.SelectedCaseIndex);
                    graphics.FillRectangle(sel ? _selBrush : Brushes.White, r);
                    graphics.DrawRectangle(Pens.WhiteSmoke, r.X, r.Y, r.Width, r.Height);
                    RectangleF txtR = r; txtR.X += 4; txtR.Width -= 8;
                    graphics.DrawString(_owner.CaseNames[i], GH_FontServer.Standard, Brushes.Black, txtR, fmt);
                }
                graphics.DrawRectangle(_borderPen, Rectangle.Round(_recListCase));
            }

            // COMP
            DrawHeader(graphics, _recBtnComp, _owner.ExpandCompMenu ? "Select Result" : $"Show: {_owner.ComponentNames[_owner.SelectedComponentIndex]}", _owner.ExpandCompMenu);
            if (_owner.ExpandCompMenu)
            {
                graphics.FillRectangle(Brushes.White, _recListComp);
                StringFormat fmt = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap, Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                for (int i = 0; i < _owner.ComponentNames.Count; i++)
                {
                    RectangleF r = new RectangleF(_recListComp.X, _recListComp.Y + i * ItemH, _recListComp.Width, ItemH);
                    bool sel = (i == _owner.SelectedComponentIndex);
                    graphics.FillRectangle(sel ? _selBrush : Brushes.White, r);
                    graphics.DrawRectangle(Pens.WhiteSmoke, r.X, r.Y, r.Width, r.Height);
                    RectangleF txtR = r; txtR.X += 4; txtR.Width -= 8;
                    graphics.DrawString(_owner.ComponentNames[i], GH_FontServer.Standard, Brushes.Black, txtR, fmt);
                }
                graphics.DrawRectangle(_borderPen, Rectangle.Round(_recListComp));
            }

            // VALS/DISPLAY
            DrawHeader(graphics, _recBtnVals, "Display Values", _owner.ExpandValuesMenu);
            if (_owner.ExpandValuesMenu)
            {
                graphics.FillRectangle(Brushes.White, _recListVals); graphics.DrawRectangle(_borderPen, Rectangle.Round(_recListVals));
                DrawToggle(graphics, _recToggleShow, "Show Values", _owner.ShowValues);
                double max = _owner.CurrentMaxVal > 0.001 ? _owner.CurrentMaxVal : 10.0;
                DrawSlider(graphics, _recSliderThres, "Min Value", _owner.ValueThreshold, 0.00, max, "F2");
                DrawSlider(graphics, _recSliderSize, "Text Size", _owner.TextSize, 8, 30, "F0");
            }

            // INFO BOX (Max/Min en dos líneas)
            float sepY = _recInfoText.Top - 2;
            graphics.DrawLine(_borderPen, _recInfoText.X + 5, sepY, _recInfoText.Right - 5, sepY);

            RectangleF topHalf = new RectangleF(_recInfoText.X, _recInfoText.Y, _recInfoText.Width, _recInfoText.Height / 2);
            RectangleF botHalf = new RectangleF(_recInfoText.X, _recInfoText.Y + _recInfoText.Height / 2, _recInfoText.Width, _recInfoText.Height / 2);

            StringFormat cfmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            graphics.DrawString(_owner.MaxText, GH_FontServer.Standard, Brushes.Black, topHalf, cfmt);
            graphics.DrawString(_owner.MinText, GH_FontServer.Standard, Brushes.Black, botHalf, cfmt);
        }

        private void DrawHeader(Graphics g, RectangleF r, string t, bool exp)
        {
            g.FillRectangle(Brushes.Black, r); g.DrawRectangle(_borderPen, Rectangle.Round(r));
            g.DrawString(t, GH_FontServer.StandardBold, Brushes.White, r.X + 4, r.Y + 2);
            g.DrawString(exp ? "-" : "+", GH_FontServer.StandardBold, Brushes.White, r.Right - 15, r.Y + 2);
        }

        private void DrawSlider(Graphics g, RectangleF r, string n, double v, double min, double max, string f)
        {
            g.FillRectangle(Brushes.WhiteSmoke, r); g.DrawRectangle(Pens.LightGray, Rectangle.Round(r));
            float p = (float)((v - min) / (max - min));
            if (p < 0) p = 0; if (p > 1) p = 1;
            RectangleF fill = new RectangleF(r.X, r.Y, r.Width * p, r.Height);
            g.FillRectangle(Brushes.Silver, fill); g.DrawRectangle(Pens.Gray, Rectangle.Round(fill));
            g.DrawString($"{n}: {v.ToString(f)}", GH_FontServer.Standard, Brushes.Black, r.X + 4, r.Y + 2);
        }

        private void DrawToggle(Graphics g, RectangleF r, string t, bool s)
        {
            g.FillRectangle(s ? _selBrush : Brushes.WhiteSmoke, r); g.DrawRectangle(Pens.LightGray, Rectangle.Round(r));
            StringFormat fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(t, GH_FontServer.Standard, Brushes.Black, r, fmt);
        }
    }
}
