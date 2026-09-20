using System.Collections.Generic;
using Rhino.Geometry;
using System.Linq;

namespace CLT_Tools
{
    public class CLTModel
    {
        public List<CLTElement> Elements { get; set; }
        public List<CLTLoadData> Loads { get; set; }
        public List<CLTSupportData> Supports { get; set; }

        // NUEVO: Lista de Uniones (Joints)
        public List<CLTJointData> Joints { get; set; }

        public CLTModel()
        {
            Elements = new List<CLTElement>();
            Loads = new List<CLTLoadData>();
            Supports = new List<CLTSupportData>();
            Joints = new List<CLTJointData>();
        }

        // Constructor
        public CLTModel(List<CLTElement> elems, List<CLTLoadData> loads, List<CLTSupportData> sups, List<CLTJointData> joints)
        {
            Elements = elems ?? new List<CLTElement>();
            Loads = loads ?? new List<CLTLoadData>();
            Supports = sups ?? new List<CLTSupportData>();
            Joints = joints ?? new List<CLTJointData>();
        }

        // --- CÁLCULOS FÍSICOS ---

        public double CalculateTotalMass()
        {
            double totalMassKg = 0;
            foreach (var el in Elements)
            {
                if (el.Geometry == null || !el.Geometry.IsValid) continue;

                // 1. Calcular Volumen
                // Area (mm2) * Espesor Total (mm) = Volumen (mm3)
                double areaMm2 = AreaMassProperties.Compute(el.Geometry).Area;
                double thicknessMm = el.Properties.Thicknesses.Sum();
                double volMm3 = areaMm2 * thicknessMm;

                // 2. Convertir a m3 (1 mm3 = 1e-9 m3)
                double volM3 = volMm3 * 1e-9;

                // 3. Masa = Vol * Densidad (kg/m3)
                // Usamos la propiedad Rho del material (que ya convertimos a kg/m3 en el componente de material)
                totalMassKg += volM3 * el.Material.Rho;
            }
            return totalMassKg;
        }

        public Point3d CalculateCenterOfGravity()
        {
            double totalMass = 0;
            double sumX = 0, sumY = 0, sumZ = 0;

            foreach (var el in Elements)
            {
                if (el.Geometry == null || !el.Geometry.IsValid) continue;

                // Masa del elemento individual
                double areaMm2 = AreaMassProperties.Compute(el.Geometry).Area;
                double thicknessMm = el.Properties.Thicknesses.Sum();
                double mass = (areaMm2 * thicknessMm * 1e-9) * el.Material.Rho;

                // Centroide del elemento
                Point3d center = el.Geometry.GetBoundingBox(true).Center;

                // Acumular momentos
                sumX += center.X * mass;
                sumY += center.Y * mass;
                sumZ += center.Z * mass;
                totalMass += mass;
            }

            if (totalMass <= 0.0001) return Point3d.Origin;

            return new Point3d(sumX / totalMass, sumY / totalMass, sumZ / totalMass);
        }
    }
}