using System;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CLT_Tools
{
    public class LoadLineComp : GH_Component
    {
        public LoadLineComp() : base("Load Line", "LoadLn", "Carga lineal", "CLT Tools", "2 :: Loads") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector", "Vec", "Input line-load vector [kN/m].", GH_ParamAccess.item, new Vector3d(0, 0, -1000.0));
            pManager.AddCurveParameter("Curve", "Crv", "Geometrical curve defining the line load.", GH_ParamAccess.item);
            pManager.AddTextParameter("Target", "Tar", "Connect a Mesh to apply load directly, or a Panel with an Element ID string.", GH_ParamAccess.item);
            pManager.AddTextParameter("LCase", "LC", "Name of load-case (e.g. 'PP', 'Live').", GH_ParamAccess.item, "LoadCase1");
            pManager.AddIntegerParameter("Nature", "Nat", "Load Category:\n0 = Live\n1 = Permanent\n2 = Snow\n3 = Wind", GH_ParamAccess.item, 0);

            pManager[2].Optional = true; // El filtro por ID es opcional
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Load", "L", "Carga", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Vector3d dir = Vector3d.ZAxis;
            Curve crv = null;
            string targetID = null;
            string lCaseName = "LoadCase1";
            int natureInt = 0;

            // 1. Lectura de datos
            if (!DA.GetData(0, ref dir)) return;
            if (!DA.GetData(1, ref crv)) return;
            DA.GetData(2, ref targetID);
            DA.GetData(3, ref lCaseName);
            DA.GetData(4, ref natureInt);

            // 2. Creación de la carga lineal base
            CLTLoadData load = new CLTLoadData(crv, dir, false, 0);

            // 3. Asignación de propiedades unificadas
            load.Type = LoadType.Line;
            load.LoadCaseName = lCaseName;

            // Si el usuario ha escrito un ID, se lo asignamos para que actúe de filtro en el Assemble
            if (!string.IsNullOrWhiteSpace(targetID))
            {
                load.TargetElementID = targetID;
            }

            // 4. Asignación de la Naturaleza (Sismo/Viento/Nieve)
            if (Enum.IsDefined(typeof(LoadNature), natureInt))
                load.Nature = (LoadNature)natureInt;
            else
                load.Nature = LoadNature.Live;

            // Enviamos el paquete de datos unificado a la salida
            DA.SetData(0, load);
        }
        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.LineLoad_icon;
        public override Guid ComponentGuid => new Guid("cccc5555-1111-2222-3333-444444444444");
    }
}