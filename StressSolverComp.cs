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
    public class StressSolverComp : GH_Component
    {
        public StressSolverComp()
          : base("Internal Stress Solver", "Forces",
              "Calculates internal forces (N, M, V) and Principal Forces of the given model.",
              "CLT Tools", "4 :: Algorithms")
        {

        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Model", "M", "CLT model.", GH_ParamAccess.item);
            pManager.AddTextParameter("LCase-Comb", "Comb", "Rules of combining LoadCases (fe: '1.35*PP + 1.5*Live').", GH_ParamAccess.list);
            pManager.AddGenericParameter("Seismic Data", "SData", "Seismic Data (Optional).", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write CSV", "W", "Set to TRUE to export results to CSV.", GH_ParamAccess.item, false);
            

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Analysed Model", "Mesh", "Modeled mesh with results.", GH_ParamAccess.item);
            pManager.AddVectorParameter("Top Face Stresses", "STop", "Tree: (S11, S22, S12) in MPa on Top Face", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Bottom Face Stresses", "SBot", "Tree: (S11, S22, S12) in MPa on Bottom Face", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Shear Stresses", "SShear", "Tree: (S13, S23, 0) in MPa", GH_ParamAccess.tree);
            
            pManager.AddTextParameter("Log", "Log", "Informe del solver.", GH_ParamAccess.item);
        }

        private double DistanceToCurve(Curve crv, Point3d pt)
        {
            if (crv == null) return double.MaxValue;
            if (crv.IsLinear())
            {
                Line l = new Line(crv.PointAtStart, crv.PointAtEnd);
                return l.ClosestPoint(pt, false).DistanceTo(pt); // FIXED: true=finita, no infinita
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
            foreach (string f in Directory.GetFiles(tempFolder, "res_force_*.txt")) { try { File.Delete(f); } catch { } }

            GH_ObjectWrapper modelWrap = null;
            List<string> combRules = new List<string>();
            GH_ObjectWrapper seismicWrap = null;
            bool writeCSV = false;

            if (!DA.GetData(0, ref modelWrap)) return;
            if (!DA.GetDataList(1, combRules)) combRules.Add("1.0*PP");
            DA.GetData(2, ref seismicWrap);
            DA.GetData(3, ref writeCSV);
            

            CLTModel model = modelWrap.Value as CLTModel;
            if (model == null) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Modelo nulo."); return; }

            object sDataObj = null;
            if (seismicWrap != null)
            {
                object raw = seismicWrap.Value;
                if (raw is GH_ObjectWrapper wrapper) sDataObj = wrapper.Value;
                else if (raw is IGH_Goo goo) sDataObj = goo.ScriptVariable();
                else sDataObj = raw;
            }

            List<CLTElement> elements = model.Elements;
            List<CLTLoadData> loads = model.Loads;
            List<CLTSupportData> supports = model.Supports;

            List<CLTJointData> joints = new List<CLTJointData>();
            try
            {
                PropertyInfo prop = model.GetType().GetProperty("Joints");
                if (prop != null)
                {
                    System.Collections.IEnumerable jointsList = prop.GetValue(model) as System.Collections.IEnumerable;
                    if (jointsList != null) foreach (var j in jointsList) joints.Add((CLTJointData)j);
                }
            }
            catch { }

            if (elements.Count == 0) return;

            string exePath = @"C:\OpenSees\OpenSees.exe";
            if (!File.Exists(exePath)) exePath = @"C:\OpenSees\bin\OpenSees.exe";
            if (!File.Exists(exePath)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "OpenSees not found."); return; }

            Mesh rawMesh = new Mesh();
            List<int> faceOwners = new List<int>();
            List<int> vertexOwners = new List<int>();

            for (int i = 0; i < elements.Count; i++)
            {
                Mesh m = elements[i].Geometry as Mesh;
                if (m == null || !m.IsValid) continue;
                rawMesh.Append(m);
                for (int k = 0; k < m.Faces.Count; k++) faceOwners.Add(i);
                for (int k = 0; k < m.Vertices.Count; k++) vertexOwners.Add(i);
            }

            List<int> globalFaceOwners;
            Mesh globalMesh = CustomWeld(rawMesh, faceOwners, vertexOwners, joints, out globalFaceOwners, 5.0);

            globalMesh.FaceNormals.ComputeFaceNormals();
            globalMesh.Compact();
            int numNodes = globalMesh.Vertices.Count;
            int numElems = globalMesh.Faces.Count;

            finalLog.AppendLine($"Topology Check: Detected {joints.Count} Joint definitions.");

            double[] nodeAreas = new double[numNodes];
            for (int i = 0; i < globalMesh.Faces.Count; i++)
            {
                MeshFace f = globalMesh.Faces[i];
                Point3d A = globalMesh.Vertices[f.A]; Point3d B = globalMesh.Vertices[f.B]; Point3d C = globalMesh.Vertices[f.C];
                double area = 0.5 * Vector3d.CrossProduct(B - A, C - A).Length;
                if (f.IsQuad)
                {
                    Point3d D = globalMesh.Vertices[f.D];
                    area += 0.5 * Vector3d.CrossProduct(C - A, D - A).Length;
                    double q = area * 0.25;
                    nodeAreas[f.A] += q; nodeAreas[f.B] += q; nodeAreas[f.C] += q; nodeAreas[f.D] += q;
                }
                else
                {
                    double t = area * 0.3333;
                    nodeAreas[f.A] += t; nodeAreas[f.B] += t; nodeAreas[f.C] += t;
                }
            }

            List<Point3d> pts = new List<Point3d>();
            for (int i = 0; i < numNodes; i++) pts.Add(globalMesh.Vertices[i]);

            Vector3d[] selfWeightForces = new Vector3d[numNodes];
            double[] elemThickness = new double[numElems];
            for (int i = 0; i < globalMesh.Faces.Count; i++)
            {
                if (globalMesh.Faces[i].IsQuad)
                {
                    int ownerIdx = globalFaceOwners[i];
                    CLTElement owner = elements[ownerIdx];
                    int n1 = globalMesh.Faces[i].A; int n2 = globalMesh.Faces[i].B; int n3 = globalMesh.Faces[i].C; int n4 = globalMesh.Faces[i].D;
                    double area = 0.5 * Vector3d.CrossProduct(pts[n2] - pts[n1], pts[n3] - pts[n1]).Length + 0.5 * Vector3d.CrossProduct(pts[n3] - pts[n1], pts[n4] - pts[n1]).Length;
                    double thickness_mm = 0;
                    if (owner.Properties.Thicknesses != null) foreach (double t in owner.Properties.Thicknesses) thickness_mm += t;
                    elemThickness[i] = (thickness_mm > 0) ? thickness_mm : 1.0;
                    double weight_N = (area * thickness_mm * 1e-9) * owner.Material.Rho * 9.81;
                    Vector3d fNode = new Vector3d(0, 0, -weight_N / 4.0);
                    selfWeightForces[n1] += fNode; selfWeightForces[n2] += fNode; selfWeightForces[n3] += fNode; selfWeightForces[n4] += fNode;
                }
            }

            List<int> fullyFixedNodes = new List<int>();
            var culture = CultureInfo.InvariantCulture;
            StringBuilder tcl = new StringBuilder();

            // --- 1. CACHÉ DE MATERIALES Y SECCIONES ---
            Dictionary<int, int> elemToSecID = new Dictionary<int, int>();
            int secCounter = 1000;
            HashSet<string> definedMats = new HashSet<string>();
            StringBuilder matBlock = new StringBuilder();

            for (int i = 0; i < elements.Count; i++)
            {
                CLTElement el = elements[i];
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
                for (int i = 0; i < numNodes; i++)
                {
                    tcl.AppendLine(string.Format(culture, "node {0} {1:F4} {2:F4} {3:F4};", i + 1, pts[i].X, pts[i].Y, pts[i].Z));
                }

                // 3. APOYOS FÍSICOS
                for (int i = 0; i < numNodes; i++) tcl.AppendLine($"fix {i + 1} 0 0 0 0 0 1;");

                fullyFixedNodes.Clear();
                List<Point3d> fixedLocations = new List<Point3d>();
                if (supports.Count > 0)
                {
                    for (int i = 0; i < numNodes; i++)
                    {
                        foreach (var sup in supports)
                        {
                            if (pts[i].DistanceTo(sup.Location) < 20.0)
                            {
                                bool alreadyFixedHere = false;
                                foreach (var fp in fixedLocations) if (fp.DistanceTo(pts[i]) < 1.0) { alreadyFixedHere = true; break; }
                                if (!alreadyFixedHere) { tcl.AppendLine($"fix {i + 1} {sup.FixString};"); fixedLocations.Add(pts[i]); if (sup.FixString.Replace(" ", "") == "111111") fullyFixedNodes.Add(i); }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    double minZ = double.MaxValue; foreach (var p in pts) if (p.Z < minZ) minZ = p.Z;
                    for (int i = 0; i < numNodes; i++)
                    {
                        if (Math.Abs(pts[i].Z - minZ) < 10)
                        {
                            bool alreadyFixedHere = false;
                            foreach (var fp in fixedLocations) if (fp.DistanceTo(pts[i]) < 1.0) { alreadyFixedHere = true; break; }
                            if (!alreadyFixedHere) { tcl.AppendLine($"fix {i + 1} 1 1 1 1 1 1;"); fixedLocations.Add(pts[i]); fullyFixedNodes.Add(i); }
                        }
                    }
                }

                // 4. MUELLES ZEROLENGTH
                tcl.AppendLine("\n# --- JOINTS (ZERO-LENGTH SPRINGS) ---");
                int springEleId = elements.Count * 10000 + 1;
                int matIdBase = 30000;
                totalSpringsGenerated = 0;
                sumKser = 0;

                for (int i = 0; i < numNodes; i++)
                {
                    for (int j = i + 1; j < numNodes; j++)
                    {
                        if (pts[i].DistanceTo(pts[j]) < 5.0)
                        {
                            CLTJointData activeJoint = null;
                            foreach (var joint in joints)
                            {
                                if (DistanceToCurve(joint.TargetLine, pts[i]) < 100.0) { activeJoint = joint; break; }
                            }

                            if (activeJoint != null)
                            {
                                double tribLength_mm = Math.Sqrt(nodeAreas[i] * 2.0);
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
                for (int i = 0; i < globalMesh.Faces.Count; i++)
                {
                    if (globalMesh.Faces[i].IsQuad)
                    {
                        int ownerIdx = globalFaceOwners[i];
                        CLTElement owner = elements[ownerIdx];
                        int id = ownerIdx + 1;
                        int n1 = globalMesh.Faces[i].A; int n2 = globalMesh.Faces[i].B; int n3 = globalMesh.Faces[i].C; int n4 = globalMesh.Faces[i].D;
                        Vector3d elemX = pts[n2] - pts[n1]; elemX.Unitize();
                        double dot = Math.Abs(Vector3d.Multiply(elemX, owner.Properties.FiberDirection));
                        int sectionID = (dot > 0.707) ? elemToSecID[ownerIdx] : elemToSecID[ownerIdx] + 1;
                        tcl.AppendLine($"element ShellMITC4 {i + 1} {n1 + 1} {n2 + 1} {n3 + 1} {n4 + 1} {sectionID};");
                    }
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
                for (int i = 0; i < numNodes; i++)
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
                        for (int i = 0; i < numNodes; i++) { double d = pts[i].DistanceTo(load.TargetPoint); if (d < minDist) { minDist = d; closestID = i + 1; } }
                        Vector3d force = (load.Vector * factor) * 1000.0;
                        if (minDist < 100.0) tcl.AppendLine(string.Format(culture, "load {0} {1:F4} {2:F4} {3:F4} 0 0 0;", closestID, force.X, force.Y, force.Z));
                    }
                    else if (load.Type == LoadType.Mesh)
                    {
                        bool isGlobal = string.IsNullOrWhiteSpace(load.TargetElementID) && load.TargetMesh == null;
                        if (isGlobal && specificTargetSignatures.Contains($"{load.LoadCaseName}_{load.Vector.X:F4}_{load.Vector.Y:F4}_{load.Vector.Z:F4}")) continue;

                        Vector3d pressure_MPa = (load.Vector * factor) / 1000.0;
                        bool hasTextFilter = !string.IsNullOrWhiteSpace(load.TargetElementID);
                        HashSet<int> validNodes = new HashSet<int>();

                        if (hasTextFilter)
                        {
                            HashSet<string> targetIDs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            string[] splits = load.TargetElementID.Split(new char[] { '\n', '\r', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in splits) { string trimmed = s.Trim(); if (!string.IsNullOrEmpty(trimmed)) targetIDs.Add(trimmed); }
                            for (int fIdx = 0; fIdx < globalMesh.Faces.Count; fIdx++)
                            {
                                int ownerIdx = globalFaceOwners[fIdx];
                                string panelID = elements[ownerIdx].Properties != null ? elements[ownerIdx].Properties.ID : $"Elem_{ownerIdx}";
                                if (targetIDs.Contains(panelID))
                                {
                                    MeshFace face = globalMesh.Faces[fIdx];
                                    validNodes.Add(face.A); validNodes.Add(face.B); validNodes.Add(face.C);
                                    if (face.IsQuad) validNodes.Add(face.D);
                                }
                            }
                        }

                        for (int i = 0; i < numNodes; i++)
                        {
                            if (fullyFixedNodes.Contains(i)) continue;
                            bool apply = true;
                            if (load.TargetMesh != null) { if (load.TargetMesh.ClosestPoint(pts[i]).DistanceTo(pts[i]) > 5.0) apply = false; }
                            else if (hasTextFilter) { if (!validNodes.Contains(i)) apply = false; }
                            else if (isGlobal) apply = true; else apply = false;

                            if (apply)
                            {
                                Vector3d f = pressure_MPa * nodeAreas[i];
                                if (f.Length > 1e-12) tcl.AppendLine(string.Format(culture, "load {0} {1:F4} {2:F4} {3:F4} 0 0 0;", i + 1, f.X, f.Y, f.Z));
                            }
                        }
                    }
                }

                // 7. SISMO
                bool isSeismic = (rule.IndexOf("Seismic", StringComparison.OrdinalIgnoreCase) >= 0 || rule.IndexOf("Sismo", StringComparison.OrdinalIgnoreCase) >= 0);
                if (isSeismic && sDataObj != null)
                {
                    double dirX = 0.0, dirY = 0.0;
                    if (rule.IndexOf("Y", StringComparison.OrdinalIgnoreCase) >= 0) dirY = 1.0; else dirX = 1.0;
                    double ab = GetDoubleVal(sDataObj, "ab"); double K = GetDoubleVal(sDataObj, "K", 1.0); double C = GetDoubleVal(sDataObj, "C", 1.0); double Rho = GetDoubleVal(sDataObj, "Rho", 1.0);
                    if (ab > 0)
                    {
                        double accel_ms2 = (ab * K * C * Rho) * 9.81;
                        for (int i = 0; i < numNodes; i++)
                        {
                            double mass = Math.Abs(selfWeightForces[i].Z) / 9.81; // Sacamos la masa inversa del peso
                            double F = mass * accel_ms2;
                            if (F > 1e-6) tcl.AppendLine(string.Format(culture, "load {0} {1:F4} {2:F4} 0 0 0 0;", i + 1, F * dirX, F * dirY));
                        }
                    }
                }

                tcl.AppendLine("}");

                // 8. ANÁLISIS (Con Newton->Linear)
                tcl.AppendLine("system UmfPack; numberer RCM; constraints Plain; integrator LoadControl 1.0;");
                tcl.AppendLine("test NormDispIncr 1.0e-5 50 0; algorithm Newton; analysis Static;");
                tcl.AppendLine("if { [analyze 1] != 0 } { puts \"Newton failed, using Linear...\"; algorithm Linear; analyze 1; }");

                // 9. RESULTADOS DE FUERZAS
                string resFileTcl = $"res_force_{c}.txt";
                tcl.AppendLine($"set f [open \"{resFileTcl}\" \"w\"];");
                tcl.AppendLine("foreach e [getEleTags] { puts $f \"$e [eleResponse $e section 1 force]\"; }");
                tcl.AppendLine("close $f;");
            }

            tcl.AppendLine("exit;");

            string tclPath = Path.Combine(tempFolder, "run_forces.tcl");
            string csvPath = Path.Combine(tempFolder, "Stress_Results.csv");
            System.Text.StringBuilder csv = new System.Text.StringBuilder();
            if (writeCSV)
            {
                csv.AppendLine("Element,OutputCase,CaseType,StepType,S11Top,S22Top,S12Top,SMaxTop,SMinTop,S11Bot,S22Bot,S12Bot,SMaxBot,SMinBot,S13,S23");
                csv.AppendLine("Text,Text,Text,Text,MPa,MPa,MPa,MPa,MPa,MPa,MPa,MPa,MPa,MPa,MPa,MPa");
            }
            File.WriteAllText(tclPath, tcl.ToString());

            string exeDir = Path.GetDirectoryName(exePath);
            string batPath = Path.Combine(tempFolder, "go_forces.bat");
            string batContent = $@"@echo off
set TCL_LIBRARY={exeDir}
cd /d ""{tempFolder}""
""{exePath}"" ""{tclPath}""
";
            File.WriteAllText(batPath, batContent);

            ProcessStartInfo psi = new ProcessStartInfo(batPath)
            {
                WorkingDirectory = tempFolder,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            try { Process.Start(psi).WaitForExit(); } catch { }

            GH_Structure<GH_Vector> outSTop = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Vector> outSBot = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Vector> outSShear = new GH_Structure<GH_Vector>();
            
            

            
            
            for (int c = 0; c < combRules.Count; c++)
            {
                string resFile = Path.Combine(tempFolder, $"res_force_{c}.txt");
                GH_Path path = new GH_Path(c);
                string combName = combRules[c];

                if (File.Exists(resFile))
                {
                    string[] lines = File.ReadAllLines(resFile);

                    Vector3d[] arrSTop = new Vector3d[numElems];
                      Vector3d[] arrSBot = new Vector3d[numElems];
                      Vector3d[] arrSShear = new Vector3d[numElems];
                      Vector3d[] arrPrinSTop = new Vector3d[numElems];
                      Vector3d[] arrPrinSBot = new Vector3d[numElems];

                    foreach (string line in lines)
                    {
                        string[] p = line.Trim().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        if (p.Length >= 9 && int.TryParse(p[0], out int id))
                        {
                            int idx = id - 1;
                            if (idx >= 0 && idx < numElems)
                            {
                                double f11 = double.Parse(p[1], culture);
                                  double f22 = double.Parse(p[2], culture);
                                  double f12 = double.Parse(p[3], culture);

                                  double m11_raw = double.Parse(p[4], culture);
                                  double m22_raw = double.Parse(p[5], culture);
                                  double m12_raw = double.Parse(p[6], culture);

                                  double v13 = double.Parse(p[7], culture);
                                  double v23 = double.Parse(p[8], culture);

                                  double t = elemThickness[idx];

                                  double s11Top = (f11 / t) + (6.0 * m11_raw) / (t * t);
                                  double s22Top = (f22 / t) + (6.0 * m22_raw) / (t * t);
                                  double s12Top = (f12 / t) + (6.0 * m12_raw) / (t * t);

                                  double s11Bot = (f11 / t) - (6.0 * m11_raw) / (t * t);
                                  double s22Bot = (f22 / t) - (6.0 * m22_raw) / (t * t);
                                  double s12Bot = (f12 / t) - (6.0 * m12_raw) / (t * t);

                                  double s13 = 1.5 * v13 / t;
                                  double s23 = 1.5 * v23 / t;

                                  double stAvg = (s11Top + s22Top) / 2.0;
                                  double stR = Math.Sqrt(Math.Pow((s11Top - s22Top) / 2.0, 2) + Math.Pow(s12Top, 2));
                                  double stMax = stAvg + stR;
                                  double stMin = stAvg - stR;
                                  double stAngle = 0.5 * Math.Atan2(2.0 * s12Top, s11Top - s22Top) * 180.0 / Math.PI;

                                  double sbAvg = (s11Bot + s22Bot) / 2.0;
                                  double sbR = Math.Sqrt(Math.Pow((s11Bot - s22Bot) / 2.0, 2) + Math.Pow(s12Bot, 2));
                                  double sbMax = sbAvg + sbR;
                                  double sbMin = sbAvg - sbR;
                                  double sbAngle = 0.5 * Math.Atan2(2.0 * s12Bot, s11Bot - s22Bot) * 180.0 / Math.PI;

                                  arrSTop[idx] = new Vector3d(s11Top, s22Top, s12Top);
                                  arrSBot[idx] = new Vector3d(s11Bot, s22Bot, s12Bot);
                                  arrSShear[idx] = new Vector3d(s13, s23, 0);
                                  arrPrinSTop[idx] = new Vector3d(stMax, stMin, stAngle);
                                  arrPrinSBot[idx] = new Vector3d(sbMax, sbMin, sbAngle);
                                  
                                  if (writeCSV)
                                  {
                                      string csvLine = string.Format(culture,
                                          "{0},{1},Combination,Static,{2:F4},{3:F4},{4:F4},{5:F4},{6:F4},{7:F4},{8:F4},{9:F4},{10:F4},{11:F4},{12:F4},{13:F4}",
                                          id, combName, s11Top, s22Top, s12Top, stMax, stMin, s11Bot, s22Bot, s12Bot, sbMax, sbMin, s13, s23);
                                      csv.AppendLine(csvLine);
                                  }

                                
                            }
                        }
                    }

                    foreach (var v in arrSTop) outSTop.Append(new GH_Vector(v), path);
                    foreach (var v in arrSBot) outSBot.Append(new GH_Vector(v), path);
                    foreach (var v in arrSShear) outSShear.Append(new GH_Vector(v), path);
                    
                    
                }
                else
                {
                    finalLog.AppendLine($"ERROR: No hay resultados para la combinación {c}.");
                }
            }

            

            finalLog.AppendLine($"Spring Generator: Successfully generated {totalSpringsGenerated} zeroLength springs.");
            if (totalSpringsGenerated > 0)
            {
                double avgKser = sumKser / totalSpringsGenerated;
                finalLog.AppendLine($"Average Kser_PerMeter read from inputs: {avgKser:F4}");
            }

            stopwatch.Stop();
            DateTime endTime = DateTime.Now;

            StringBuilder report = new StringBuilder();
            report.AppendLine("=== FORCES SOLVER REPORT ===");
            report.AppendLine("");
            report.AppendLine($"Start Time:   {startTime:HH:mm:ss}");
            report.AppendLine($"End Time:     {endTime:HH:mm:ss}");
            report.AppendLine($"Duration:     {stopwatch.Elapsed.TotalSeconds:F2} seconds");
            report.AppendLine("");
            report.AppendLine($"Total Nodes:    {numNodes}");
            report.AppendLine($"Total Elements: {numElems} (Shells)");
            report.AppendLine("");
            report.AppendLine("Analysis Files Directory:");
            report.AppendLine(tclPath);
            if (writeCSV)
            {
                try
                {
                    System.IO.File.WriteAllText(csvPath, csv.ToString());
                    finalLog.AppendLine($"CSV Export: Success -> {csvPath}");
                }
                catch (Exception ex)
                {
                    finalLog.AppendLine($"CSV Export ERROR: {ex.Message}");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No se pudo escribir el CSV. Cierralo si esta abierto en Excel.");
                }
                report.AppendLine("");
                report.AppendLine("Excel/CSV Output:");
                report.AppendLine(csvPath);
            }
            
            report.AppendLine("=========================");
            if (finalLog.Length > 0) { report.AppendLine(""); report.AppendLine("--- DETAILS ---"); report.Append(finalLog.ToString()); }

            List<string> faceIDs = new List<string>();
            for (int i = 0; i < globalMesh.Faces.Count; i++)
            {
                int ownerIdx = globalFaceOwners[i];
                var ownerProps = elements[ownerIdx].Properties;
                string panelID = (ownerProps != null && !string.IsNullOrEmpty(ownerProps.ID)) ? ownerProps.ID : $"Elem_{ownerIdx}";
                faceIDs.Add(panelID);
            }
            globalMesh.UserDictionary.Set("CLT_ElementIDs", faceIDs);
            globalMesh.UserDictionary.Set("CLT_CaseNames", combRules);

            DA.SetData(0, globalMesh);
            DA.SetDataTree(1, outSTop);
            DA.SetDataTree(2, outSBot);
            DA.SetDataTree(3, outSShear);
            DA.SetData(4, report.ToString());
        }

        private Dictionary<string, double> ParseLoadCombination(string rule)
        {
            Dictionary<string, double> factors = new Dictionary<string, double>();
            if (string.IsNullOrWhiteSpace(rule)) return factors;
            if (rule.Contains("#")) rule = rule.Substring(0, rule.IndexOf("#"));
            string[] parts = rule.Split('+');
            foreach (string part in parts)
            {
                string p = part.Trim(); if (string.IsNullOrEmpty(p)) continue;
                double factor = 1.0; string name = p;
                if (p.Contains("*"))
                {
                    string[] factorName = p.Split('*');
                    if (double.TryParse(factorName[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double val)) factor = val;
                    if (factorName.Length > 1) name = factorName[1].Trim();
                }
                factors[name] = factor;
            }
            return factors;
        }

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
                        if (DistanceToCurve(j.TargetLine, pt) < 100.0)
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

        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.StressSolver_icon;
        public override Guid ComponentGuid => new Guid("BC4D12A3-9A8B-7C6D-5E4F-3A2B1C0D9E8F");
    }
}