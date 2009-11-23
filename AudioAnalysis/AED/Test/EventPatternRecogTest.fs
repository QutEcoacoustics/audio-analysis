module EventPatternRecogTest

open QutSensors.AudioAnalysis.AED.EventPatternRecog
open QutSensors.AudioAnalysis.AED.Util
open Xunit


// TODO copied and modified from AED test - generalise
let loadTestFile2 f i j = fileToMatrix (@"matlab\" + f) i j |> Math.Matrix.ofArray2D

// TODO copied and modified from AED test - generalise
// Format: time start, time duration, freq start, freq end
let loadEventsFile f m n =
    let aem = loadTestFile2 f m n
    seq {for i in 0..(m-1) -> {Left=aem.[i,0]; Right=aem.[i,0]+aem.[i,1]; Bottom=aem.[i,2]; Top=aem.[i,3]}}

[<Fact>]
let detectGroundParrotsTest () = 
    let ae = loadEventsFile "GroundParrotAE.txt" 811 10
    let m = loadEventsFile "GroundParrotResults.txt" 10 4 |> Seq.sort
    Assert.Equal(m, detectGroundParrots ae |> Seq.sort)
    //let r = detectGroundParrots t ae |> Seq.sort
    //Assert.True(false, sprintf "\nmatlab: %A\n\n F#: %A" (Seq.nth 9 m) (Seq.nth 9 r))