namespace CLT_Tools
{
    public class CLTMaterial
    {
        public string Name { get; set; } // Propiedad para el Assemble

        public double E0 { get; set; }
        public double E90 { get; set; }
        public double Gxy { get; set; }
        public double Gxz { get; set; }
        public double Gyz { get; set; }
        public double Nu12 { get; set; }
        public double Rho { get; set; } // kg/m3

        public double AlphaT1 { get; set; }
        public double AlphaT2 { get; set; }

        public double Fm0k { get; set; }
        public double Fm90k { get; set; }
        public double Ft0k { get; set; }
        public double Ft90k { get; set; }
        public double Fc0k { get; set; }
        public double Fc90k { get; set; }

        public double Fvxzk { get; set; }
        public double Fvxyk { get; set; }
        public double Fvnetk { get; set; }
        public double Fvtork { get; set; }
        public double Fvyzk { get; set; }

        // Constructor que pide 'name' PRIMERO
        public CLTMaterial(
            string name,
            double e0, double e90, double gxy, double gxz, double gyz, double nu12, double rho,
            double alphaT1, double alphaT2,
            double fm0, double fm90, double ft0, double ft90, double fc0, double fc90,
            double fvxz, double fvxy, double fvnet, double fvtor, double fvyz
        )
        {
            Name = name;
            E0 = e0; E90 = e90; Gxy = gxy; Gxz = gxz; Gyz = gyz; Nu12 = nu12; Rho = rho;
            AlphaT1 = alphaT1; AlphaT2 = alphaT2;
            Fm0k = fm0; Fm90k = fm90; Ft0k = ft0; Ft90k = ft90; Fc0k = fc0; Fc90k = fc90;
            Fvxzk = fvxz; Fvxyk = fvxy; Fvnetk = fvnet; Fvtork = fvtor; Fvyzk = fvyz;
        }
    }
}