using System.Collections.Generic;
using Rhino.Geometry;

namespace CLT_Tools
{
    public enum LoadNature { Live = 0, Permanent = 1, Snow = 2, Wind = 3, Other = 4 }
    public enum LoadType { Mesh, Point, Line }
    public enum CoordinateSystem { Global, Local }

    public class CLTLoadData
    {
        public LoadType Type { get; set; }

        // --- TARGETS ---
        public Mesh TargetMesh { get; set; }
        public string TargetElementID { get; set; }
        public Point3d TargetPoint { get; set; }
        public Curve TargetCurve { get; set; }

        public Vector3d Vector { get; set; }
        public CoordinateSystem System { get; set; }
        public int LoadCase { get; set; }
        public string LoadCaseName { get; set; } = "LoadCase1";
        public LoadNature Nature { get; set; } = LoadNature.Live;

        // Constructor 1: Carga Global (Vector puro)
        public CLTLoadData(Vector3d vector)
        {
            Type = LoadType.Mesh;
            TargetMesh = null;
            TargetElementID = null;
            Vector = vector;
            System = CoordinateSystem.Global;
            LoadCase = 0;
            LoadCaseName = "LoadCase1";
        }

        // Constructor 2: Carga de Malla Específica
        public CLTLoadData(Mesh mesh, Vector3d vector, bool isLocal, int lCase)
        {
            Type = LoadType.Mesh;
            TargetMesh = mesh;
            TargetElementID = null;
            Vector = vector;
            System = isLocal ? CoordinateSystem.Local : CoordinateSystem.Global;
            LoadCase = lCase;
            LoadCaseName = "LoadCase1";
        }

        // Constructor 3: Carga Puntual
        public CLTLoadData(Point3d point, Vector3d vector, int lCase)
        {
            Type = LoadType.Point;
            TargetPoint = point;
            Vector = vector;
            System = CoordinateSystem.Global;
            LoadCase = lCase;
            LoadCaseName = "LoadCase1";
        }

        // Constructor 4: Carga Lineal
        public CLTLoadData(Curve curve, Vector3d vector, bool isLocal, int lCase)
        {
            Type = LoadType.Line;
            TargetCurve = curve;
            Vector = vector;
            System = isLocal ? CoordinateSystem.Local : CoordinateSystem.Global;
            LoadCase = lCase;
            LoadCaseName = "LoadCase1";
        }
    }

    public class SeismicData
    {
        public bool IsActive { get; set; } = false;
        public string LocationName { get; set; } = "None";
        public double ab { get; set; }
        public double K { get; set; }
        public double C { get; set; }
        public double Rho { get; set; }
        public double Mu { get; set; }
        public double Psi { get; set; }

        public override string ToString()
        {
            if (!IsActive) return "Seismic: Inactive";
            return $"Seismic: {LocationName} | ab={ab:F2}g | K={K} | C={C} | Rho={Rho} | Mu={Mu} | Psi={Psi}";
        }
    }
}