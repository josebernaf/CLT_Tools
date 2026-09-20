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
    public class ForcesSolverComp : GH_Component
    {
        public ForcesSolverComp()
          : base("Internal Forces Solver", "Forces",
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
            pManager.AddVectorParameter("Membrane Forces (N)", "VecN", "Forces [kN/m] (F11, F22, F12)", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Bending Moments (M)", "VecM", "Bending moments [kNm/m] (M11, M22, M12)", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Shear Forces (V)", "VecV", "Shear forces [kN/m] (V13, V23, VMax)", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Principal Membrane", "PrinN", "Principal forces (FMax, FMin, FAngle)", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Principal Bending", "PrinM", "Principal moments (MMax, MMin, MAngle)", GH_ParamAccess.tree);
            pManager.AddTextParameter("Log", "Log", "Informe del solver.", GH_ParamAccess.item);
        }

        private double DistanceToCurve(Curve crv, Point3d pt)
        {
            if (crv == null) return double.MaxValue;
            if (crv.IsLinear())
            {
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

            try { foreach (var proc in Process.GetProcessesByName("OpenSees")) proc.Kill(); } catch { }
            string tempFolder = Path.Combine(Path.GetTempPath(), "CLT_Solver");
            if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
            foreach (string f in Directory.GetFiles(tempFolder, "res_force_*.txt")) { try { File.Delete(f); } catch { } }

            GH_ObjectWrapper seismicWrap = null;
            object sDataObj = null;

            DA.GetData(2, ref seismicWrap);
            if (seismicWrap != null)
            {
                object raw = seismicWrap.Value;
                if (raw is GH_ObjectWrapper wrapper) sDataObj = wrapper.Value;
                else if (raw is IGH_Goo goo) sDataObj = goo.ScriptVariable();
                else sDataObj = raw;
            }

            GH_ObjectWrapper modelWrap = null;
            List<string> combRules = new List<string>();
            bool writeCSV = false;

            if (!DA.GetData(0, ref modelWrap)) return;
            if (!DA.GetDataList(1, combRules)) combRules.Add("1.0*PP");
            DA.GetData(3, ref writeCSV);

            CLTModel model = modelWrap.Value as CLTModel;
            if (model == null) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Modelo nulo."); return; }

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
            if (!File.Exists(exePath)) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "OpenSees no encontrado."); return; }

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
                    double weight_N = (area * thickness_mm * 1e-9) * owner.Material.Rho * 9.81;
                    Vector3d fNode = new Vector3d(0, 0, -weight_N / 4.0);
                    selfWeightForces[n1] += fNode; selfWeightForces[n2] += fNode; selfWeightForces[n3] += fNode; selfWeightForces[n4] += fNode;
                }
            }

            List<int> fullyFixedNodes = new List<int>();
            var culture = CultureInfo.InvariantCulture;
            StringBuilder tcl = new StringBuilder();

            int totalSpringsGenerated = 0;
            double sumKser = 0;

            for (int c = 0; c < combRules.Count; c++)
            {
                string rule = combRules[c];
                tcl.AppendLine($"# --- COMBINATION {c}: {rule} ---");
                tcl.AppendLine("wipe;");
                tcl.AppendLine("model BasicBuilder -ndm 3 -ndf 6;");

                for (int i = 0; i < elements.Count; i++)
                {
                    CLTElement el = elements[i];
                    CLTMaterial m = el.Material;
                    var props = el.Properties;
                    int id = i + 1;
                    tcl.AppendLine($"nDMaterial ElasticOrthotropic {10000 + id} {m.E0} {m.E90} {m.E90} {m.Nu12} 0 0 {m.Gxy} {m.Gyz} {m.Gxz} {m.Rho};");
                    double nu21 = m.Nu12 * (m.E90 / m.E0);
                    tcl.AppendLine($"nDMaterial ElasticOrthotropic {20000 + id} {m.E90} {m.E0} {m.E90} {nu21} 0 0 {m.Gxy} {m.Gxz} {m.Gyz} {m.Rho};");

                    tcl.Append($"section LayeredShell {1000 + id} {props.Thicknesses.Count}");
                    for (int k = 0; k < props.Thicknesses.Count; k++) tcl.Append($" {(Math.Abs(props.Angles[k]) < 1.0 ? 10000 + id : 20000 + id)} {props.Thicknesses[k]}");
                    tcl.AppendLine(";");

                    tcl.Append($"section LayeredShell {2000 + id} {props.Thicknesses.Count}");
                    for (int k = 0; k < props.Thicknesses.Count; k++) tcl.Append($" {(Math.Abs(props.Angles[k]) < 1.0 ? 20000 + id : 10000 + id)} {props.Thicknesses[k]}");
                    tcl.AppendLine(";");
                }

                for (int i = 0; i < numNodes; i++)
                {
                    Point3d p = pts[i];
                    tcl.AppendLine(string.Format(culture, "node {0} {1:F4} {2:F4} {3:F4};", i + 1, p.X, p.Y, p.Z));
                }

                fullyFixedNodes.Clear();

                if (supports.Count > 0)
                {
                    for (int i = 0; i < numNodes; i++)
                    {
                        foreach (var sup in supports)
                        {
                            if (pts[i].DistanceTo(sup.Location) < 20.0)
                            {
                                tcl.AppendLine($"fix {i + 1} {sup.FixString};");
                                if (sup.FixString.Replace(" ", "") == "111111") fullyFixedNodes.Add(i);
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
                            tcl.AppendLine($"fix {i + 1} 1 1 1 1 1 1;");
                            fullyFixedNodes.Add(i);
                        }
                    }
                }

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
                                if (DistanceToCurve(joint.TargetLine, pts[i]) < 2.0)
                                {
                                    activeJoint = joint; break;
                                }
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
                                tcl.AppendLine(string.Format(culture, "uniaxialMaterial Elastic {0} 10.0;", rotMatID));

                                tcl.AppendLine($"element zeroLength {springEleId++} {i + 1} {j + 1} -mat {matID} {matID} {matID} {rotMatID} {rotMatID} {rotMatID} -dir 1 2 3 4 5 6;");
                                totalSpringsGenerated++;
                            }
                        }
                    }
                }
                tcl.AppendLine("# ------------------------------------\n");

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
                        int sectionID = (dot > 0.707) ? (1000 + id) : (2000 + id);
                        tcl.AppendLine($"element ShellMITC4 {i + 1} {n1 + 1} {n2 + 1} {n3 + 1} {n4 + 1} {sectionID};");
                    }
                }

                Dictionary<string, double> loadFactors = ParseLoadCombination(rule);
                tcl.AppendLine("timeSeries Linear 1; pattern Plain 1 1 {");

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
                        if (l.TargetElementID != null && l.TargetElementID.Trim().Equals("<empty>", StringComparison.OrdinalIgnoreCase))
                        {
                            l.TargetElementID = "IGNORE_THIS_LOAD";
                        }
                        if (!string.IsNullOrWhiteSpace(l.TargetElementID) && l.TargetElementID != "IGNORE_THIS_LOAD")
                        {
                            string sig = $"{l.LoadCaseName}_{l.Vector.X:F4}_{l.Vector.Y:F4}_{l.Vector.Z:F4}";
                            specificTargetSignatures.Add(sig);
                        }
                        else if (l.TargetMesh != null)
                        {
                            string sig = $"{l.LoadCaseName}_{l.Vector.X:F4}_{l.Vector.Y:F4}_{l.Vector.Z:F4}";
                            specificTargetSignatures.Add(sig);
                        }
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
                        string sig = $"{load.LoadCaseName}_{load.Vector.X:F4}_{load.Vector.Y:F4}_{load.Vector.Z:F4}";

                        if (isGlobal && specificTargetSignatures.Contains(sig)) continue;

                        Vector3d pressure_MPa = (load.Vector * factor);

                        bool hasTextFilter = !string.IsNullOrWhiteSpace(load.TargetElementID);
                        HashSet<int> validNodes = new HashSet<int>();

                        if (hasTextFilter)
                        {
                            HashSet<string> targetIDs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            string[] splits = load.TargetElementID.Split(new char[] { '\n', '\r', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in splits)
                            {
                                string trimmed = s.Trim();
                                if (!string.IsNullOrEmpty(trimmed)) targetIDs.Add(trimmed);
                            }

                            for (int fIdx = 0; fIdx < globalMesh.Faces.Count; fIdx++)
                            {
                                int ownerIdx = globalFaceOwners[fIdx];
                                CLTElement ownerEl = elements[ownerIdx];
                                string panelID = ownerEl.Properties != null ? ownerEl.Properties.ID : $"Elem_{ownerIdx}";

                                if (targetIDs.Contains(panelID))
                                {
                                    MeshFace face = globalMesh.Faces[fIdx];
                                    validNodes.Add(face.A);
                                    validNodes.Add(face.B);
                                    validNodes.Add(face.C);
                                    if (face.IsQuad) validNodes.Add(face.D);
                                }
                            }
                        }

                        for (int i = 0; i < numNodes; i++)
                        {
                            if (fullyFixedNodes.Contains(i)) continue;

                            bool apply = true;
                            if (load.TargetMesh != null)
                            {
                                Point3d pt = pts[i];
                                Point3d closePt = load.TargetMesh.ClosestPoint(pt);
                                if (pt.DistanceTo(closePt) > 5.0) apply = false;
                            }
                            else if (hasTextFilter)
                            {
                                if (!validNodes.Contains(i)) apply = false;
                            }
                            else if (isGlobal) apply = true;
                            else apply = false;

                            if (apply)
                            {
                                Vector3d f = pressure_MPa * nodeAreas[i];
                                if (f.Length > 1e-12) tcl.AppendLine(string.Format(culture, "load {0} {1:F4} {2:F4} {3:F4} 0 0 0;", i + 1, f.X, f.Y, f.Z));
                            }
                        }
                    }
                }
                bool isSeismic = (rule.IndexOf("Seismic", StringComparison.OrdinalIgnoreCase) >= 0 || rule.IndexOf("Sismo", StringComparison.OrdinalIgnoreCase) >= 0);
                if (isSeismic && sDataObj != null)
                {
                    double dirX = 0.0, dirY = 0.0;
                    if (rule.IndexOf("Y", StringComparison.OrdinalIgnoreCase) >= 0) dirY = 1.0; else dirX = 1.0;

                    double ab = GetDoubleVal(sDataObj, "ab");
                    double K = GetDoubleVal(sDataObj, "K", 1.0);
                    double C = GetDoubleVal(sDataObj, "C", 1.0);
                    double Rho = GetDoubleVal(sDataObj, "Rho", 1.0);

                    if (ab > 0)
                    {
                        double accel_ms2 = (ab * K * C * Rho) * 9.81;
                        for (int i = 0; i < numNodes; i++)
                        {
                            double mass = Math.Abs(selfWeightForces[i].Z) / 9.81;
                            double F = mass * accel_ms2;
                            if (F > 1e-6)
                            {
                                tcl.AppendLine(string.Format(culture, "load {0} {1:F4} {2:F4} 0 0 0 0;", i + 1, F * dirX, F * dirY));
                            }
                        }
                    }
                }
                tcl.AppendLine("}");

                tcl.AppendLine("system UmfPack; numberer RCM; constraints Plain; integrator LoadControl 1.0;");
                tcl.AppendLine("algorithm Newton; test NormDispIncr 1.0e-6 50 0;");
                tcl.AppendLine("analysis Static; analyze 1;");

                string resFileTcl = $"res_force_{c}.txt";
                tcl.AppendLine($"set f [open \"{resFileTcl}\" \"w\"];");
                tcl.AppendLine("foreach e [getEleTags] { puts $f \"$e [eleResponse $e section 1 force]\"; }");
                tcl.AppendLine("close $f;");
            }

            tcl.AppendLine("exit;");

            string tclPath = Path.Combine(tempFolder, "run_forces.tcl");
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

            GH_Structure<GH_Vector> outN = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Vector> outM = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Vector> outV = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Vector> outPrinN = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Vector> outPrinM = new GH_Structure<GH_Vector>();

            string csvPath = Path.Combine(tempFolder, "Forces_Results.csv");
            StringBuilder csv = new StringBuilder();

            if (writeCSV)
            {
                csv.AppendLine("Element,OutputCase,CaseType,StepType,F11,F22,F12,FMax,FMin,FAngle,FVM,M11,M22,M12,MMax,MMin,MAngle,V13,V23,VMax,VAngle");
                csv.AppendLine("Text,Text,Text,Text,kN/m,kN/m,kN/m,kN/m,kN/m,Degrees,kN/m,kNm/m,kNm/m,kNm/m,kNm/m,kNm/m,Degrees,kN/m,kN/m,kN/m,Degrees");
            }

            for (int c = 0; c < combRules.Count; c++)
            {
                string resFile = Path.Combine(tempFolder, $"res_force_{c}.txt");
                GH_Path path = new GH_Path(c);
                string combName = combRules[c];

                if (File.Exists(resFile))
                {
                    string[] lines = File.ReadAllLines(resFile);

                    Vector3d[] arrN = new Vector3d[numElems];
                    Vector3d[] arrM = new Vector3d[numElems];
                    Vector3d[] arrV = new Vector3d[numElems];
                    Vector3d[] arrPrinN = new Vector3d[numElems];
                    Vector3d[] arrPrinM = new Vector3d[numElems];

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

                                double m11 = double.Parse(p[4], culture) / 1000;
                                double m22 = double.Parse(p[5], culture) / 1000;
                                double m12 = double.Parse(p[6], culture) / 1000;

                                double v13 = double.Parse(p[7], culture);
                                double v23 = double.Parse(p[8], culture);

                                double fAvg = (f11 + f22) / 2.0;
                                double rF = Math.Sqrt(Math.Pow((f11 - f22) / 2.0, 2) + Math.Pow(f12, 2));
                                double fMax = fAvg + rF;
                                double fMin = fAvg - rF;
                                double fAngle = 0.5 * Math.Atan2(2.0 * f12, f11 - f22) * (180.0 / Math.PI);
                                double fVM = Math.Sqrt(f11 * f11 + f22 * f22 - f11 * f22 + 3 * f12 * f12);

                                double mAvg = (m11 + m22) / 2.0;
                                double rM = Math.Sqrt(Math.Pow((m11 - m22) / 2.0, 2) + Math.Pow(m12, 2));
                                double mMax = mAvg + rM;
                                double mMin = mAvg - rM;
                                double mAngle = 0.5 * Math.Atan2(2.0 * m12, m11 - m22) * (180.0 / Math.PI);

                                double vMax = Math.Sqrt(v13 * v13 + v23 * v23);
                                double vAngle = Math.Atan2(v23, v13) * (180.0 / Math.PI);

                                arrN[idx] = new Vector3d(f11, f22, f12);
                                arrM[idx] = new Vector3d(m11, m22, m12);
                                arrV[idx] = new Vector3d(v13, v23, vMax);

                                arrPrinN[idx] = new Vector3d(fMax, fMin, fAngle);
                                arrPrinM[idx] = new Vector3d(mMax, mMin, mAngle);

                                if (writeCSV)
                                {
                                    string csvLine = string.Format(culture,
                                        "{0},{1},Combination,Static,{2:F4},{3:F4},{4:F4},{5:F4},{6:F4},{7:F4},{8:F4},{9:F4},{10:F4},{11:F4},{12:F4},{13:F4},{14:F4},{15:F4},{16:F4},{17:F4},{18:F4}",
                                        id, combName,
                                        f11, f22, f12, fMax, fMin, fAngle, fVM,
                                        m11, m22, m12, mMax, mMin, mAngle,
                                        v13, v23, vMax, vAngle);
                                    csv.AppendLine(csvLine);
                                }
                            }
                        }
                    }

                    foreach (var v in arrN) outN.Append(new GH_Vector(v), path);
                    foreach (var v in arrM) outM.Append(new GH_Vector(v), path);
                    foreach (var v in arrV) outV.Append(new GH_Vector(v), path);
                    foreach (var v in arrPrinN) outPrinN.Append(new GH_Vector(v), path);
                    foreach (var v in arrPrinM) outPrinM.Append(new GH_Vector(v), path);
                }
                else
                {
                    finalLog.AppendLine($"ERROR: No hay resultados para la combinación {c}.");
                }
            }

            if (writeCSV)
            {
                try
                {
                    File.WriteAllText(csvPath, csv.ToString());
                    finalLog.AppendLine($"CSV Export: Success -> {csvPath}");
                }
                catch (Exception ex)
                {
                    finalLog.AppendLine($"CSV Export ERROR: {ex.Message}");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No se pudo escribir el CSV. Ciérralo si está abierto en Excel.");
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
            DA.SetDataTree(1, outN);
            DA.SetDataTree(2, outM);
            DA.SetDataTree(3, outV);
            DA.SetDataTree(4, outPrinN);
            DA.SetDataTree(5, outPrinM);
            DA.SetData(6, report.ToString());
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
        private double GetDoubleVal(object obj, string name, double defVal = 0.0)
        {
            if (obj == null) return defVal;
            Type t = obj.GetType();
            foreach (var prop in t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
            {
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    try { return Convert.ToDouble(prop.GetValue(obj)); } catch { }
                }
            }
            foreach (var field in t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic))
            {
                if (string.Equals(field.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    try { return Convert.ToDouble(field.GetValue(obj)); } catch { }
                }
            }
            return defVal;
        }

        protected override System.Drawing.Bitmap Icon => CLT_Tools.Properties.Resources.ForcesSolver_icon;
        public override Guid ComponentGuid => new Guid("1a2b3c4d-5555-6666-7777-888899990000");
    }
}