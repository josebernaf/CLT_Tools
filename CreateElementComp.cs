using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace CLT_Tools
{
    public class CreateElementComp : GH_Component
    {
        public CreateElementComp()
          : base("CreateElement", "Elem", "Creates the CLT element.", "CLT Tools", "1 :: Model")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "If several meshes, they will be connected if their nodes meet unless joint(s) is defined.", GH_ParamAccess.list);
            pManager.AddTextParameter("Identifier", "Id", "Identifier of the shell.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Material / Name", "Mat", "Base material.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Layers Thickness", "LThs", "Thickness of the layers that conform the panel element.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Layers Orientation", "LOri", "Direction of the different layers that conform the panel.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Main Orientation", "MOri", "Direction of the principal fiber as a vector.", GH_ParamAccess.item, Vector3d.ZAxis);

            pManager[1].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Element(s)", "Elem", "Shell element.", GH_ParamAccess.list);
            pManager.AddTextParameter("Info", "Info", "Identifier of the shell.", GH_ParamAccess.list);
            pManager.AddPointParameter("Center", "Pt", "Geometrical center points of each panel as a list.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Main Face", "Norm", "Normal vector to the main face. Start of the vector is the first layer and so on.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> meshes = new List<Mesh>();
            List<string> ids = new List<string>();

            // Leemos como IGH_Goo (Genérico) para inspeccionar qué es
            IGH_Goo matInputGoo = null;

            List<double> ths = new List<double>();
            List<double> angs = new List<double>();
            Vector3d fiberDir = Vector3d.ZAxis;

            if (!DA.GetDataList(0, meshes)) return;
            DA.GetDataList(1, ids);
            if (!DA.GetData(2, ref matInputGoo)) return; // Obtenemos el input genérico
            if (!DA.GetDataList(3, ths)) return;
            DA.GetDataList(4, angs);
            DA.GetData(5, ref fiberDir);

            // --- LÓGICA DE DETECCIÓN DE TIPO ---
            object processedMatInput = "Default";

            // Opción A: Es un Material Real (envuelto en GH_ObjectWrapper o directo)
            if (matInputGoo.ScriptVariable() is CLTMaterial matObj)
            {
                processedMatInput = matObj;
            }
            // Opción B: Es un Texto (GH_String)
            else if (GH_Convert.ToString(matInputGoo, out string matName, GH_Conversion.Both))
            {
                processedMatInput = matName;
            }

            // Rellenar ángulos
            if (angs.Count == 0) for (int i = 0; i < ths.Count; i++) angs.Add((i % 2 == 0) ? 0.0 : 90.0);
            else while (angs.Count < ths.Count) angs.Add(0.0);

            List<CLTElement> outElements = new List<CLTElement>();
            List<string> outIds = new List<string>();
            List<Point3d> outCenters = new List<Point3d>();
            List<Vector3d> outNormals = new List<Vector3d>();

            for (int i = 0; i < meshes.Count; i++)
            {
                Mesh m = meshes[i];
                if (m == null || !m.IsValid) continue;

                string currentId = (i < ids.Count) ? ids[i] : $"Shell_{i + 1}";
                CLTPanelData panelData = new CLTPanelData(ths, angs, currentId, fiberDir);

                // Pasamos el objeto procesado (String o Material) al constructor inteligente
                CLTElement element = new CLTElement(m, processedMatInput, panelData);

                BoundingBox bbox = m.GetBoundingBox(true);
                m.FaceNormals.ComputeFaceNormals();
                Vector3d normal = (m.FaceNormals.Count > 0) ? m.FaceNormals[0] : Vector3d.ZAxis;

                outElements.Add(element);
                outIds.Add(currentId);
                outCenters.Add(bbox.Center);
                outNormals.Add(normal);
            }

            DA.SetDataList(0, outElements);
            DA.SetDataList(1, outIds);
            DA.SetDataList(2, outCenters);
            DA.SetDataList(3, outNormals);
        }

        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.CreateElement_icon;
        public override Guid ComponentGuid => new Guid("eeee4444-1111-2222-3333-444444444444");
    }
}