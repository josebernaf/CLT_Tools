using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace CLT_Tools
{
    public class SeismicActivityComp : GH_Component
    {
        public SeismicActivityComp()
          : base("Seismic Activity", "Seismic",
              "Define NCSE-02 parameters. Connect a City Name (Text) to auto-load 'ab' and 'K' from the Database, or a Number to define 'ab' manually.",
              "CLT Tools", "2 :: Loads")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Location / ab", "Loc", "City Name (Text) OR Acceleration ab (Number).", GH_ParamAccess.item);
            pManager.AddNumberParameter("Coeff K (Manual)", "K", "Contribution Coefficient. Only used if 'Loc' is a number.", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("Soil Type (C)", "Soil", "1:Roca (C=1.0)\n2:Duro (C=1.3)\n3:Blando (C=1.6)\n4:Muy Blando (C=2.0)", GH_ParamAccess.item, 2);
            pManager.AddBooleanParameter("Special Importance", "Imp", "True for Hospitals, Fire Stations, etc. (Rho = 1.3)", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Ductility (μ)", "μ", "Conduct Coefficient. CLT Standard = 2.0", GH_ParamAccess.item, 2.0);
            pManager.AddIntegerParameter("Building Use", "Use", "Defines Psi factor for Live Loads:\n0:Roof (0.0)\n1:Residential (0.3)\n2:Office (0.3)\n3:Public (0.6)\n4:Commerce (0.6)", GH_ParamAccess.item, 1);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Seismic Data", "SData", "Seismic Data package for the Solver.", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "Info", "Readable summary of the seismic parameters.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_Goo inputGoo = null;
            double manualK = 1.0;
            int soilType = 2;
            bool isSpecial = false;
            double mu = 2.0;
            int useType = 1;

            if (!DA.GetData(0, ref inputGoo)) return;
            DA.GetData(1, ref manualK);
            DA.GetData(2, ref soilType);
            DA.GetData(3, ref isSpecial);
            DA.GetData(4, ref mu);
            DA.GetData(5, ref useType);

            SeismicData data = new SeismicData();
            data.IsActive = true;
            data.Rho = isSpecial ? 1.3 : 1.0;

            // --- CORRECCIÓN AQUÍ: Usar .Mu en lugar de .Ductility ---
            data.Mu = mu;

            // Coeficiente C (Terreno)
            switch (soilType)
            {
                case 1: data.C = 1.0; break;
                case 2: data.C = 1.3; break;
                case 3: data.C = 1.6; break;
                case 4: data.C = 2.0; break;
                default: data.C = 1.3; break;
            }

            // Psi (Uso)
            switch (useType)
            {
                case 0: data.Psi = 0.0; break;
                case 1: data.Psi = 0.3; break;
                case 2: data.Psi = 0.3; break;
                case 3: data.Psi = 0.6; break;
                case 4: data.Psi = 0.6; break;
                default: data.Psi = 0.3; break;
            }

            // Lógica de Localización (Base de Datos)
            if (inputGoo.ScriptVariable() is double val)
            {
                data.ab = val;
                data.K = manualK;
                data.LocationName = $"Manual ({val}g)";
            }
            else if (GH_Convert.ToString(inputGoo, out string cityInput, GH_Conversion.Both))
            {
                if (NCSE_Database.TryGetCityData(cityInput, out double db_ab, out double db_K))
                {
                    data.ab = db_ab;
                    data.K = db_K;
                    data.LocationName = char.ToUpper(cityInput[0]) + cityInput.Substring(1).ToLower();
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"City '{cityInput}' not found in NCSE-02 Annex 1. Using default minimum ab=0.04g.");
                    data.ab = 0.04;
                    data.K = 1.0;
                    data.LocationName = $"{cityInput} (Default)";
                }
            }

            DA.SetData(0, data);
            DA.SetData(1, data.ToString());
        }

        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.SeismicData_icon;
        public override Guid ComponentGuid => new Guid("77779999-1111-2222-3333-666666666666");
    }
}