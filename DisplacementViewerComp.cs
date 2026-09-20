using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Linq;

namespace CLT_Tools
{
    public class DisplacementViewerComp : GH_Component
    {
        // --- ESTADO PERSISTENTE (UI) ---
        public bool ExpandCaseMenu { get; set; } = true;
        public bool ExpandDisplayMenu { get; set; } = true;
        public bool ExpandValuesMenu { get; set; } = true;

        public double ScaleDeformation { get; set; } = 100.0;
        public bool ShowAxes { get; set; } = false;

        public bool ShowValues { get; set; } = false;
        public double ValueThreshold { get; set; } = 0.0;
        public int TextSize { get; set; } = 12;

        public int SelectedCaseIndex { get; set; } = 0;
        public List<string> CaseNames { get; set; } = new List<string>();

        public double CurrentMaxDisp { get; set; } = 0.0;

        // --- CACHÉ DE DATOS (ESTO ES LO QUE SE VE Y SE BAKEAA) ---
        // 1. Geometría lista para Bake
        private Mesh _bakeDefMesh = new Mesh();
        private Mesh _bakeLegendMesh = new Mesh();
        private List<Curve> _bakeLegendWires = new List<Curve>();

        // 2. Datos para reconstruir textos en el Bake (Más seguro que guardar objetos TextEntity)
        private class BakeTextData
        {
            public string Text;
            public Plane Plane;
            public double Height;
            public TextJustification Justification;
        }
        private List<BakeTextData> _bakeLegendTextData = new List<BakeTextData>();

        // 3. Objetos ligeros para visualización en pantalla (Preview)
        private List<Rhino.Display.Text3d> _previewLegendTexts = new List<Rhino.Display.Text3d>();

        // 4. Tags del modelo (HUD 2D)
        public List<Point3d> _cachedTagLocs = new List<Point3d>();
        public List<string> _cachedTagTexts = new List<string>();

        public DisplacementViewerComp()
          : base("Displacement Viewer", "DispView", "Displacement visualizer that allows evaluating and superimposing the deformed mesh of the structure in real time using a color map and interactive controls.", "CLT Tools", "5.Results")
        {
        }

        public override void CreateAttributes()
        {
            m_attributes = new DisplacementViewAttributes(this);
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Analysed Model", "Mesh", "Modelo analizado.", GH_ParamAccess.item);
            pManager.AddVectorParameter("All Displacements", "Vecs", "Vectors tree.", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Filter Selection", "Filt", "Filter (Text ID or Geometry).", GH_ParamAccess.list);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Deformed Mesh", "DefM", "Malla coloreada y deformada.", GH_ParamAccess.item);
            pManager.AddLineParameter("Deformed Axes", "Axes", "Ejes locales.", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // 0. LIMPIEZA TOTAL DE MEMORIA
            _cachedTagLocs.Clear();
            _cachedTagTexts.Clear();

            _bakeDefMesh = new Mesh();
            _bakeLegendMesh = new Mesh();
            _bakeLegendWires.Clear();
            _bakeLegendTextData.Clear();

            _previewLegendTexts.Clear();

            CurrentMaxDisp = 0.0;

            // 1. INPUTS
            Mesh mesh = null;
            GH_Structure<GH_Vector> dispTree = null;
            List<object> filterList = new List<object>();

            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetDataTree(1, out dispTree)) return;
            DA.GetDataList(2, filterList);

            // 2. METADATOS
            CaseNames.Clear();
            if (mesh.UserDictionary.ContainsKey("CLT_CaseNames"))
            {
                var val = mesh.UserDictionary["CLT_CaseNames"];
                if (val is System.Collections.IEnumerable list)
                    foreach (var item in list) CaseNames.Add(item.ToString());
            }
            List<string> elemIDs = new List<string>();
            if (mesh.UserDictionary.ContainsKey("CLT_ElementIDs"))
            {
                var val = mesh.UserDictionary["CLT_ElementIDs"];
                if (val is System.Collections.IEnumerable list)
                    foreach (var item in list) elemIDs.Add(item.ToString());
            }

            if (CaseNames.Count == 0) CaseNames.Add("LoadCase 0");
            while (CaseNames.Count < dispTree.Branches.Count) CaseNames.Add($"Case {CaseNames.Count}");
            if (SelectedCaseIndex >= dispTree.Branches.Count) SelectedCaseIndex = 0;
            if (SelectedCaseIndex < 0) SelectedCaseIndex = 0;

            // 3. VECTORES
            List<GH_Vector> activeGhVectors = dispTree.Branches[SelectedCaseIndex];
            List<Vector3d> activeVectors = activeGhVectors.Select(v => v.Value).ToList();

            double maxD = 0.0;
            foreach (var v in activeVectors) if (v.Length > maxD) maxD = v.Length;
            CurrentMaxDisp = maxD;

            // 4. LÓGICA DE FILTRADO
            bool useFilter = filterList.Count > 0 && filterList[0] != null;
            bool[] isFaceVisible = new bool[mesh.Faces.Count];
            bool[] isVertexVisible = new bool[mesh.Vertices.Count];

            if (!useFilter)
            {
                for (int i = 0; i < isFaceVisible.Length; i++) isFaceVisible[i] = true;
                for (int i = 0; i < isVertexVisible.Length; i++) isVertexVisible[i] = true;
            }
            else
            {
                HashSet<string> targetIDs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                List<GeometryBase> targetGeoms = new List<GeometryBase>();

                foreach (var obj in filterList)
                {
                    if (obj is GH_String gs) targetIDs.Add(gs.Value);
                    else if (obj is string s) targetIDs.Add(s);
                    else if (obj is IGH_Goo goo && goo.ScriptVariable() is GeometryBase geo) targetGeoms.Add(geo);
                    else if (obj is GeometryBase rawGeo) targetGeoms.Add(rawGeo);
                }

                for (int i = 0; i < mesh.Faces.Count; i++)
                {
                    bool visible = false;
                    Point3d center = mesh.Faces.GetFaceCenter(i);
                    if (targetIDs.Count > 0 && i < elemIDs.Count)
                    {
                        if (targetIDs.Contains(elemIDs[i])) visible = true;
                    }
                    if (!visible && targetGeoms.Count > 0)
                    {
                        foreach (var g in targetGeoms)
                        {
                            double dist = g.GetBoundingBox(true).ClosestPoint(center).DistanceTo(center);
                            if (dist < 10.0) { visible = true; break; }
                        }
                    }
                    isFaceVisible[i] = visible;
                }
                for (int i = 0; i < mesh.Faces.Count; i++)
                {
                    if (isFaceVisible[i])
                    {
                        MeshFace f = mesh.Faces[i];
                        isVertexVisible[f.A] = true; isVertexVisible[f.B] = true; isVertexVisible[f.C] = true;
                        if (f.IsQuad) isVertexVisible[f.D] = true;
                    }
                }
            }

            // 5. CONSTRUCCIÓN DE MALLA DEFORMADA
            Mesh defMesh = mesh.DuplicateMesh();
            defMesh.VertexColors.CreateMonotoneMesh(Color.FromArgb(0, 0, 255));
            for (int i = 0; i < defMesh.Vertices.Count; i++)
            {
                if (i < activeVectors.Count)
                {
                    Vector3d d = activeVectors[i];
                    if (d.IsValid)
                    {
                        double len = d.Length;
                        double t = (maxD > 1e-9) ? len / maxD : 0.0;
                        if (t > 1.0) t = 1.0;

                        // Color directo RGB: Azul (0%) -> Amarillo (50%) -> Rojo (100%)
                        Color c = GetCustomColor(t);
                        defMesh.VertexColors[i] = c;

                        if (len > 1e-9)
                        {
                            Point3d originalPt = defMesh.Vertices[i];
                            Point3d movedPt = originalPt + (d * ScaleDeformation);
                            defMesh.Vertices[i] = new Point3f((float)movedPt.X, (float)movedPt.Y, (float)movedPt.Z);
                        }
                    }
                }
            }

            // Tags (Antes de borrar caras)
            if (ShowValues)
            {
                for (int i = 0; i < defMesh.Vertices.Count; i++)
                {
                    if (!isVertexVisible[i]) continue;
                    if (i < activeVectors.Count)
                    {
                        double len = activeVectors[i].Length;
                        if (len >= ValueThreshold)
                        {
                            _cachedTagLocs.Add(defMesh.Vertices[i]);
                            _cachedTagTexts.Add(len.ToString("0.00"));
                        }
                    }
                }
            }

            // 6. FILTRADO (Borrar Caras)
            if (useFilter)
            {
                List<int> facesToDelete = new List<int>();
                for (int i = 0; i < isFaceVisible.Length; i++)
                {
                    if (!isFaceVisible[i]) facesToDelete.Add(i);
                }
                if (facesToDelete.Count > 0)
                {
                    defMesh.Faces.DeleteFaces(facesToDelete);
                    defMesh.Compact();
                }
            }
            defMesh.FaceNormals.ComputeFaceNormals();

            // GUARDAR MODELO PARA BAKE
            _bakeDefMesh = defMesh;

            // 7. CALCULAR LEYENDA Y GUARDAR EN LISTAS
            BoundingBox originalBox = mesh.GetBoundingBox(true);
            CalculateLegend(originalBox, maxD);

            // 8. SALIDAS
            DA.SetData(0, defMesh);

            if (ShowAxes)
            {
                List<Line> axes = new List<Line>();
                foreach (Point3d v in defMesh.Vertices)
                {
                    axes.Add(new Line(v, v + Vector3d.XAxis * (ScaleDeformation * 0.2)));
                }
                DA.SetDataList(1, axes);
            }
        }

        // --- MÉTODO AUXILIAR PARA CALCULAR LEYENDA (LLAMADO EN SOLVEINSTANCE) ---
        private void CalculateLegend(BoundingBox bbox, double maxVal)
        {
            double height = bbox.Max.Z - bbox.Min.Z;
            double widthX = bbox.Max.X - bbox.Min.X;
            double widthY = bbox.Max.Y - bbox.Min.Y;
            double maxDim = Math.Max(widthX, widthY);

            if (height < (maxDim * 0.1)) height = maxDim * 0.5;
            if (height < 1.0) height = 10.0;

            double barWidth = height * 0.05;
            double gapX = barWidth * 2;
            Point3d origin = new Point3d(bbox.Max.X + gapX + barWidth, bbox.Min.Y, bbox.Min.Z);

            int steps = 10;
            double stepH = height / steps;

            // A. MALLA DE COLOR
            for (int i = 0; i <= steps; i++)
            {
                double z = origin.Z + (i * stepH);
                double t = (double)i / steps;

                // Color directo RGB: Azul (0%) -> Amarillo (50%) -> Rojo (100%)
                Color c = GetCustomColor(t);

                _bakeLegendMesh.Vertices.Add(new Point3d(origin.X, origin.Y, z));
                _bakeLegendMesh.Vertices.Add(new Point3d(origin.X + barWidth, origin.Y, z));
                _bakeLegendMesh.VertexColors.Add(c); _bakeLegendMesh.VertexColors.Add(c);

                if (i > 0)
                {
                    int v = _bakeLegendMesh.Vertices.Count;
                    _bakeLegendMesh.Faces.AddFace(v - 4, v - 3, v - 1, v - 2);
                }
            }
            _bakeLegendMesh.Normals.ComputeNormals();
            _bakeLegendMesh.Compact();

            // B. CURVAS Y TEXTOS
            Point3d bl = new Point3d(origin.X, origin.Y, origin.Z);
            Point3d br = new Point3d(origin.X + barWidth, origin.Y, origin.Z);
            Point3d tl = new Point3d(origin.X, origin.Y, origin.Z + height);
            Point3d tr = new Point3d(origin.X + barWidth, origin.Y, origin.Z + height);
            Polyline border = new Polyline(new Point3d[] { bl, br, tr, tl, bl });
            _bakeLegendWires.Add(border.ToNurbsCurve());

            for (int i = 0; i <= steps; i++)
            {
                double z = origin.Z + (i * stepH);
                double val = maxVal * ((double)i / steps);
                string txt = (i == 0) ? "0.00" : $"{val:0.00}";

                Point3d p1 = new Point3d(origin.X + barWidth, origin.Y, z);
                Point3d p2 = new Point3d(origin.X + barWidth + (barWidth * 0.2), origin.Y, z);
                _bakeLegendWires.Add(new Line(p1, p2).ToNurbsCurve());


                double txtHeight = height / 50.0;
                if (txtHeight < 0.1) txtHeight = 1.0;
                double xPos = p2.X + (barWidth * 0.1);
                double zCentered = z;

                Point3d txtPos = new Point3d(xPos, p2.Y, zCentered);

                Plane plane = new Plane(txtPos, Vector3d.XAxis, Vector3d.ZAxis);

                // Guardar datos para BAKE
                _bakeLegendTextData.Add(new BakeTextData
                {
                    Text = txt,
                    Plane = plane,
                    Height = txtHeight,
                    Justification = TextJustification.MiddleLeft
                });

                // Guardar objetos para PREVIEW (Text3d es ligero)
                var t3d = new Rhino.Display.Text3d(txt, plane, txtHeight);
                t3d.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Left;
                t3d.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Middle;
                _previewLegendTexts.Add(t3d);
            }

            // Título
            Point3d titlePos = new Point3d(origin.X, origin.Y, origin.Z + height + (height * 0.05));
            double titleH = height / 30.0;
            Plane titlePl = new Plane(titlePos, Vector3d.XAxis, Vector3d.ZAxis);

            _bakeLegendTextData.Add(new BakeTextData
            {
                Text = "Desplazamiento [mm]",
                Plane = titlePl,
                Height = titleH,
                Justification = TextJustification.BottomLeft
            });

            var titleT3d = new Rhino.Display.Text3d("Desplazamiento [mm]", titlePl, titleH);
            titleT3d.HorizontalAlignment = Rhino.DocObjects.TextHorizontalAlignment.Left;
            titleT3d.VerticalAlignment = Rhino.DocObjects.TextVerticalAlignment.Bottom;
            _previewLegendTexts.Add(titleT3d);
        }

        // --- BAKE CORRECTO (CORREGIDO) ---
        public override void BakeGeometry(Rhino.RhinoDoc doc, Rhino.DocObjects.ObjectAttributes att, List<Guid> obj_ids)
        {
            // Gestión de atributos (Si es null, creamos uno por defecto. Si no, usamos el que elige el usuario)
            if (att == null) att = doc.CreateDefaultAttributes();

            // 1. BAKE MALLA DEFORMADA
            if (_bakeDefMesh != null && _bakeDefMesh.IsValid)
            {
                var meshAttr = att.Duplicate(); // Copiamos capa/color elegidos
                meshAttr.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject; // Forzamos color del análisis
                meshAttr.Name = "Deformed_" + CaseNames[SelectedCaseIndex];

                Guid id = doc.Objects.AddMesh(_bakeDefMesh, meshAttr);
                if (id != Guid.Empty) obj_ids.Add(id);
            }

            // 2. BAKE LEYENDA (MALLA)
            if (_bakeLegendMesh != null && _bakeLegendMesh.IsValid)
            {
                var legAttr = att.Duplicate();
                legAttr.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
                legAttr.Name = "Legend_Gradient";

                Guid id = doc.Objects.AddMesh(_bakeLegendMesh, legAttr);
                if (id != Guid.Empty) obj_ids.Add(id);
            }

            // 3. BAKE LÍNEAS LEYENDA
            foreach (var crv in _bakeLegendWires)
            {
                if (crv != null && crv.IsValid)
                {
                    Guid id = doc.Objects.AddCurve(crv, att);
                    if (id != Guid.Empty) obj_ids.Add(id);
                }
            }

            // 4. BAKE TEXTOS
            foreach (var data in _bakeLegendTextData)
            {
                // Opción A: Texto simple
                Guid id = doc.Objects.AddText(data.Text, data.Plane, data.Height, "Arial", false, false);

                // Si el ID es válido, intentamos moverlo a la capa correcta si es necesario,
                // o usamos TextEntity para ser más precisos con los atributos 'att'.
                if (id != Guid.Empty)
                {
                    var txtObj = doc.Objects.Find(id);
                    if (txtObj != null)
                    {
                        txtObj.Attributes = att.Duplicate();
                        txtObj.CommitChanges();
                    }
                    obj_ids.Add(id);
                }
            }
        }
        public static Color GetCustomColor(double t)
        {
            if (t < 0.0) t = 0.0;
            if (t > 1.0) t = 1.0;

            // 11 paradas de color (Azul -> Celeste -> Amarillo cálido -> Naranja -> Rojo) sin tonos verdes
            Color[] palette = new Color[]
            {
               Color.FromArgb(41,  98,  173), // 0: Azul oscuro (0%)
               Color.FromArgb(60,  120, 190), // 1
               Color.FromArgb(94,  143, 201), // 2
               Color.FromArgb(130, 175, 215), // 3
               Color.FromArgb(161, 204, 224), // 4: Celeste pálido
               Color.FromArgb(220, 220, 140), // 5: Amarillo suave (50%) - sin verde
               Color.FromArgb(248, 235, 85),  // 6: Amarillo puro
               Color.FromArgb(248, 185, 59),  // 7: Amarillo anaranjado
               Color.FromArgb(239, 126, 42),  // 8: Naranja
               Color.FromArgb(226, 68,  34),  // 9: Naranja rojizo
               Color.FromArgb(215, 25,  28)   // 10: Rojo intenso (100%)
            };

            double scaled = t * (palette.Length - 1);
            int index = (int)Math.Floor(scaled);
            if (index >= palette.Length - 1) return palette[palette.Length - 1];

            double localT = scaled - index;
            Color c0 = palette[index];
            Color c1 = palette[index + 1];

            int r = (int)(c0.R + localT * (c1.R - c0.R));
            int g = (int)(c0.G + localT * (c1.G - c0.G));
            int b = (int)(c0.B + localT * (c1.B - c0.B));

            return Color.FromArgb(255, r, g, b);
        }

        // --- PREVISUALIZACIÓN ---
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            base.DrawViewportMeshes(args);
            
            // Dibuja el modelo del output
            if (_bakeDefMesh != null && _bakeDefMesh.IsValid)
            {
                args.Display.DrawMeshFalseColors(_bakeDefMesh);
            }

            // Dibuja la leyenda
            if (_bakeLegendMesh != null && _bakeLegendMesh.IsValid)
                args.Display.DrawMeshFalseColors(_bakeLegendMesh);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args); // Dibuja cables del output

            // Tags 2D (Modelo)
            if (ShowValues && _cachedTagLocs.Count > 0)
            {
                for (int i = 0; i < _cachedTagLocs.Count; i++)
                    args.Display.Draw2dText(_cachedTagTexts[i], Color.Black, _cachedTagLocs[i], true, TextSize);
            }

            // Líneas Leyenda
            foreach (var crv in _bakeLegendWires)
                args.Display.DrawCurve(crv, Color.Black, 1);

            // Textos Leyenda (Versión display)
            foreach (var t3d in _previewLegendTexts)
                if (t3d != null) args.Display.Draw3dText(t3d, Color.Black);
        }

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetDouble("ScaleDef", ScaleDeformation);
            writer.SetBoolean("ShowAxes", ShowAxes);
            writer.SetInt32("SelCase", SelectedCaseIndex);
            writer.SetBoolean("ExpCase", ExpandCaseMenu);
            writer.SetBoolean("ExpDisp", ExpandDisplayMenu);
            writer.SetBoolean("ExpVal", ExpandValuesMenu);
            writer.SetBoolean("ShowVal", ShowValues);
            writer.SetDouble("ValThres", ValueThreshold);
            writer.SetInt32("TxtSize", TextSize);
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            if (reader.ItemExists("ScaleDef")) ScaleDeformation = reader.GetDouble("ScaleDef");
            if (reader.ItemExists("ShowAxes")) ShowAxes = reader.GetBoolean("ShowAxes");
            if (reader.ItemExists("SelCase")) SelectedCaseIndex = reader.GetInt32("SelCase");
            if (reader.ItemExists("ExpCase")) ExpandCaseMenu = reader.GetBoolean("ExpCase");
            if (reader.ItemExists("ExpDisp")) ExpandDisplayMenu = reader.GetBoolean("ExpDisp");
            if (reader.ItemExists("ExpVal")) ExpandValuesMenu = reader.GetBoolean("ExpVal");
            if (reader.ItemExists("ShowVal")) ShowValues = reader.GetBoolean("ShowVal");
            if (reader.ItemExists("ValThres")) ValueThreshold = reader.GetDouble("ValThres");
            if (reader.ItemExists("TxtSize")) TextSize = reader.GetInt32("TxtSize");
            return base.Read(reader);
        }

        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.DisplacementViewer_icon;
        public override Guid ComponentGuid => new Guid("55556666-1111-2222-3333-999999999999");
    }

    public class DisplacementViewAttributes : GH_ComponentAttributes
    {
        private DisplacementViewerComp _owner;
        private RectangleF _recBtnCase, _recListCase;
        private RectangleF _recBtnDisp, _recListDisp;
        private RectangleF _recBtnVals, _recListVals;
        private RectangleF _recSliderDef;
        private RectangleF _recBtnAxes;
        private RectangleF _recToggleShow;
        private RectangleF _recSliderThres;
        private RectangleF _recSliderSize;
        private RectangleF _recInfoText;

        private const float HeaderH = 20;
        private const float ItemH = 20;
        private const float SliderH = 20;
        private const float InfoH = 25;
        private const float FixedWidth = 160;

        private readonly Pen _borderPen = new Pen(Color.FromArgb(51, 51, 51));
        private readonly Brush _selBrush = new SolidBrush(Color.FromArgb(247, 165, 56));

        private bool _isDraggingSlider = false;
        private RectangleF _activeSliderRec;
        private int _activeSliderID = -1;

        public DisplacementViewAttributes(DisplacementViewerComp owner) : base(owner) { _owner = owner; }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_owner.ExpandDisplayMenu && _recSliderDef.Contains(e.CanvasLocation))
                {
                    _isDraggingSlider = true; _activeSliderRec = _recSliderDef; _activeSliderID = 0;
                    UpdateSliderValue(e.CanvasLocation.X); sender.Refresh(); return GH_ObjectResponse.Capture;
                }
                if (_owner.ExpandValuesMenu)
                {
                    if (_recSliderThres.Contains(e.CanvasLocation))
                    {
                        _isDraggingSlider = true; _activeSliderRec = _recSliderThres; _activeSliderID = 1;
                        UpdateSliderValue(e.CanvasLocation.X); sender.Refresh(); return GH_ObjectResponse.Capture;
                    }
                    if (_recSliderSize.Contains(e.CanvasLocation))
                    {
                        _isDraggingSlider = true; _activeSliderRec = _recSliderSize; _activeSliderID = 2;
                        UpdateSliderValue(e.CanvasLocation.X); sender.Refresh(); return GH_ObjectResponse.Capture;
                    }
                }
            }
            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (_isDraggingSlider && e.Button == MouseButtons.Left)
            {
                UpdateSliderValue(e.CanvasLocation.X); _owner.ExpireSolution(true); sender.Refresh(); return GH_ObjectResponse.Capture;
            }
            return base.RespondToMouseMove(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (_isDraggingSlider) { _isDraggingSlider = false; _activeSliderID = -1; return GH_ObjectResponse.Release; }
            if (e.Button == MouseButtons.Left)
            {
                if (_recBtnCase.Contains(e.CanvasLocation)) { _owner.ExpandCaseMenu = !_owner.ExpandCaseMenu; _owner.ExpireSolution(true); return GH_ObjectResponse.Handled; }
                if (_recBtnDisp.Contains(e.CanvasLocation)) { _owner.ExpandDisplayMenu = !_owner.ExpandDisplayMenu; _owner.ExpireSolution(true); return GH_ObjectResponse.Handled; }
                if (_recBtnVals.Contains(e.CanvasLocation)) { _owner.ExpandValuesMenu = !_owner.ExpandValuesMenu; _owner.ExpireSolution(true); return GH_ObjectResponse.Handled; }

                if (_owner.ExpandCaseMenu && _recListCase.Contains(e.CanvasLocation))
                {
                    int idx = (int)((e.CanvasLocation.Y - _recListCase.Y) / ItemH);
                    if (idx >= 0 && idx < _owner.CaseNames.Count) { _owner.SelectedCaseIndex = idx; _owner.ExpireSolution(true); }
                    return GH_ObjectResponse.Handled;
                }
                if (_owner.ExpandDisplayMenu && _recBtnAxes.Contains(e.CanvasLocation))
                {
                    _owner.ShowAxes = !_owner.ShowAxes; _owner.ExpireSolution(true); return GH_ObjectResponse.Handled;
                }
                if (_owner.ExpandValuesMenu && _recToggleShow.Contains(e.CanvasLocation))
                {
                    _owner.ShowValues = !_owner.ShowValues; _owner.ExpireSolution(true); return GH_ObjectResponse.Handled;
                }
            }
            return base.RespondToMouseUp(sender, e);
        }

        private void UpdateSliderValue(float mouseX)
        {
            float p = (mouseX - _activeSliderRec.X) / _activeSliderRec.Width;
            if (p < 0) p = 0; if (p > 1) p = 1;

            if (_activeSliderID == 0) _owner.ScaleDeformation = 0 + (1000 - 0) * p;
            else if (_activeSliderID == 1) { double max = _owner.CurrentMaxDisp > 0.001 ? _owner.CurrentMaxDisp : 10.0; _owner.ValueThreshold = 0 + (max - 0) * p; }
            else if (_activeSliderID == 2) _owner.TextSize = (int)(8 + (30 - 8) * p);
        }

        protected override void Layout()
        {
            base.Layout();
            float width = Math.Max(Bounds.Width, FixedWidth);
            RectangleF b = Bounds; float currentY = b.Bottom;

            _recBtnCase = new RectangleF(b.X, currentY, width, HeaderH); currentY += HeaderH;
            float hCase = _owner.ExpandCaseMenu ? (_owner.CaseNames.Count * ItemH) : 0;
            _recListCase = new RectangleF(b.X, currentY, width, hCase);
            if (_owner.ExpandCaseMenu) currentY += hCase + 4;

            _recBtnDisp = new RectangleF(b.X, currentY, width, HeaderH); currentY += HeaderH;
            if (_owner.ExpandDisplayMenu)
            {
                float innerY = currentY + 5;
                _recSliderDef = new RectangleF(b.X + 5, innerY, width - 10, SliderH); innerY += SliderH + 2;
                _recBtnAxes = new RectangleF(b.X + 5, innerY, width - 10, SliderH); innerY += SliderH + 5;
                _recListDisp = new RectangleF(b.X, currentY, width, innerY - currentY); currentY = innerY;
            }
            else { _recListDisp = RectangleF.Empty; _recSliderDef = RectangleF.Empty; _recBtnAxes = RectangleF.Empty; }

            _recBtnVals = new RectangleF(b.X, currentY, width, HeaderH); currentY += HeaderH;
            if (_owner.ExpandValuesMenu)
            {
                float innerY = currentY + 5;
                _recToggleShow = new RectangleF(b.X + 5, innerY, width - 10, SliderH); innerY += SliderH + 5;
                _recSliderThres = new RectangleF(b.X + 5, innerY, width - 10, SliderH); innerY += SliderH + 2;
                _recSliderSize = new RectangleF(b.X + 5, innerY, width - 10, SliderH); innerY += SliderH + 5;
                _recListVals = new RectangleF(b.X, currentY, width, innerY - currentY); currentY = innerY;
            }
            else { _recListVals = RectangleF.Empty; _recToggleShow = RectangleF.Empty; _recSliderThres = RectangleF.Empty; _recSliderSize = RectangleF.Empty; }

            currentY += 5;
            _recInfoText = new RectangleF(b.X, currentY, width, InfoH); currentY += InfoH;
            Bounds = new RectangleF(b.X, b.Y, width, currentY - b.Y);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);
            if (channel != GH_CanvasChannel.Objects) return;

            DrawHeader(graphics, _recBtnCase, "Select Load Case", _owner.ExpandCaseMenu);
            if (_owner.ExpandCaseMenu)
            {
                graphics.FillRectangle(Brushes.White, _recListCase);
                StringFormat fmt = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap, Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
                for (int i = 0; i < _owner.CaseNames.Count; i++)
                {
                    RectangleF r = new RectangleF(_recListCase.X, _recListCase.Y + i * ItemH, _recListCase.Width, ItemH);
                    bool sel = (i == _owner.SelectedCaseIndex);
                    graphics.FillRectangle(sel ? _selBrush : Brushes.White, r);
                    graphics.DrawRectangle(Pens.WhiteSmoke, r.X, r.Y, r.Width, r.Height);
                    RectangleF txtR = r; txtR.X += 4; txtR.Width -= 8;
                    graphics.DrawString(_owner.CaseNames[i], GH_FontServer.Standard, Brushes.Black, txtR, fmt);
                }
                graphics.DrawRectangle(_borderPen, Rectangle.Round(_recListCase));
            }

            DrawHeader(graphics, _recBtnDisp, "Display Settings", _owner.ExpandDisplayMenu);
            if (_owner.ExpandDisplayMenu)
            {
                graphics.FillRectangle(Brushes.White, _recListDisp); graphics.DrawRectangle(_borderPen, Rectangle.Round(_recListDisp));
                DrawSlider(graphics, _recSliderDef, "Def Scale", _owner.ScaleDeformation, 0, 1000, "F0");
                DrawToggle(graphics, _recBtnAxes, "Show Local Axes", _owner.ShowAxes);
            }

            DrawHeader(graphics, _recBtnVals, "Displacement Values", _owner.ExpandValuesMenu);
            if (_owner.ExpandValuesMenu)
            {
                graphics.FillRectangle(Brushes.White, _recListVals); graphics.DrawRectangle(_borderPen, Rectangle.Round(_recListVals));
                DrawToggle(graphics, _recToggleShow, "Show Values", _owner.ShowValues);
                double max = _owner.CurrentMaxDisp > 0.001 ? _owner.CurrentMaxDisp : 10.0;
                DrawSlider(graphics, _recSliderThres, "Min Value", _owner.ValueThreshold, 0.00, max, "F2");
                DrawSlider(graphics, _recSliderSize, "Text Size", _owner.TextSize, 8, 30, "F0");
            }

            float ly = _recInfoText.Top - 2;
            graphics.DrawLine(_borderPen, _recInfoText.X + 5, ly, _recInfoText.Right - 5, ly);
            StringFormat cf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            graphics.DrawString($"Max. Displ.: {_owner.CurrentMaxDisp:0.00} mm", GH_FontServer.Standard, Brushes.Black, _recInfoText, cf);
        }

        private void DrawHeader(Graphics g, RectangleF r, string t, bool exp)
        {
            g.FillRectangle(Brushes.Black, r); g.DrawRectangle(_borderPen, Rectangle.Round(r));
            g.DrawString(t, GH_FontServer.StandardBold, Brushes.White, r.X + 4, r.Y + 2);
            g.DrawString(exp ? "-" : "+", GH_FontServer.StandardBold, Brushes.White, r.Right - 15, r.Y + 2);
        }
        private void DrawSlider(Graphics g, RectangleF r, string n, double v, double min, double max, string f)
        {
            g.FillRectangle(Brushes.WhiteSmoke, r); g.DrawRectangle(Pens.LightGray, Rectangle.Round(r));
            float p = (float)((v - min) / (max - min));
            if (p < 0) p = 0; if (p > 1) p = 1;
            RectangleF fill = new RectangleF(r.X, r.Y, r.Width * p, r.Height);
            g.FillRectangle(Brushes.Silver, fill); g.DrawRectangle(Pens.Gray, Rectangle.Round(fill));
            g.DrawString($"{n}: {v.ToString(f)}", GH_FontServer.Standard, Brushes.Black, r.X + 4, r.Y + 2);
        }
        private void DrawToggle(Graphics g, RectangleF r, string t, bool s)
        {
            g.FillRectangle(s ? _selBrush : Brushes.WhiteSmoke, r); g.DrawRectangle(Pens.LightGray, Rectangle.Round(r));
            StringFormat fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(t, GH_FontServer.Standard, Brushes.Black, r, fmt);
        }
    }
}