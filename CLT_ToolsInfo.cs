using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace CLT_Tools
{
    public class CLT_ToolsInfo : GH_AssemblyInfo
    {
        public override string Name => "CLT_Tools";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => Properties.Resources.Handbook_icon;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "CLT Tools: Parametric structural analysis of Cross-Laminated Timber (CLT) panels using OpenSees.";

        public override Guid Id => new Guid("ccd4e75c-dbff-42f2-b7b5-ea19ebe5c1f2");

        //Return a string identifying you or your company.
        public override string AuthorName => "Jose Francisco Berná Falcó";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "Jose Francisco Berná Falcó, Arquitecto - MArqUA (Universidad de Alicante)";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    }
}