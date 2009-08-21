open Common
open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open QutSensors.AudioAnalysis.AED.LargeEvents
open Xunit
    
[<Fact>]
let testLastMin () =
    Assert.Equal(1, lastMin [0..2] [2;1;2])
    Assert.Equal(2, lastMin [0..3] [1;2;1;1])
    Assert.Equal(0, lastMin [0..2] [1;1;2])
    Assert.Equal(3, lastMin [0..4] [2;1;1;0;1])
    Assert.Equal(1, lastMin [0..3] [2;1;2;1])
    
[<Fact>]
let testThreshold () =
    let MATLAB_LENGTH = 1229 // TODO also defined in GetAcousticEventsTest
    let aem = loadEventsFile "AE.txt" MATLAB_LENGTH    
    Assert.Equal(9000, threshold aem)
    
[<Fact>]
let testAeToMatrix () =
    let ae = {Bounds={Left=1;Top=0;Width=5;Height=2}; Elements=Set.of_list [(0,1); (1,3)]}
    let m = Math.Matrix.of_list [[1.0; 0.0; 0.0; 0.0; 0.0]; [0.0; 0.0; 1.0; 0.0; 0.0]]
    Assert.Equal(m, aeToMatrix ae)