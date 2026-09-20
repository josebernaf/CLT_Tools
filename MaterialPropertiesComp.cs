using Grasshopper.Kernel;
using System;

namespace CLT_Tools
{
    public class MaterialPropertiesComp : GH_Component
    {
        public MaterialPropertiesComp()
          : base("Material Properties", "MatProp", "Defines the Orthotropic material properties.", "CLT Tools", "3 :: Material")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Family of material", "Fam", "Family of the material. Could be 'steel', 'aluminium', 'timber', 'concrete', etc.", GH_ParamAccess.item, "timber");
            pManager.AddTextParameter("Name", "Name", "Name of the material [ID].", GH_ParamAccess.item, "C24");
            pManager.AddNumberParameter("E,0", "E,0", "Young's Modulus [MPa or N/mm²] in the first material direction.", GH_ParamAccess.item, 12000);
            pManager.AddNumberParameter("E,90", "E,90", "Young's Modulus [MPa or N/mm²] in the second material direction.", GH_ParamAccess.item, 460);
            pManager.AddNumberParameter("G,xy", "G,xy", "In-plane Shear Modulus [MPa or N/mm²] in the first material direction.", GH_ParamAccess.item, 690);
            pManager.AddNumberParameter("α", "α", "Material's Poisson coefficient [Adimensional]", GH_ParamAccess.item, 0.335);
            pManager.AddNumberParameter("G,xz", "G,xz", "Transverse shear Modulus [MPa or N/mm²] of the material in the section perpendicular to the first material direction.", GH_ParamAccess.item, 690);
            pManager.AddNumberParameter("G,yz", "G,yz", "ransverse shear Modulus [MPa or N/mm²] of the material in the section perpendicular to the second material direction.", GH_ParamAccess.item, 50);
            pManager.AddNumberParameter("ρₘ", "ρₘ", "Specific weight [kN/m³] of the material.", GH_ParamAccess.item, 3.5);
            pManager.AddNumberParameter("alphaT1", "T1", "Coeficient of thermal expansion [1/ºC] in the first material direction.", GH_ParamAccess.item, 0.0000038);
            pManager.AddNumberParameter("alphaT2", "T2", "Coeficient of thermal expansion [1/ºC] in the second material direction.", GH_ParamAccess.item, 0.0000185);

            // Resistencias
            pManager.AddNumberParameter("fm,0,k", "fm,0,k", "Characteristic strength for bending [MPa or N/mm²] in the first material direction.", GH_ParamAccess.item, 24);
            pManager.AddNumberParameter("fm,90,k", "fm,90,k", "Characteristic strength for bending [MPa or N/mm²] in the second material direction.", GH_ParamAccess.item, 0); 
            pManager.AddNumberParameter("ft,0,k", "ft,0,k", "Characteristic strength for tension [MPa or N/mm²] in the first material direction.", GH_ParamAccess.item, 14.5);
            pManager.AddNumberParameter("ft,90,k", "ft,90,k", "Characteristic strength for tension [MPa or N/mm²] in the second material direction.", GH_ParamAccess.item, 0.12);
            pManager.AddNumberParameter("fc,0,k", "fc,0,k", "Characteristic strength for compresion [MPa or N/mm²] in the first material direction.", GH_ParamAccess.item, 21);
            pManager.AddNumberParameter("fc,90,k", "fc,90,k", "Characteristic strength for compresion [MPa or N/mm²] in the second material direction.", GH_ParamAccess.item, 2.5);

            // Cortantes
            pManager.AddNumberParameter("fv,xz,k", "fv,xz,k", "Characteristic strength for shear [MPa or N/mm²].", GH_ParamAccess.item, 4);
            pManager.AddNumberParameter("fv,xy,k", "fv,xy,k", "Characteristic strength for shear (FM1) [MPa or N/mm²].", GH_ParamAccess.item, 3.5);
            pManager.AddNumberParameter("fv,net,k", "fv,net,k", "Characteristic strength for shear (FM2) [MPa or N/mm²].", GH_ParamAccess.item, 3.9);
            pManager.AddNumberParameter("fv,tor,k", "fv,tor,k", "Characteristic strength for shear (FM3) [MPa or N/mm²].", GH_ParamAccess.item, 2.5);
            pManager.AddNumberParameter("fv,yz,k", "fv,yz,k", "Characteristic strength for rolling shear [MPa or N/mm²].", GH_ParamAccess.item, 3.5);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "Mat", "An orthotropic material definition.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string fam = "", name = "";
            double e0 = 0, e90 = 0, gxy = 0, nu = 0, gxz = 0, gyz = 0, gamma = 0;
            double t1 = 0, t2 = 0;
            double fm0 = 0, fm90 = 0, ft0 = 0, ft90 = 0, fc0 = 0, fc90 = 0;
            double fvxz = 0, fvxy = 0, fvnet = 0, fvtor = 0, fvyz = 0;

            if (!DA.GetData(0, ref fam)) return;
            if (!DA.GetData(1, ref name)) return;
            if (!DA.GetData(2, ref e0)) return;
            if (!DA.GetData(3, ref e90)) return;
            if (!DA.GetData(4, ref gxy)) return;
            if (!DA.GetData(5, ref nu)) return;
            if (!DA.GetData(6, ref gxz)) return;
            if (!DA.GetData(7, ref gyz)) return;
            if (!DA.GetData(8, ref gamma)) return;
            if (!DA.GetData(9, ref t1)) return;
            if (!DA.GetData(10, ref t2)) return;

            // Resistencias
            if (!DA.GetData(11, ref fm0)) return;
            if (!DA.GetData(12, ref fm90)) return;
            if (!DA.GetData(13, ref ft0)) return;
            if (!DA.GetData(14, ref ft90)) return;
            if (!DA.GetData(15, ref fc0)) return;
            if (!DA.GetData(16, ref fc90)) return;

            // Cortantes
            if (!DA.GetData(17, ref fvxz)) return;
            if (!DA.GetData(18, ref fvxy)) return;
            if (!DA.GetData(19, ref fvnet)) return;
            if (!DA.GetData(20, ref fvtor)) return;
            if (!DA.GetData(21, ref fvyz)) return;

            // Conversión Gamma (kN/m3) a Rho (kg/m3)
            // 3.5 kN/m3 * 102 ~= 357 kg/m3
            double rhoKg = gamma * 102.0;

            // --- AQUÍ ESTABA EL ERROR ---
            // Ahora pasamos 'name' como primer argumento
            CLTMaterial mat = new CLTMaterial(
                name,
                e0, e90, gxy, gxz, gyz, nu, rhoKg,
                t1, t2,
                fm0, fm90, ft0, ft90, fc0, fc90,
                fvxz, fvxy, fvnet, fvtor, fvyz
            );

            DA.SetData(0, mat);
        }

        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.Material_icon;
        public override Guid ComponentGuid => new Guid("bbbb2222-1111-1111-1111-222222222222");
    }
}