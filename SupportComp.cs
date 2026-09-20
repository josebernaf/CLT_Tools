using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CLT_Tools
{
    public class SupportComp : GH_Component
    {
        // 1. ESTADO INTERNO (Las variables que guardan si el botón está activado)
        public bool Tx { get; set; } = true;
        public bool Ty { get; set; } = true;
        public bool Tz { get; set; } = true;
        public bool Rx { get; set; } = true;
        public bool Ry { get; set; } = true;
        public bool Rz { get; set; } = true;

        public SupportComp()
          : base("Support", "Supp", "Constrains active when orange.", "CLT Tools", "1 :: Model")
        {
        }

        // 2. VINCULAR CON LOS ATRIBUTOS VISUALES PERSONALIZADOS
        public override void CreateAttributes()
        {
            m_attributes = new SupportAttributes(this);
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "Input indexes or points.", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Plane", "Pln", "Plane for orienting the support. By default, supports ar defined using the global coordinate system.", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddNumberParameter("Cₜ", "Ct", "Translational spring stiffness [kN/m] at the support. By deafult, value is 0.", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Cᵣ", "Cr", "Rotational spring stiffness [kNm/rad] at the support. By deafult, value is 0.", GH_ParamAccess.item, 0);

            // ¡YA NO HAY INPUTS BOOLEANOS AQUÍ! SE CONTROLAN CON LOS BOTONES
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Support(s)", "Sup", "Output support(s).", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> pts = new List<Point3d>();
            if (!DA.GetDataList(0, pts)) return;

            // Aquí usamos las propiedades públicas (Tx, Ty...) que modifican los botones
            List<CLTSupportData> supports = new List<CLTSupportData>();
            foreach (var p in pts)
            {
                supports.Add(new CLTSupportData(p, Tx, Ty, Tz, Rx, Ry, Rz));
            }
            DA.SetDataList(0, supports);
        }

        // 3. PERSISTENCIA (Guardar el estado de los botones cuando cierras Rhino)
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetBoolean("Tx", Tx); writer.SetBoolean("Ty", Ty); writer.SetBoolean("Tz", Tz);
            writer.SetBoolean("Rx", Rx); writer.SetBoolean("Ry", Ry); writer.SetBoolean("Rz", Rz);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (reader.ItemExists("Tx")) Tx = reader.GetBoolean("Tx");
            if (reader.ItemExists("Ty")) Ty = reader.GetBoolean("Ty");
            if (reader.ItemExists("Tz")) Tz = reader.GetBoolean("Tz");
            if (reader.ItemExists("Rx")) Rx = reader.GetBoolean("Rx");
            if (reader.ItemExists("Ry")) Ry = reader.GetBoolean("Ry");
            if (reader.ItemExists("Rz")) Rz = reader.GetBoolean("Rz");
            return base.Read(reader);
        }

        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.SupportPoint_icon;
        public override Guid ComponentGuid => new Guid("eeeee555-1111-2222-3333-555555555555");
    }

    // --- CLASE DE ATRIBUTOS VISUALES (LA MAGIA DE LA UI) ---
    public class SupportAttributes : GH_ComponentAttributes
    {
        private SupportComp _owner;
        private RectangleF _buttonArea;

        // Rectángulos para cada botón individual
        private RectangleF _btnTx, _btnTy, _btnTz, _btnRx, _btnRy, _btnRz;

        public SupportAttributes(SupportComp owner) : base(owner)
        {
            _owner = owner;
        }

        // 1. CALCULAR TAMAÑO (Hacer el componente más alto para que quepan los botones)
        protected override void Layout()
        {
            base.Layout();
            // Añadimos espacio extra abajo (por ejemplo 40 píxeles)
            RectangleF bounds = Bounds;
            bounds.Height += 40;
            Bounds = bounds;

            // Definir dónde va la zona de botones
            _buttonArea = new RectangleF(Bounds.X, Bounds.Bottom - 40, Bounds.Width, 40);

            // Calcular rejilla de 2 filas x 3 columnas
            float w = Bounds.Width / 3;
            float h = 20;
            float x = Bounds.X;
            float y = Bounds.Bottom - 40;

            _btnTx = new RectangleF(x, y, w, h);
            _btnTy = new RectangleF(x + w, y, w, h);
            _btnTz = new RectangleF(x + 2 * w, y, w, h);

            _btnRx = new RectangleF(x, y + h, w, h);
            _btnRy = new RectangleF(x + w, y + h, w, h);
            _btnRz = new RectangleF(x + 2 * w, y + h, w, h);
        }

        // 2. DIBUJAR (RENDER)
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel == GH_CanvasChannel.Objects)
            {
                // Dibujar botones T (Traslación)
                DrawButton(graphics, _btnTx, "Tx", _owner.Tx);
                DrawButton(graphics, _btnTy, "Ty", _owner.Ty);
                DrawButton(graphics, _btnTz, "Tz", _owner.Tz);

                // Dibujar botones R (Rotación)
                DrawButton(graphics, _btnRx, "Rx", _owner.Rx);
                DrawButton(graphics, _btnRy, "Ry", _owner.Ry);
                DrawButton(graphics, _btnRz, "Rz", _owner.Rz);
            }
        }

        private void DrawButton(Graphics g, RectangleF rect, string text, bool active)
        {
            // Estilo visual tipo Karamba (Naranja activo, Gris inactivo)
            Brush bgBrush = active ? new SolidBrush(Color.FromArgb(255, 150, 0)) : Brushes.LightGray;
            Pen borderPen = Pens.Black;
            Brush textBrush = Brushes.Black;

            // Padding para que no se toquen
            RectangleF drawRect = rect;
            drawRect.Inflate(-2, -2);

            g.FillRectangle(bgBrush, drawRect);
            g.DrawRectangle(borderPen, drawRect.X, drawRect.Y, drawRect.Width, drawRect.Height);

            // Centrar texto
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;

            // CORRECCIÓN: Usamos GH_FontServer.Standard en lugar de StandardSmall
            g.DrawString(text, GH_FontServer.Standard, textBrush, drawRect, format);
        }

        // 3. INTERACCIÓN (CLICS DEL RATÓN)
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left)
            {
                bool changed = false;

                if (_btnTx.Contains(e.CanvasLocation)) { _owner.Tx = !_owner.Tx; changed = true; }
                else if (_btnTy.Contains(e.CanvasLocation)) { _owner.Ty = !_owner.Ty; changed = true; }
                else if (_btnTz.Contains(e.CanvasLocation)) { _owner.Tz = !_owner.Tz; changed = true; }
                else if (_btnRx.Contains(e.CanvasLocation)) { _owner.Rx = !_owner.Rx; changed = true; }
                else if (_btnRy.Contains(e.CanvasLocation)) { _owner.Ry = !_owner.Ry; changed = true; }
                else if (_btnRz.Contains(e.CanvasLocation)) { _owner.Rz = !_owner.Rz; changed = true; }

                if (changed)
                {
                    _owner.ExpireSolution(true); // Recalcular componente
                    return GH_ObjectResponse.Handled;
                }
            }
            return base.RespondToMouseDown(sender, e);
        }
    }
}