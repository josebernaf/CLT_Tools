using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CLT_Tools
{
    public class LineJointComp : GH_Component
    {
        public LineJointComp()
          : base("Line Joint", "LJoint",
              "Define una unión lineal entre paneles CLT, calculando la rigidez al deslizamiento (Kser y Ku) según el CTE DB-SE-M.",
              "CLT Tools", "1 :: Model")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Line(s)", "L", "Líneas geométricas donde se produce la unión entre paneles.", GH_ParamAccess.list);

            // Descripción detallada para que el usuario sepa qué número poner (o conectar un Value List)
            pManager.AddIntegerParameter("Type", "T",
                "Tipo de fijación (según CTE DB-SE-M):\n" +
                "0 = Tirafondos, Pernos, Pasadores\n" +
                "1 = Clavos sin pretaladro\n" +
                "2 = Grapas\n" +
                "3 = Conector Placa / Anillo\n" +
                "4 = Conector Dentado (Doble cara)\n" +
                "5 = Conector Dentado (Una cara)\n" +
                "6 = Manual (Catálogo)", GH_ParamAccess.item, 0);

            pManager.AddNumberParameter("Diameter (d)", "D", "Diámetro de la fijación o del conector [mm].", GH_ParamAccess.item, 8.0);
            pManager.AddNumberParameter("Spacing (s)", "S", "Separación entre fijaciones [mm].", GH_ParamAccess.item, 150.0);
            pManager.AddNumberParameter("Density (ρₘ)", "Rho", "Densidad media de la madera [kg/m³] (ej. 380 para C24).", GH_ParamAccess.item, 380.0);
            pManager.AddNumberParameter("Manual Kser", "Kser", "Rigidez distribuida de la union [N/mm / m lineal]. Valor directamente del catalogo del fabricante. Solo se usa si Type = 6.", GH_ParamAccess.item, 0.0);

            // Hacer que todos los parámetros menos la curva sean opcionales (tienen valores por defecto)
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Joint Data", "J", "Objeto de unión CLT para conectar al componente AssembleModel.", GH_ParamAccess.list);
            pManager.AddTextParameter("Info", "I", "Reporte analítico de las rigideces calculadas.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 1. Declarar variables
            List<Curve> lines = new List<Curve>();
            int typeInt = 0;
            double diameter = 8.0;
            double spacing = 150.0;
            double density = 380.0;
            double manualKser = 0.0;

            // 2. Obtener datos
            if (!DA.GetDataList(0, lines)) return;
            DA.GetData(1, ref typeInt);
            DA.GetData(2, ref diameter);
            DA.GetData(3, ref spacing);
            DA.GetData(4, ref density);
            DA.GetData(5, ref manualKser);

            // 3. Mapear el número entero al enumerador del CTE
            FastenerType type = FastenerType.Tirafondo_Perno_Pasador;
            if (Enum.IsDefined(typeof(FastenerType), typeInt))
            {
                type = (FastenerType)typeInt;
            }

            // 4. Calcular propiedades con las fórmulas del CTE DB-SE-M
            CLTJointProperties props = new CLTJointProperties(type, diameter, spacing, density, manualKser);

            // 5. Asignar las propiedades a cada línea de entrada
            List<CLTJointData> jointDataList = new List<CLTJointData>();
            foreach (Curve crv in lines)
            {
                if (crv != null && crv.IsValid)
                {
                    jointDataList.Add(new CLTJointData(crv, props));
                }
            }

            // 6. Formatear el reporte de texto (Output 'Info')
            string info = $"=== CTE DB-SE-M JOINT STIFFNESS ===\n";
            info += $"Type: {type.ToString().Replace("_", " ")}\n";
            info += $"Diameter: {diameter} mm\n";
            info += $"Density (Rho_m): {density} kg/m³\n";
            info += $"Spacing: {spacing} mm\n";
            info += $"-----------------------------------\n";
            info += $"Kser (per fastener): {props.Kser_PerFastener:F2} N/mm\n";
            info += $"Ku (per fastener):   {props.Ku_PerFastener:F2} N/mm\n";
            info += $"Kser (DISTRIBUTED):  {props.Kser_PerMeter:F2} (N/mm)/m\n";
            info += $"===================================";

            // 7. Salidas
            DA.SetDataList(0, jointDataList);
            DA.SetData(1, info);

            // Mensaje flotante debajo del componente en el Canvas
            this.Message = $"{props.Kser_PerMeter:F0} N/mm/m";
        }

        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.LineJoint_icon;
        public override Guid ComponentGuid => new Guid("c1b2a3d4-5566-7788-9900-aabbccddeeff");
    }
}