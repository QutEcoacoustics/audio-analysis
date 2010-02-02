module EventPatternRecogTest

open Common
open QutSensors.AudioAnalysis.AED.EventPatternRecog
open QutSensors.AudioAnalysis.AED.Util
open Xunit

[<Fact>]
let testTemplateBounds () =
    let (tl, tb, ttd, tfr) = templateBounds groundParrotTemplate
    Assert.Equal(5.166440, tl)
    Assert.Equal(3531.445313, tb)
    Assert.Equal(8.962902-5.166440, ttd)
    Assert.Equal(4995.703125-3531.445313, tfr)
    
let testCandidates () =
    let md = GParrots_JB2_20090607_173000_wav_minute_3
    let aes = loadFloatEventsFile "EPRAE.csv" md
    let f = @"matlab\" + md.Dir + @"\" + "EPRCandidates.csv"
    let ls = System.IO.File.ReadAllLines f
    let g (l:string []) = Seq.find (fun ae -> floatEquals (left ae) (System.Convert.ToDouble l.[0]) 0.00001) aes
    let saes = Seq.map (fun l -> String.split [','] l |> List.toArray |> g) ls
    ()

[<Fact>]
let detectGroundParrotsTest () = 
    let md = GParrots_JB2_20090607_173000_wav_minute_3
    let ae = loadFloatEventsFile "EPRAE.csv" md
    let m = loadFloatEventsFile "EPRresults.csv" md |> Seq.sort
    //Assert.Equal(m, detectGroundParrots ae |> Seq.sort)
    let r = detectGroundParrots ae |> Seq.sort
    
    let toString r = sprintf "%f, %f, %f, %f" (left r) (right r) (bottom r) (top r)
    let s = sprintf "\r\nmatlab %i, F# %i\r\n" (Seq.length m) (Seq.length r)
    let f (m,r) = if m = r then "match" else sprintf "\r\nmatlab:\t %s \r\nF#:\t %s" (toString m) (toString r)
    let l = sprintf "\r\n\r\nF# (8):\t %s\r\n" (toString (Seq.nth 7 r))
    Assert.True(false, s + (Seq.zip m r |> Seq.map f |> String.concat "\r\n") + l)