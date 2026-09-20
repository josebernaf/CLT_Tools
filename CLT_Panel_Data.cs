using System.Collections.Generic;
using Rhino.Geometry;

namespace CLT_Tools
{
    public class CLTPanelData
    {
        public List<double> Thicknesses { get; set; }
        public List<double> Angles { get; set; }
        public string ID { get; set; }
        public Vector3d FiberDirection { get; set; }

        public CLTPanelData(List<double> ths, List<double> angs, string id, Vector3d fiberDir)
        {
            Thicknesses = ths;
            Angles = angs;
            ID = id;
            FiberDirection = fiberDir;
        }
    }
}