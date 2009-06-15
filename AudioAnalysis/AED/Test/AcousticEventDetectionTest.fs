#light

open QutSensors.AudioAnalysis.AED.AcousticEventDetection
open Common
open Math.Matrix
open Util.Array2
open Xunit

[<Fact>]
let testRemoveSubbandModeIntensities2 () =
    let i3m = loadTestFile "I3.txt"
    let i4 = toBlackAndWhite 9.0 (of_array2 i3m) |> to_array2
    let i4m = loadTestFile "I4.txt"
    a2FloatEquals i4 i4m 0.001 |> Assert.True