using Rhino.Geometry;

namespace CLT_Tools
{
    public class CLTElement
    {
        public Mesh Geometry { get; set; }
        public string MaterialName { get; set; } // Nombre para búsqueda
        public CLTMaterial Material { get; set; } // Objeto real (si existe)
        public CLTPanelData Properties { get; set; }

        // Constructor
        public CLTElement(Mesh mesh, object matInput, CLTPanelData props)
        {
            Geometry = mesh;
            Properties = props;

            // Lógica Híbrida
            if (matInput is CLTMaterial matObj)
            {
                // CASO A: El input es el COMPONENTE material
                Material = matObj;
                MaterialName = matObj.Name; // Usamos el nombre del objeto
            }
            else if (matInput is string matName)
            {
                // CASO B: El input es un TEXTO
                Material = null; // Pendiente de asignar en Assemble
                MaterialName = matName;
            }
            else
            {
                // CASO C: Fallback
                Material = null;
                MaterialName = "Default";
            }
        }
    }
}