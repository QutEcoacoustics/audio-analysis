open Common
open QutSensors.AudioAnalysis.AED.SubbandMode
open Xunit

[<Fact>]
let testRowHist () =
    let m = Math.Matrix.of_list [[ 1.0; 1.4; 2.2; 2.5; 2.51; 3.1; 3.7 ]]
    let vIn = Math.Vector.Generic.of_list [(1.0, 3.7)]
    let vOut = Math.Vector.Generic.of_list [[|2; 2; 3|]]
    Assert.Equal(rowHist m vIn, vOut)
    
[<Fact>]
let testRemoveSubbandModeIntensities2 () =
    let f d i j =
        let i3 = loadTestFile3 d "I2.txt" i j |> removeSubbandModeIntensities2
        let i3m = loadTestFile3 d "I3.txt" i j
        matrixFloatEquals i3 i3m 0.001 |> Assert.True
    test f