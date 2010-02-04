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
    
let rectToString r = sprintf "%f, %f, %f, %f" (left r) (right r) (bottom r) (top r)

let assertSeqEqual f xs ys =
    let l = if Seq.length xs = Seq.length ys then None else sprintf "Lengths differ %i vs %i" (Seq.length xs) (Seq.length ys)|> Some
    let bs = Seq.map2 (=) xs ys
    let c = if Seq.forall id bs then [None]
            else let i = Seq.findIndex not bs
                 let i' = i + 1
                 [ sprintf "First difference at position %i" i |> Some;
                   sprintf "Expected[%i]:\t%s\r\nFound[%i]:\t%s" i (Seq.nth i xs |> f) i (Seq.nth i ys |> f) |> Some;
                   (if i' < Seq.length ys then sprintf "Found[%i]:\t%s" i' (Seq.nth i' ys |> f) |> Some else None) ]
    let m = catOptions (l::c)
    if Seq.isEmpty m then Assert.True(true) else Assert.True(false, "\r\n\r\n" + (String.concat "\r\n\r\n" m) + "\r\n")
        
[<Fact>]
let testCandidates () =
    let md = GParrots_JB2_20090607_173000_wav_minute_3
    let aes = loadFloatEventsFile "EPRAE.csv" md
    let f = @"matlab\" + md.Dir + @"\" + "EPRCandidates.csv"
    let ls = System.IO.File.ReadAllLines f
    
    let g x = String.split [' '] x |> Seq.map (fun s -> let n = System.Convert.ToInt32 s - 1 in Seq.nth n aes)    
    let mcs = Seq.map (fun l -> String.split [','] l |> Seq.nth 5 |> g) ls
    let msaes = Seq.map (Seq.nth 0 << Seq.sort) mcs
    
    let (_, tb, ttd, tfr) = templateBounds groundParrotTemplate
    let (saes, cs) = candidates tb ttd tfr aes
    Assert.Equal(Seq.length msaes, Seq.length saes)
    assertSeqEqual rectToString (Seq.sort msaes) (Seq.sort saes)

[<Fact>]
let detectGroundParrotsTest () = 
    let md = GParrots_JB2_20090607_173000_wav_minute_3
    let ae = loadFloatEventsFile "EPRAE.csv" md
    let m = loadFloatEventsFile "EPRresults.csv" md |> Seq.sort
    assertSeqEqual rectToString m (detectGroundParrots ae |> Seq.sort)