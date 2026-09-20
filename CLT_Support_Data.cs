using Rhino.Geometry;

namespace CLT_Tools
{
    public class CLTSupportData
    {
        public Point3d Location { get; set; }
        public string FixString { get; set; } // Formato paara OpenSees: "1 1 1 0 0 0"

        public CLTSupportData(Point3d loc, bool tx, bool ty, bool tz, bool rx, bool ry, bool rz)
        {
            Location = loc;
            // Convertimos los booleanos a 1 (Fijo) o 0 (Libre)
            FixString = string.Format("{0} {1} {2} {3} {4} {5}",
                tx ? 1 : 0,
                ty ? 1 : 0,
                tz ? 1 : 0,
                rx ? 1 : 0,
                ry ? 1 : 0,
                rz ? 1 : 0);
        }
    }
}