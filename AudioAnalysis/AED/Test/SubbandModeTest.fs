module SubbandModeTest

open Common
open QutSensors.AudioAnalysis.AED.SubbandMode
open Xunit
    
[<Fact>]
let testRemoveSubbandModeIntensities2 () =
    let f md =
        let i3 = loadTestFile "I2.csv" md |> removeSubbandModeIntensities2
        let i3m = loadTestFile "I3.csv" md
        matrixFloatEquals i3 i3m 0.0001 |> Assert.True
    testAll f