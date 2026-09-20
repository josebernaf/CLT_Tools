using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CLT_Tools
{
    public class CLTHandbookComponent : GH_Component
    {
        public CLTHandbookComponent()
          : base("CLT Handbook", "Info",
              "Plugin Information (License, Support info, etc.).",
              "CLT Tools", "0 :: Info") 
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Chapter", "C", "Select handbook chapter to see the info.", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Information", "Info", "Tecnichal data", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int category = 0;
            if (!DA.GetData(0, ref category)) return;

            StringBuilder sb = new StringBuilder();

            switch (category)
            {
                
                case 1: // FABRICANTES Y DIMENSIONES
                    sb.AppendLine("--- DIMENSIONES DE FABRICANTES ---");
                    sb.AppendLine("Fuente: Catálogos técnicos");
                    sb.AppendLine("");
                    sb.AppendLine("1. STORA ENSO");
                    sb.AppendLine("Muros   Largo: 16.00 m máx.                  Losas   Largo: 16.00 m máx.");
                    sb.AppendLine("        Ancho: 2.95 m máx.                           Ancho: 0.60 - 3.20 m");
                    sb.AppendLine("        Espesores: 20 - 40 mm                        Espesores: 20 - 40 mm");
                    sb.AppendLine("        Nº de Capas: 3 - 5                           Nº de Capas: 3 - 5 - 7 - 8");
                    sb.AppendLine("");
                    sb.AppendLine("2. KLH");
                    sb.AppendLine("Muros   Largo: 16.50 m máx.                  Losas   Largo: 16.50 m máx.");
                    sb.AppendLine("        Ancho máx: 2.40 - 2.95 m                     Ancho: 2.40 - 2.95 m");
                    sb.AppendLine("        Espesores: 20 - 30 - 35 40 mm                Espesores: 20 - 40 mm");
                    sb.AppendLine("        Nº de Capas: 3 - 5                           Nº de Capas: 3 - 5 - 7");
                    sb.AppendLine("");
                    sb.AppendLine("3. EGOIN");
                    sb.AppendLine("Muros   Largo: 17.50 m máx.                  Losas   Largo: 17.50 m máx.");
                    sb.AppendLine("        Ancho: 3.80 m máx.                           Ancho: 3.80 m máx.");
                    sb.AppendLine("        Espesores: 20 - 25 - 30 - 40 mm              Espesores: 20 - 25 - 30 - 40 mm");
                    sb.AppendLine("        Nº de Capas: 3 - 5 - 7 - 9                   Nº de Capas: 3 - 5 - 7 - 9");
                    sb.AppendLine("");
                    sb.AppendLine("4. BINDERHOLZ");
                    sb.AppendLine("Muros   Largo: 22.00 m máx.                  Losas   Largo: 20.00 m máx.");
                    sb.AppendLine("        Ancho: 3.50 m máx.                           Ancho: 1.25 m máx.");
                    sb.AppendLine("        Espesores: 20 - 30 - 35 - 40 mm              Espesores: 20 - 30 - 35 - 40 mm");
                    sb.AppendLine("        Nº de Capas: 3 - 5                           Nº de Capas: 3 - 5 - 7");

                    break;

                case 2: // NORMATIVA CTE
                    sb.AppendLine("--- NORMATIVA (CTE DB-SE) ---");
                    sb.AppendLine("");
                    sb.AppendLine("COEFICIENTES PARCIALES SEGURIDAD (ELU):");
                    sb.AppendLine("Carga Permanente (G) = 1.35");
                    sb.AppendLine("Carga Variable (Q)   = 1.50");
                    sb.AppendLine("");
                    sb.AppendLine("COEFICIENTES SIMULTANEIDAD (Psi):");
                    sb.AppendLine("Viento (W):    Psi0=0.6, Psi1=0.5, Psi2=0.0");
                    sb.AppendLine("Uso Res. (A):  Psi0=0.7, Psi1=0.5, Psi2=0.3");
                    sb.AppendLine("");
                    sb.AppendLine("SEGURIDAD MATERIAL (Madera):");
                    sb.AppendLine("k_mod (Clase Servicio 1/2, Corto Plazo) = 0.80 - 0.90");
                    sb.AppendLine("Gamma_M (Madera Laminada/CLT) = 1.25");
                    sb.AppendLine("");
                    sb.AppendLine("");
                    sb.AppendLine("--- ESTADOS LÍMITE DE SERVICIO (ELS) ---");
                    sb.AppendLine("");
                    sb.AppendLine("FLECHA MÁXIMA ADMISIBLE (CTE):");
                    sb.AppendLine("Total a plazo infinito: L/400");
                    sb.AppendLine("Activa (confort):       L/500");
                    sb.AppendLine("");
                    sb.AppendLine("DESPLOME GLOBAL (CTBUH / Práctica):");
                    sb.AppendLine("H/500 (Límite estricto)");
                    break;

                
                case 3: // LICENSE INFO
                    sb.AppendLine("--- NON-COMMERCIAL LICENSE ---");
                    sb.AppendLine("");
                    sb.AppendLine("This plugin is restricted to educational uses only.");
                    sb.AppendLine("");
                    sb.AppendLine("For commercial purposes contact jfbf2@alu.ua.es to see if commercial licenses are available.");
                    sb.AppendLine("");
                    sb.AppendLine("This plugin uses the open source solver 'OpenSees' to sove the calculations, so in order for the plugin to work you need to install the last version of OpenSees (last compatible version is 3.7.1). OpenSees folder must be saved on the directory 'C:\\OpenSees' so that the plugin can interact with the program.");
                    sb.AppendLine("");
                    sb.AppendLine("OpenSees download link: https://github.com/OpenSees/OpenSees/releases/tag/v3.7.1");
                    sb.AppendLine("");
                    sb.AppendLine("Thanks for ussing CLT_Tools :)");
                    sb.AppendLine("");
                    break;

                default:
                    sb.AppendLine("Select a correct cattegory 1-3.");
                    break;
            }

            DA.SetData(0, sb.ToString());
        }


        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.Handbook_icon;
        public override Guid ComponentGuid => new Guid("abcd1234-1111-2222-3333-999999999999");
    }
}