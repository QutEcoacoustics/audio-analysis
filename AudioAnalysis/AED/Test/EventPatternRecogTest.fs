module EventPatternRecogTest

open Common
open QutSensors.AudioAnalysis.AED.EventPatternRecog
open QutSensors.AudioAnalysis.AED.Util
open Xunit


// TODO copied and modified from AED test - generalise
// Format: time start, time duration, freq start, freq end
let loadEventsFile f md =
    let aem = loadTestFile2 md.Dir f 
    seq {for i in 0..(aem.NumCols-1) -> cornersToRect aem.[0,i] (aem.[0,i]+aem.[1,i]) aem.[3,i] aem.[2,i]}

[<Fact>]
let detectGroundParrotsTest () = 
    let md = GParrots_JB2_20090607_173000_wav_minute_3
    let ae = loadEventsFile "EPRAE.csv" md
    let m = loadEventsFile "EPRresults.csv" md |> Seq.sort
    //Assert.Equal(m, detectGroundParrots ae |> Seq.sort)
    let r = detectGroundParrots ae |> Seq.sort
    
    let toString r = sprintf "%f, %f, %f, %f" (left r) (right r) (bottom r) (top r)
    let s = sprintf "\r\nmatlab %i, F# %i\r\n" (Seq.length m) (Seq.length r)
    let f (m,r) = if m = r then "match" else sprintf "\r\nmatlab:\t %s \r\nF#:\t %s" (toString m) (toString r)
    let l = sprintf "\r\n\r\nF# (9):\t %s\r\n" (toString (Seq.nth 8 r))
    
    Assert.True(false, s + (Seq.zip m r |> Seq.map f |> String.concat "\r\n") + l)