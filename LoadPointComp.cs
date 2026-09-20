using System;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CLT_Tools
{
    public class LoadPointComp : GH_Component
    {
        public LoadPointComp() : base("Load Point", "LoadPt", "Carga puntual", "CLT Tools", "2 :: Loads") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Vector", "Vec", "Input point-load vector [kN].", GH_ParamAccess.item, new Vector3d(0, 0, -10.0));
            pManager.AddPointParameter("Point", "Pt", "Geometrical point where the load is applied.", GH_ParamAccess.item);
            pManager.AddTextParameter("Target ID", "Tar", "Connect a Mesh to apply load directly, or a Panel with an Element ID string.", GH_ParamAccess.item);
            pManager.AddTextParameter("LCase", "LC", "Name of load-case (e.g. 'PP', 'Live').", GH_ParamAccess.item, "LoadCase1");
            pManager.AddIntegerParameter("Nature", "Nat", "Load Category:\n0 = Live\n1 = Permanent\n2 = Snow\n3 = Wind", GH_ParamAccess.item, 0);

            pManager[2].Optional = true;
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
            Point3d pt = Point3d.Origin;
            string targetID = null;
            string lCaseName = "LoadCase1";
            int natureInt = 0;

            // 1. Lectura de datos
            if (!DA.GetData(0, ref dir)) return;
            if (!DA.GetData(1, ref pt)) return;
            DA.GetData(2, ref targetID);
            DA.GetData(3, ref lCaseName);
            DA.GetData(4, ref natureInt);

            // 2. Creación de la carga puntual base
            CLTLoadData load = new CLTLoadData(dir);
            load.TargetPoint = pt;

            // 3. Asignación de propiedades unificadas
            load.Type = LoadType.Point;
            load.LoadCaseName = lCaseName;

            // Asignar el ID si el usuario quiere restringir la carga a un panel concreto
            if (!string.IsNullOrWhiteSpace(targetID))
            {
                load.TargetElementID = targetID;
            }

            // 4. Asignación de la Naturaleza (Sismo/Viento/Nieve)
            if (Enum.IsDefined(typeof(LoadNature), natureInt))
                load.Nature = (LoadNature)natureInt;
            else
                load.Nature = LoadNature.Live;

            DA.SetData(0, load);
        }
        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.PointLoad_icon;
        public override Guid ComponentGuid => new Guid("bbbb5555-1111-2222-3333-444444444444");
    }
}