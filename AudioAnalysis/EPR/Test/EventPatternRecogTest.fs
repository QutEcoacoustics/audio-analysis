open QutSensors.AudioAnalysis.AED.Util
open QutSensors.AudioAnalysis.EPR.EventPatternRecog
open Xunit


// TODO copied and modified from AED test - generalise
let loadTestFile2 f i j = fileToMatrix (@"matlab\" + f) i j |> Math.Matrix.of_array2

// TODO copied and modified from AED test - generalise
// Format: time start, time duration, freq start, freq end
let loadEventsFile f m n =
    let aem = loadTestFile2 f m n
    seq {for i in 0..(m-1) -> {Left=aem.[i,0]; Right=aem.[i,0]+aem.[i,1]; Bottom=aem.[i,2]; Top=aem.[i,3]}}

[<Fact>]
let detectGroundParrotsTest () = 
    let t = loadEventsFile "GroundParrotTemplate.txt" 11 4
    let ae = loadEventsFile "GroundParrotAE.txt" 811 10
    let r = loadEventsFile "GroundParrotResults.txt" 10 4
    Assert.Equal(Seq.sort r, detectGroundParrots t ae |> Seq.sort)