using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types; // Necesario para IGH_Goo
using Rhino.Geometry;

namespace CLT_Tools
{
    public class UniformLoadComp : GH_Component
    {
        public UniformLoadComp()
          : base("UniformLoad", "LoadU", "Carga uniforme. Conecta Malla o Texto (ID) en el input 'Geo'.", "CLT Tools", "2 :: Loads")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector", "Vec", "Input surface-load vector [kN/m²].", GH_ParamAccess.item, new Vector3d(0, 0, -1));
            pManager.AddGenericParameter("Target", "Tar", "Connect a Mesh to apply load directly, or a Panel with an Element ID string.", GH_ParamAccess.item);
            pManager.AddTextParameter("LCase", "LC", "Name of load-case (e.g. 'Wx', 'Live').", GH_ParamAccess.item, "LoadCase1");
            pManager.AddIntegerParameter("Nature", "Nat", "Seismic Category:\n0 = Live\n1 = Permanent\n2 = Snow\n3 = Wind", GH_ParamAccess.item, 0);

            pManager[1].Optional = true; // Puede ser carga global si está vacío
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load", "L", "Load object.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Vector3d vec = Vector3d.ZAxis;
            IGH_Goo targetGoo = null;
            string lCaseName = "LoadCase1";
            int natureInt = 0; // Default: Live

            if (!DA.GetData(0, ref vec)) return;
            DA.GetData(1, ref targetGoo);
            DA.GetData(2, ref lCaseName);
            DA.GetData(3, ref natureInt);

            // Convertir kN/m2 -> N/mm2 (x1000)
            Vector3d pressureN = vec * 0.001;

            // Crear carga base
            CLTLoadData load = new CLTLoadData(pressureN);
            load.LoadCaseName = lCaseName;

            // --- LÓGICA DE TARGET HÍBRIDO ---
            if (targetGoo != null)
            {
                // Opción A: Es una Malla (Geometría)
                if (targetGoo.ScriptVariable() is Mesh m)
                {
                    load.TargetMesh = m;
                    load.Type = LoadType.Mesh;
                }
                // Opción B: Es un Texto (ID del Elemento)
                else if (GH_Convert.ToString(targetGoo, out string id, GH_Conversion.Both))
                {
                    // Evitar strings vacíos o nulos
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        load.TargetElementID = id;
                        load.Type = LoadType.Mesh; // Sigue siendo tipo Mesh conceptualmente
                    }
                }
            }
            // Si targetGoo es null, se queda como carga Global (TargetMesh = null)

            // Asignar Naturaleza
            if (Enum.IsDefined(typeof(LoadNature), natureInt))
                load.Nature = (LoadNature)natureInt;
            else
                load.Nature = LoadNature.Live;

            DA.SetData(0, load);
        }

        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.UniformLoad_icon;
        public override Guid ComponentGuid => new Guid("aaaa5555-1111-2222-3333-555555555555");
    }
}