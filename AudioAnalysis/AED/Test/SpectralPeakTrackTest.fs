module SpectralPeakTrackTest

open Common
open QutSensors.AudioAnalysis.AED.SpectralPeakTrack
open Xunit

[<Fact>]
let testVerticalPeaks () = 
    let i3c = loadTestFile2 "SPT" "I3.csv" |> verticalPeaks 9.0
    let i3cm = loadTestFile2 "SPT" "I3c.csv" 
    Assert.True (matrixFloatEquals i3cm i3c 0.0001)