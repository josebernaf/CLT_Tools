wipe; model BasicBuilder -ndm 3 -ndf 6;
nDMaterial ElasticOrthotropic 18803 12000 460 460 0.335 0 0 690 50 690 357;
nDMaterial PlateFiber 23803 18803;
section LayeredShell 1000 3 23803 40 23803 40 23803 40;
node 1 0 0 0; node 2 1 0 0; node 3 1 1 0; node 4 0 1 0;
element ShellMITC4 1 1 2 3 4 1000;
fix 1 1 1 1 1 1 1; fix 2 1 1 1 1 1 1;
timeSeries Linear 1; pattern Plain 1 1 { load 3 0 0 10 0 0 0; load 4 0 0 10 0 0 0; }
system UmfPack; numberer RCM; constraints Plain; integrator LoadControl 1.0; algorithm Newton; analysis Static; analyze 1;
puts "--- STRESSES ---"
catch { puts [eleResponse 1 stresses] }
puts "--- MAT 1 STRESSES ---"
catch { puts [eleResponse 1 material 1 stresses] }
