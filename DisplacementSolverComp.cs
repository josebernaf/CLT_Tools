using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CLT_Tools
{
    public class DisplacementSolverComp : GH_Component
    {
        public DisplacementSolverComp()
          : base("Displacement Solver", "Analyze", "Calculates the deflections of a given model.", "CLT Tools", "4 :: Algorithms")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "CLT model.", GH_ParamAccess.item);
            pManager.AddTextParameter("LCase-Comb", "Comb", "Rules of combining LoadCases (e.g. '1.35*PP + 1.5*Live').", GH_ParamAccess.list);
            pManager.AddGenericParameter("Seismic Data", "SData", "Seismic Data (Optional).", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write CSV", "W", "Set to TRUE to export results to CSV in the same directory as the rest analysis files.", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Analysed Mesh", "Mesh", "Mesh.", GH_ParamAccess.item);
            pManager.AddVectorParameter("All Displacements", "AllD", "Vectors.", GH_ParamAccess.tree);
            pManager.AddTextParameter("Log", "Log", "Report.", GH_ParamAccess.item);
        }

        // --- HELPER: Distancia Inteligente a Curvas ---
        private double DistanceToCurve(Curve crv, Point3d pt)
        {
            if (crv == null) return double.MaxValue;
            if (crv.IsLinear())
            {
                // Si la línea es recta, la tratamos como INFINITA para que no haya "chinchetas" en las esquinas
                Line l = new Line(crv.PointAtStart, crv.PointAtEnd);
                return l.ClosestPoint(pt, true).DistanceTo(pt); // FIXED: true=finita, no infinita
            }
            crv.ClosestPoint(pt, out double t);
            return crv.PointAt(t).DistanceTo(pt);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            StringBuilder finalLog = new StringBuilder();
            DateTime startTime = DateTime.Now;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // 1. LIMPIEZA
            try { foreach (var proc in Process.GetProcessesByName("OpenSees")) proc.Kill(); } catch { }
            string tempFolder = Path.Combine(Path.GetTempPath(), "CLT_Solver");
            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
            foreach (string f in Directory.GetFiles(tempFolder, "res_*.txt")) { try { File.Delete(f); } catch { } }

            GH_ObjectWrapper modelWrap = null;
            List<string> combRules = new List<string>();
            GH_ObjectWrapper seismicWrap = null;
            bool writeCSV = false;

            if (!DA.GetData(0, ref modelWrap)) return;
            if (!DA.GetDataList(1, combRules)) combRules.Add("1.0*PP");
            DA.GetData(2, ref seismicWrap);
            DA.GetData(3, ref writeCSV);

            CLTModel model = modelWrap.Value as CLTModel;
            if (model == null) return;

            object sDataObj = null;
            if (seismicWrap != null)
            {
                object raw = seismicWrap.Value;
                if (raw is GH_ObjectWrapper wrapper) sDataObj = wrapper.Value;
                else if (raw is IGH_Goo goo) sDataObj = goo.ScriptVariable();
                else sDataObj = raw;
            }

            List<CLTElement> originalElements = model.Elements;
            List<CLTLoadData> loads = model.Loads;
            List<CLTSupportData> supports = model.Supports;

            // --- EXTRAER JUNTAS DEL MODELO ---
            List<CLTJointData> joints = new List<CLTJointData>();
            try
            {
                PropertyInfo prop = model.GetType().GetProperty("Joints");
                if (prop != null)
                {
                    System.Collections.IEnumerable jointsList = prop.GetValue(model) as System.Collections.IEnumerable;
                    if (jointsList != null)
                    {
                        foreach (var j in jointsList) joints.Add((CLTJointData)j);
                    }
                }
            }
            catch { }

            if (originalElements.Count == 0) return;

            string exePath = @"C:\OpenSees\OpenSees.exe";
            if (!File.Exists(exePath)) exePath = @"C:\OpenSees\bin\OpenSees.exe";
            if (!File.Exists(exePath)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "OpenSees not found."); return; }

            // TOPOLOGÍA & REFINAMIENTO CON JOINT-AWARE
            Mesh coarseMesh = new Mesh();
            List<int> coarseOwners = new List<int>();
            List<int> vertexOwners = new List<int>(); // Rastreamos dueño de cada vértice

            for (int i = 0; i < originalElements.Count; i++)
            {
                Mesh m = originalElements[i].Geometry as Mesh;
                if (m != null)
                {
                    coarseMesh.Append(m);
                    for (int k = 0; k < m.Faces.Count; k++) coarseOwners.Add(i);
                    for (int k = 0; k < m.Vertices.Count; k++) vertexOwners.Add(i);
                }
            }

            List<int> weldedOwners;
            // Llamamos al CustomWeld avanzado pasándole joints y vertexOwners
            Mesh weldedCoarseMesh = CustomWeld(coarseMesh, coarseOwners, vertexOwners, joints, out weldedOwners, 5.0);

            Mesh ghostMesh = weldedCoarseMesh;
            List<int> ghostOwners = weldedOwners;

            ghostMesh.FaceNormals.ComputeFaceNormals();
            ghostMesh.Compact();

            int numGhostNodes = ghostMesh.Vertices.Count;
            Point3d[] ghostPts = ghostMesh.Vertices.ToPoint3dArray();

            double[] nodeMasses = CalculateNodeMasses(ghostMesh, ghostOwners, originalElements);
            double[] ghostNodeAreas = new double[numGhostNodes];

            double totalMass = nodeMasses.Sum();
            finalLog.AppendLine($"Physics Check: Total Model Mass = {totalMass:F2} kg");
            finalLog.AppendLine($"Topology Check: Detected {joints.Count} Joint definitions.");

            Vector3d[] selfWeightForces = new Vector3d[numGhostNodes];
            for (int i = 0; i < numGhostNodes; i++) selfWeightForces[i] = new Vector3d(0, 0, -nodeMasses[i] * 9.81);

            for (int i = 0; i < ghostMesh.Faces.Count; i++)
            {
                MeshFace f = ghostMesh.Faces[i];
                Point3d A = ghostPts[f.A], B = ghostPts[f.B], C = ghostPts[f.C];
                double area = 0;
                if (f.IsQuad)
                {
                    area = 0.5 * (Vector3d.CrossProduct(B - A, ghostPts[f.D] - A).Length + Vector3d.CrossProduct(C - B, ghostPts[f.D] - B).Length);
                    double q = area / 4.0;
                    ghostNodeAreas[f.A] += q; ghostNodeAreas[f.B] += q; ghostNodeAreas[f.C] += q; ghostNodeAreas[f.D] += q;
                }
                else
                {
                    area = 0.5 * Vector3d.CrossProduct(B - A, C - A).Length;
                    double t = area / 3.0;
                    ghostNodeAreas[f.A] += t; ghostNodeAreas[f.B] += t; ghostNodeAreas[f.C] += t;
                }
            }

            List<string> nodeFixities = new List<string>(new string[numGhostNodes]);
            foreach (var sup in supports)
            {
                for (int i = 0; i < numGhostNodes; i++) if (ghostPts[i].DistanceTo(sup.Location) < 15.0) nodeFixities[i] = sup.FixString;
            }

            List<string> lineFixities = new List<string>();
            List<Line> constrainedLines = new List<Line>();
            RTree supTree2 = new RTree();
            for (int i = 0; i < supports.Count; i++) supTree2.Insert(supports[i].Location, i);

            for (int i = 0; i < weldedCoarseMesh.TopologyEdges.Count; i++)
            {
                Line edge = weldedCoarseMesh.TopologyEdges.EdgeLine(i);
                bool startFixed = false; string startFixStr = "";
                bool endFixed = false;
                supTree2.Search(new Sphere(edge.From, 20.0), (s, a) => { startFixed = true; startFixStr = supports[a.Id].FixString; });
                supTree2.Search(new Sphere(edge.To, 20.0), (s, a) => { endFixed = true; });
                if (startFixed && endFixed) { constrainedLines.Add(edge); lineFixities.Add(startFixStr); }
            }

            var culture = CultureInfo.InvariantCulture;
            StringBuilder tcl = new StringBuilder();
            GH_Structure<GH_Vector> outVectors = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Vector> outRotations = new GH_Structure<GH_Vector>();

            // --- 1. CACHÉ DE MATERIALES Y SECCIONES (Alta Eficiencia) ---
            Dictionary<int, int> elemToSecID = new Dictionary<int, int>();
            int secCounter = 1000;
            HashSet<string> definedMats = new HashSet<string>();
            StringBuilder matBlock = new StringBuilder();

            for (int i = 0; i < originalElements.Count; i++)
            {
                CLTElement el = originalElements[i];
                int mb = 10000 + (Math.Abs(el.MaterialName.GetHashCode()) % 10000);
                if (!definedMats.Contains(el.MaterialName))
                {
                    CLTMaterial m = el.Material;
                    matBlock.AppendLine(string.Format(culture, "nDMaterial ElasticOrthotropic {0} {1} {2} {3} {4} 0 0 {5} {6} {7} {8};", mb, m.E0, m.E90, m.E90, m.Nu12, m.Gxy, m.Gyz, m.Gxz, m.Rho));
                    matBlock.AppendLine($"nDMaterial PlateFiber {mb + 5000} {mb};");
                    double nu21 = m.Nu12 * (m.E90 / m.E0);
                    matBlock.AppendLine(string.Format(culture, "nDMaterial ElasticOrthotropic {0} {1} {2} {3} {4} 0 0 {5} {6} {7} {8};", mb + 10000, m.E90, m.E0, m.E90, nu21, m.Gxy, m.Gxz, m.Gyz, m.Rho));
                    matBlock.AppendLine($"nDMaterial PlateFiber {mb + 15000} {mb + 10000};");
                    definedMats.Add(el.MaterialName);
                }
                int sID = secCounter++;
                matBlock.Append($"section LayeredShell {sID} {el.Properties.Thicknesses.Count}");
                for (int k = 0; k < el.Properties.Thicknesses.Count; k++)
                {
                    int id = (Math.Abs(el.Properties.Angles[k]) < 1.0) ? mb + 5000 : mb + 15000;
                    matBlock.Append(string.Format(culture, " {0} {1}", id, el.Properties.Thicknesses[k]));
                }
                matBlock.AppendLine(";");

                int sID_Rot = secCounter++;
                matBlock.Append($"section LayeredShell {sID_Rot} {el.Properties.Thicknesses.Count}");
                for (int k = 0; k < el.Properties.Thicknesses.Count; k++)
                {
                    int id = (Math.Abs(el.Properties.Angles[k]) < 1.0) ? mb + 15000 : mb + 5000;
                    matBlock.Append(string.Format(culture, " {0} {1}", id, el.Properties.Thicknesses[k]));
                }
                matBlock.AppendLine(";");
                elemToSecID[i] = sID;
            }

            int totalSpringsGenerated = 0;
            double sumKser = 0;

            // --- BUCLE PRINCIPAL DE COMBINACIONES ---
            for (int c = 0; c < combRules.Count; c++)
            {
                string rule = combRules[c];
                tcl.AppendLine($"# --- COMBINATION {c}: {rule} ---");
                tcl.AppendLine("wipe; model BasicBuilder -ndm 3 -ndf 6;");
                tcl.Append(matBlock.ToString());

                // 2. NODOS
                for (int i = 0; i < numGhostNodes; i++)
                {
                    tcl.AppendLine(string.Format(culture, "node {0} {1:F4} {2:F4} {3:F4};", i + 1, ghostPts[i].X, ghostPts[i].Y, ghostPts[i].Z));
                }

                // 3. APOYOS FÍSICOS (Con protección anti-chinchetas y anti-rotación Z)
                for (int i = 0; i < numGhostNodes; i++) tcl.AppendLine($"fix {i + 1} 0 0 0 0 0 1;");

                List<Point3d> fixedLocations = new List<Point3d>();
                if (supports.Count > 0)
                {
                    for (int i = 0; i < numGhostNodes; i++)
                    {
                        foreach (var sup in supports)
                        {
                            if (ghostPts[i].DistanceTo(sup.Location) < 20.0)
                            {
                                bool alreadyFixedHere = false;
                                foreach (var fp in fixedLocations) if (fp.DistanceTo(ghostPts[i]) < 1.0) { alreadyFixedHere = true; break; }
                                if (!alreadyFixedHere) { tcl.AppendLine($"fix {i + 1} {sup.FixString};"); fixedLocations.Add(ghostPts[i]); }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    double minZ = double.MaxValue; foreach (var p in ghostPts) if (p.Z < minZ) minZ = p.Z;
                    for (int i = 0; i < numGhostNodes; i++)
                    {
                        if (Math.Abs(ghostPts[i].Z - minZ) < 10)
                        {
                            bool alreadyFixedHere = false;
                            foreach (var fp in fixedLocations) if (fp.DistanceTo(ghostPts[i]) < 1.0) { alreadyFixedHere = true; break; }
                            if (!alreadyFixedHere) { tcl.AppendLine($"fix {i + 1} 1 1 1 1 1 1;"); fixedLocations.Add(ghostPts[i]); }
                        }
                    }
                }

                // 4. MAGIA: MUELLES ZEROLENGTH
                tcl.AppendLine("\n# --- JOINTS (ZERO-LENGTH SPRINGS) ---");
                int springEleId = originalElements.Count * 10000 + 1;
                int matIdBase = 30000;
                totalSpringsGenerated = 0;
                sumKser = 0;

                for (int i = 0; i < numGhostNodes; i++)
                {
                    for (int j = i + 1; j < numGhostNodes; j++)
                    {
                        if (ghostPts[i].DistanceTo(ghostPts[j]) < 5.0)
                        {
                            CLTJointData activeJoint = null;
                            foreach (var joint in joints)
                            {
                                if (DistanceToCurve(joint.TargetLine, ghostPts[i]) < 100.0) { activeJoint = joint; break; }
                            }

                            if (activeJoint != null)
                            {
                                double tribLength_mm = Math.Sqrt(ghostNodeAreas[i] * 2.0);
                                if (tribLength_mm < 10.0) tribLength_mm = 150.0;
                                double tribLength_m = tribLength_mm / 1000.0;

                                double kserInput = activeJoint.Properties.Kser_PerMeter;
                                sumKser += kserInput;
                                double K_spring = kserInput * tribLength_m;
                                if (K_spring < 0.0001) K_spring = 0.0001;

                                int matID = matIdBase++;
                                tcl.AppendLine(string.Format(culture, "uniaxialMaterial Elastic {0} {1:F6};", matID, K_spring));
                                int rotMatID = matIdBase++;
                                tcl.AppendLine(string.Format(culture, "uniaxialMaterial Elastic {0} 1e6;", rotMatID)); // Anti-trampillas

                                tcl.AppendLine($"element zeroLength {springEleId++} {i + 1} {j + 1} -mat {matID} {matID} {matID} {rotMatID} {rotMatID} {rotMatID} -dir 1 2 3 4 5 6;");
                                totalSpringsGenerated++;
                            }
                        }
                    }
                }
                tcl.AppendLine("# ------------------------------------\n");

                // 5. SHELLS
                for (int i = 0; i < ghostMesh.Faces.Count; i++)
                {
                    int o = ghostOwners[i];
                    MeshFace f = ghostMesh.Faces[i];
                    int n1 = f.A + 1, n2 = f.B + 1, n3 = f.C + 1, n4 = f.IsQuad ? f.D + 1 : f.C + 1;
                    Vector3d ex = ghostPts[f.B] - ghostPts[f.A]; ex.Unitize();
                    Vector3d fd = originalElements[o].Properties.FiberDirection; if (fd.Length < 0.001) fd = Vector3d.ZAxis; fd.Unitize();
                    int sec = (Math.Abs(ex * fd) > 0.707) ? elemToSecID[o] : elemToSecID[o] + 1;
                    if (f.IsQuad) tcl.AppendLine($"element ShellMITC4 {i + 1} {n1} {n2} {n3} {n4} {sec};");
                    else tcl.AppendLine($"element ShellMITC4 {i + 1} {n1} {n2} {n3} {n3} {sec};");
                }

                // 6. CARGAS
                tcl.AppendLine("timeSeries Linear 1; pattern Plain 1 1 {");
                Dictionary<string, double> loadFactors = ParseLoadCombination(rule);

                double swFactor = 1.0;
                if (!string.IsNullOrWhiteSpace(rule))
                {
                    swFactor = 0.0;
                    if (loadFactors.ContainsKey("SelfWeight")) swFactor = loadFactors["SelfWeight"];
                    if (loadFactors.ContainsKey("PP")) swFactor = loadFactors["PP"];
                }
                for (int i = 0; i < numGhostNodes; i++)
                {
                    if (selfWeightForces[i].Length > 1e-8)
                    {
                        Vector3d sw = selfWeightForces[i] * swFactor;
                        tcl.AppendLine(string.Format(culture, "load {0} 0 0 {1:F4} 0 0 0;", i + 1, sw.Z));
                    }
                }

                HashSet<string> appliedLoadsCache = new HashSet<string>();
                HashSet<string> specificTargetSignatures = new HashSet<string>();

                foreach (var l in loads)
                {
                    if (l.Type == LoadType.Mesh)
                    {
                        if (l.TargetElementID != null && l.TargetElementID.Trim().Equals("<empty>", StringComparison.OrdinalIgnoreCase)) l.TargetElementID = "IGNORE_THIS_LOAD";
                        if (!string.IsNullOrWhiteSpace(l.TargetElementID) && l.TargetElementID != "IGNORE_THIS_LOAD") specificTargetSignatures.Add($"{l.LoadCaseName}_{l.Vector.X:F4}_{l.Vector.Y:F4}_{l.Vector.Z:F4}");
                        else if (l.TargetMesh != null) specificTargetSignatures.Add($"{l.LoadCaseName}_{l.Vector.X:F4}_{l.Vector.Y:F4}_{l.Vector.Z:F4}");
                    }
                }

                foreach (var load in loads)
                {
                    double factor = 1.0;
                    if (!string.IsNullOrWhiteSpace(rule))
                    {
                        factor = 0.0;
                        string name = load.LoadCaseName ?? "LoadCase1";
                        if (loadFactors.ContainsKey(name)) factor = loadFactors[name];
                    }
                    if (Math.Abs(factor) < 1e-9) continue;
                    if (load.TargetElementID == "IGNORE_THIS_LOAD") continue;
                    string loadHash = $"{load.LoadCaseName}_{load.Vector.X:F4}_{load.Vector.Y:F4}_{load.Vector.Z:F4}_{load.TargetElementID}";
                    if (!appliedLoadsCache.Add(loadHash)) continue;

                    if (load.Type == LoadType.Point)
                    {
                        int closestID = -1; double minDist = double.MaxValue;
                        for (int i = 0; i < numGhostNodes; i++) { double d = ghostPts[i].DistanceTo(load.TargetPoint); if (d < minDist) { minDist = d; closestID = i + 1; } }
                        if (minDist < 100.0) tcl.AppendLine(string.Format(culture, "load {0} {1:F4} {2:F4} {3:F4} 0 0 0;", closestID, load.Vector.X * factor, load.Vector.Y * factor, load.Vector.Z * factor));
                    }
                    else if (load.Type == LoadType.Mesh)
                    {
                        bool isGlobal = string.IsNullOrWhiteSpace(load.TargetElementID) && load.TargetMesh == null;
                        if (isGlobal && specificTargetSignatures.Contains($"{load.LoadCaseName}_{load.Vector.X:F4}_{load.Vector.Y:F4}_{load.Vector.Z:F4}")) continue;

                        Vector3d P = load.Vector * factor;
                        bool hasTextFilter = !string.IsNullOrWhiteSpace(load.TargetElementID);
                        HashSet<int> validNodes = new HashSet<int>();

                        if (hasTextFilter)
                        {
                            HashSet<string> targetIDs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            string[] splits = load.TargetElementID.Split(new char[] { '\n', '\r', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in splits) { string trimmed = s.Trim(); if (!string.IsNullOrEmpty(trimmed)) targetIDs.Add(trimmed); }
                            for (int fIdx = 0; fIdx < ghostMesh.Faces.Count; fIdx++)
                            {
                                int ownerIdx = ghostOwners[fIdx];
                                string panelID = originalElements[ownerIdx].Properties != null ? originalElements[ownerIdx].Properties.ID : $"Elem_{ownerIdx}";
                                if (targetIDs.Contains(panelID))
                                {
                                    MeshFace face = ghostMesh.Faces[fIdx];
                                    validNodes.Add(face.A); validNodes.Add(face.B); validNodes.Add(face.C);
                                    if (face.IsQuad) validNodes.Add(face.D);
                                }
                            }
                        }

                        for (int i = 0; i < numGhostNodes; i++)
                        {
                            bool apply = true;
                            if (load.TargetMesh != null) { if (load.TargetMesh.ClosestPoint(ghostPts[i]).DistanceTo(ghostPts[i]) > 10.0) apply = false; }
                            else if (hasTextFilter) { if (!validNodes.Contains(i)) apply = false; }
                            else if (isGlobal) apply = true; else apply = false;

                            if (apply)
                            {
                                Vector3d F = P * ghostNodeAreas[i];
                                if (F.Length > 1e-12) tcl.AppendLine(string.Format(culture, "load {0} {1:F4} {2:F4} {3:F4} 0 0 0;", i + 1, F.X, F.Y, F.Z));
                            }
                        }
                    }
                }

                // 7. SISMO (Universal)
                bool isSeismic = (rule.IndexOf("Seismic", StringComparison.OrdinalIgnoreCase) >= 0 || rule.IndexOf("Sismo", StringComparison.OrdinalIgnoreCase) >= 0);
                if (isSeismic && sDataObj != null)
                {
                    double dirX = 0.0, dirY = 0.0;
                    if (rule.IndexOf("Y", StringComparison.OrdinalIgnoreCase) >= 0) dirY = 1.0; else dirX = 1.0;
                    double ab = GetDoubleVal(sDataObj, "ab"); double K = GetDoubleVal(sDataObj, "K", 1.0); double C = GetDoubleVal(sDataObj, "C", 1.0); double Rho = GetDoubleVal(sDataObj, "Rho", 1.0);

                    if (ab > 0)
                    {
                        double accel_ms2 = (ab * K * C * Rho) * 9.81;
                        for (int i = 0; i < numGhostNodes; i++)
                        {
                            double mass = nodeMasses[i];
                            double F = mass * accel_ms2;
                            if (F > 1e-6) tcl.AppendLine(string.Format(culture, "load {0} {1:F4} {2:F4} 0 0 0 0;", i + 1, F * dirX, F * dirY));
                        }
                    }
                }

                tcl.AppendLine("}");

                // 8. ANÁLISIS (Newton->Linear)
                tcl.AppendLine("system UmfPack; numberer RCM; constraints Plain; integrator LoadControl 1.0;");
                tcl.AppendLine("test NormDispIncr 1.0e-5 50 0; algorithm Newton; analysis Static;");
                tcl.AppendLine("if { [analyze 1] != 0 } { puts \"Newton failed, using Linear...\"; algorithm Linear; analyze 1; }");

                // 9. RESULTADOS DE DESPLAZAMIENTOS
                string rFile = $"res_{c}.txt";
                tcl.AppendLine($"set f [open \"{rFile}\" \"w\"];");
                tcl.AppendLine("foreach n [getNodeTags] { puts $f \"$n [format \"%.10f\" [nodeDisp $n 1]] [format \"%.10f\" [nodeDisp $n 2]] [format \"%.10f\" [nodeDisp $n 3]] [format \"%.10f\" [nodeDisp $n 4]] [format \"%.10f\" [nodeDisp $n 5]] [format \"%.10f\" [nodeDisp $n 6]]\"; }");
                tcl.AppendLine("close $f;");
            }
            tcl.AppendLine("exit;");

            string tclPath = Path.Combine(tempFolder, "run.tcl");
            File.WriteAllText(tclPath, tcl.ToString());
            string batP = Path.Combine(tempFolder, "go.bat");
            File.WriteAllText(batP, $@"@echo off
set TCL_LIBRARY={Path.GetDirectoryName(exePath)}
cd /d ""{tempFolder}""
""{exePath}"" ""{tclPath}""
");
            ProcessStartInfo psi = new ProcessStartInfo(batP) { WorkingDirectory = tempFolder, CreateNoWindow = true, UseShellExecute = false };
            try { Process.Start(psi).WaitForExit(); } catch { }

            for (int c = 0; c < combRules.Count; c++)
            {
                string rFile = Path.Combine(tempFolder, $"res_{c}.txt");
                GH_Path path = new GH_Path(c);
                if (File.Exists(rFile))
                {
                    string[] lines = File.ReadAllLines(rFile);
                    foreach (string l in lines)
                    {
                        var p = l.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (p.Length >= 7)
                        {
                            double x = double.Parse(p[1], culture);
                            double y = double.Parse(p[2], culture);
                            double z = double.Parse(p[3], culture);
                            outVectors.Append(new GH_Vector(new Vector3d(x, y, z)), path);
                            double rx = double.Parse(p[4], culture);
                            double ry = double.Parse(p[5], culture);
                            double rz = double.Parse(p[6], culture);
                            outRotations.Append(new GH_Vector(new Vector3d(rx, ry, rz)), path);
                        }
                    }
                }
                else
                {
                    finalLog.AppendLine($"ERROR: File res_{c}.txt not generated.");
                }
            }

            string csvPath = Path.Combine(tempFolder, "Analysis_Results.csv");
            if (writeCSV)
            {
                try
                {
                    StringBuilder csv = new StringBuilder();
                    csv.AppendLine("Joint,OutputCase,CaseType,StepType,U1,U2,U3,R1,R2,R3");
                    csv.AppendLine("Text,Text,Text,Text,mm,mm,mm,Radians,Radians,Radians");

                    for (int c = 0; c < combRules.Count; c++)
                    {
                        string combName = combRules[c];
                        GH_Path path = new GH_Path(c);
                        var branch = outVectors.get_Branch(path);
                        var branchRot = outRotations.get_Branch(path);

                        if (branch != null)
                        {
                            for (int i = 0; i < branch.Count; i++)
                            {
                                GH_Vector disp = (GH_Vector)branch[i];
                                GH_Vector rot = (GH_Vector)branchRot[i];
                                int nodeID = i + 1;
                                string line = string.Format(culture, "{0},{1},{2},{3},{4:F6},{5:F6},{6:F6},{7:F6},{8:F6},{9:F6}",
                                    nodeID, combName, "Combination", "Static",
                                    disp.Value.X, disp.Value.Y, disp.Value.Z,
                                    rot.Value.X, rot.Value.Y, rot.Value.Z);
                                csv.AppendLine(line);
                            }
                        }
                    }
                    File.WriteAllText(csvPath, csv.ToString());
                    finalLog.AppendLine($"CSV Export: Success -> {csvPath}");
                }
                catch (Exception ex)
                {
                    finalLog.AppendLine($"CSV Export ERROR: {ex.Message}");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not write CSV. Close the file if open in Excel.");
                }
            }

            // AÑADIMOS EL REPORTE FINAL DE AUDITORÍA DE MUELLES
            finalLog.AppendLine($"Topology Check: Successfully generated {totalSpringsGenerated} zeroLength springs in OpenSees.");

            stopwatch.Stop();
            DateTime endTime = DateTime.Now;

            StringBuilder report = new StringBuilder();
            report.AppendLine("=== DISP. SOLVER REPORT ===");
            report.AppendLine("");
            report.AppendLine($"Start Time:   {startTime:HH:mm:ss}");
            report.AppendLine($"End Time:     {endTime:HH:mm:ss}");
            report.AppendLine($"Duration:     {stopwatch.Elapsed.TotalSeconds:F2} seconds");
            report.AppendLine("");
            report.AppendLine($"Total Nodes:    {numGhostNodes}");
            report.AppendLine($"Total Elements: {ghostMesh.Faces.Count} (Shells)");
            report.AppendLine("");
            report.AppendLine("Analysis Files Directory:");
            report.AppendLine(tclPath);
            if (writeCSV)
            {
                report.AppendLine("");
                report.AppendLine("Excel/CSV Output:");
                report.AppendLine(csvPath);
            }
            report.AppendLine("=========================");
            if (finalLog.Length > 0) { report.AppendLine(""); report.AppendLine("--- DETAILS ---"); report.Append(finalLog.ToString()); }

            List<string> ids = new List<string>();
            foreach (int o in ghostOwners) ids.Add(originalElements[o].Properties.ID);
            ghostMesh.UserDictionary.Set("CLT_CaseNames", combRules);
            ghostMesh.UserDictionary.Set("CLT_ElementIDs", ids);

            DA.SetData(0, ghostMesh);
            DA.SetDataTree(1, outVectors);
            DA.SetData(2, report.ToString());
        }

        private double GetDoubleVal(object obj, string name, double defVal = 0.0)
        {
            if (obj == null) return defVal;
            Type t = obj.GetType();
            foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    try { return Convert.ToDouble(prop.GetValue(obj)); } catch { }
                }
            }
            foreach (var field in t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (string.Equals(field.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    try { return Convert.ToDouble(field.GetValue(obj)); } catch { }
                }
            }
            return defVal;
        }

        private double[] CalculateNodeMasses(Mesh mesh, List<int> owners, List<CLTElement> elements)
        {
            int numNodes = mesh.Vertices.Count;
            double[] masses = new double[numNodes];
            Point3d[] pts = mesh.Vertices.ToPoint3dArray();
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                MeshFace f = mesh.Faces[i];
                double area = 0.5 * Vector3d.CrossProduct(pts[f.B] - pts[f.A], pts[f.C] - pts[f.A]).Length;
                if (f.IsQuad) area += 0.5 * Vector3d.CrossProduct(pts[f.C] - pts[f.A], pts[f.D] - pts[f.A]).Length;
                CLTElement owner = elements[owners[i]];
                double thickness = owner.Properties.Thicknesses.Sum();
                double volume_m3 = (area * 1e-6) * (thickness * 1e-3);
                double elemMass = volume_m3 * owner.Material.Rho;
                int nodesInFace = f.IsQuad ? 4 : 3;
                double massPerNode = elemMass / nodesInFace;
                masses[f.A] += massPerNode; masses[f.B] += massPerNode; masses[f.C] += massPerNode;
                if (f.IsQuad) masses[f.D] += massPerNode;
            }
            return masses;
        }

        private Dictionary<string, double> ParseLoadCombination(string rule)
        {
            Dictionary<string, double> factors = new Dictionary<string, double>();
            if (string.IsNullOrWhiteSpace(rule)) return factors;
            string[] parts = rule.Split('+');
            foreach (string part in parts)
            {
                string p = part.Trim(); if (string.IsNullOrEmpty(p)) continue;
                double factor = 1.0; string name = p;
                if (p.Contains("*")) { string[] factorName = p.Split('*'); if (double.TryParse(factorName[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double val)) factor = val; if (factorName.Length > 1) name = factorName[1].Trim(); }
                factors[name] = factor;
            }
            return factors;
        }

        // --- JOINT AWARE WELD ---
        private Mesh CustomWeld(Mesh source, List<int> sourceTags, List<int> vertexOwners, List<CLTJointData> joints, out List<int> weldedTags, double tolerance)
        {
            List<Point3d> uniquePts = new List<Point3d>();
            List<int> uniquePtsOwners = new List<int>();
            int[] mapOldToNew = new int[source.Vertices.Count];

            for (int i = 0; i < source.Vertices.Count; i++)
            {
                Point3d pt = source.Vertices[i];
                int owner = vertexOwners.Count > i ? vertexOwners[i] : -1;

                bool onJoint = false;
                if (joints != null)
                {
                    foreach (var j in joints)
                    {
                        // Utilizamos la nueva función de distancia infinita (tolerancia de 100mm)
                        if (DistanceToCurve(j.TargetLine, pt) < 2.0)
                        {
                            onJoint = true; break;
                        }
                    }
                }

                int found = -1;
                for (int j = 0; j < uniquePts.Count; j++)
                {
                    if (uniquePts[j].DistanceTo(pt) < tolerance)
                    {
                        if (!onJoint || uniquePtsOwners[j] == owner)
                        {
                            found = j;
                            break;
                        }
                    }
                }

                if (found != -1)
                {
                    mapOldToNew[i] = found;
                }
                else
                {
                    uniquePts.Add(pt);
                    uniquePtsOwners.Add(owner);
                    mapOldToNew[i] = uniquePts.Count - 1;
                }
            }

            Mesh newMesh = new Mesh();
            newMesh.Vertices.AddVertices(uniquePts);
            weldedTags = new List<int>();

            for (int i = 0; i < source.Faces.Count; i++)
            {
                MeshFace f = source.Faces[i];
                int a = mapOldToNew[f.A]; int b = mapOldToNew[f.B]; int c = mapOldToNew[f.C]; int d = mapOldToNew[f.D];
                if (f.IsQuad && a != b && b != c && c != d && d != a) { newMesh.Faces.AddFace(a, b, c, d); weldedTags.Add(sourceTags[i]); }
                else if (!f.IsQuad && a != b && b != c && c != a) { newMesh.Faces.AddFace(a, b, c); weldedTags.Add(sourceTags[i]); }
            }
            return newMesh;
        }

        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.DisplacementSolver_icon;
        public override Guid ComponentGuid => new Guid("ffff8888-1111-2222-3333-555555555555");
    }
}
