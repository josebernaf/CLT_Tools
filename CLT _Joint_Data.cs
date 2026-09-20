using System;
using Rhino.Geometry;

namespace CLT_Tools
{
    // 1. Enumerador con todos los tipos de fijación según CTE DB-SE-M (Tabla 7.2)
    public enum FastenerType
    {
        Tirafondo_Perno_Pasador = 0,
        Clavo_SinPretaladro = 1,
        Grapa = 2,
        Conector_Placa_Anillo = 3,         // Tipo A y B
        Conector_Dentado_DobleCara = 4,    // Tipo C10
        Conector_Dentado_UnaCara = 5,      // Tipo C1 a C9, C11
        Manual_Catalogo = 6                // Para introducir Kser de fabricante
    }

    // 2. Clase que almacena y calcula las propiedades mecánicas de la unión
    public class CLTJointProperties
    {
        public FastenerType Type { get; set; }
        // Para fijaciones normales es 'd', para conectores es 'dc'
        public double Diameter_mm { get; set; }
        public double Spacing_mm { get; set; }
        public double DensityMean_kgm3 { get; set; } // rho_m según CTE

        // Valores calculados
        public double Kser_PerFastener { get; private set; } // N/mm por ud.
        public double Ku_PerFastener { get; private set; }   // N/mm por ud.
        public double Kser_PerMeter { get; private set; }    // N/mm por metro lineal

        public CLTJointProperties(FastenerType type, double diameter, double spacing, double densityMean, double customKser = 0)
        {
            Type = type;
            Diameter_mm = diameter;
            Spacing_mm = spacing;
            DensityMean_kgm3 = densityMean;

            CalculateStiffness(customKser);
        }

        private void CalculateStiffness(double customKser)
        {
            if (Type == FastenerType.Manual_Catalogo)
            {
                Kser_PerFastener = customKser;
            }
            else if (Type == FastenerType.Tirafondo_Perno_Pasador)
            {
                Kser_PerFastener = (Math.Pow(DensityMean_kgm3, 1.5) * Diameter_mm) / 23.0;
            }
            else if (Type == FastenerType.Clavo_SinPretaladro)
            {
                Kser_PerFastener = (Math.Pow(DensityMean_kgm3, 1.5) * Math.Pow(Diameter_mm, 0.8)) / 30.0;
            }
            else if (Type == FastenerType.Grapa)
            {
                Kser_PerFastener = (Math.Pow(DensityMean_kgm3, 1.5) * Math.Pow(Diameter_mm, 0.8)) / 80.0;
            }
            else if (Type == FastenerType.Conector_Placa_Anillo || Type == FastenerType.Conector_Dentado_DobleCara)
            {
                Kser_PerFastener = (DensityMean_kgm3 * Diameter_mm) / 2.0;
            }
            else if (Type == FastenerType.Conector_Dentado_UnaCara)
            {
                Kser_PerFastener = (DensityMean_kgm3 * Diameter_mm) / 4.0;
            }

            // CTE DB-SE-M: Ku = 2/3 * Kser (Deslizamiento último)
            Ku_PerFastener = Kser_PerFastener * (2.0 / 3.0);

            // Rigidez equivalente por metro lineal de junta
            if (Spacing_mm > 0)
            {
                double fastenersPerMeter = 1000.0 / Spacing_mm;
                Kser_PerMeter = Kser_PerFastener * fastenersPerMeter;
            }
            else
            {
                Kser_PerMeter = 0; // Evitar división por cero
            }
        }
    }

    // 3. Clase final que viajará hacia AssembleModel
    public class CLTJointData
    {
        public Curve TargetLine { get; set; }
        public CLTJointProperties Properties { get; set; }

        public CLTJointData(Curve target, CLTJointProperties props)
        {
            TargetLine = target;
            Properties = props;
        }
    }
}