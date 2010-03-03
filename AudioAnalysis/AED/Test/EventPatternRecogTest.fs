module EventPatternRecogTest

open Common
open QutSensors.AudioAnalysis.AED.EventPatternRecog
open QutSensors.AudioAnalysis.AED.Util
open Xunit

[<Fact>]
let testTemplateBounds () =
    let (tl, tb, ttd, tfr) = templateBounds groundParrotTemplate
    Assert.Equal(5.166439909297052, tl)
    Assert.Equal(3531.4453125, tb)
    Assert.Equal(8.962902494331066 - 5.166439909297052, ttd)
    Assert.Equal(4995.703125 - 3531.4453125, tfr)
        
let fromCsv =
    let md = GParrots_JB2_20090607_173000_wav_minute_3
    let aes = loadFloatEventsFile "EPRAE.csv" md
    let f = @"matlab\" + md.Dir + @"\" + "EPRCandidates.csv"
    let ls = System.IO.File.ReadAllLines f |> List.ofArray
    
    let g x = split [|' '|] x |> Seq.map (fun s -> let n = System.Convert.ToInt32 s - 1 in Seq.nth n aes)    
    let (mcs, scores) = List.map (fun l -> let es = split [|','|] l in (Seq.nth 5 es |> g, Seq.nth 6 es |> System.Convert.ToDouble)) ls |> List.unzip
    let msaes = Seq.map (Seq.nth 0 << Seq.sort) mcs
    (aes, msaes, mcs, scores)
        
[<Fact>]
let testCandidates () =
    let (aes, msaes, mcs, scores)= fromCsv
    let (_, tb, ttd, tfr) = templateBounds groundParrotTemplate
    let (saes, cs) = candidates tb ttd tfr aes
    assertSeqEqual (=) rectToString msaes saes
    // check each sequence of candidates
    Seq.zip mcs cs |> Seq.mapi (fun i (mc, c) -> (i, seqEqual (=) rectToString mc c))
        |> Seq.tryFind (fun (i, ss) -> not (Seq.isEmpty ss))
        |> Option.iter (fun (i, ss) -> Assert.True(false,
                                           sprintf "First difference in outside sequence at position %i %s" i "\r\n\r\n" +
                                           (String.concat "\r\n\r\n" ss) + "\r\n"))

[<Fact>]
let testPixelAxisLengths () =
    let (_, _, ttd, tfr) = templateBounds groundParrotTemplate
    let (xl, yl) = pixelAxisLengths ttd tfr
    Assert.Equal(328.0, xl)
    Assert.Equal(35.0, yl)

[<Fact>]
let testTemplateCentroidsBottomLefts () =
    let md = GParrots_JB2_20090607_173000_wav_minute_3
    let mToTuples = mapByRow (fun v -> (v.[0], v.[1]))
    let mtcs = loadTestFile "EPRtemplatecentroids.csv" md |> mToTuples
    let mtbls = loadTestFile "EPRtemplatebottomlefts.csv" md |> mToTuples
    
    let (tl, tb, ttd, tfr) = templateBounds groundParrotTemplate
    let (xl, yl) = pixelAxisLengths ttd tfr
    let (tcs, tbls) = centroidsBottomLefts tl tb ttd tfr xl yl groundParrotTemplate
    assertSeqEqual (=) defToString mtcs tcs
    assertSeqEqual (=) defToString mtbls tbls

[<Fact>]
let testScores () =
    let (aes, msaes, mcs, mscores)= fromCsv
    let scores = scoreGroundParrots aes
    let eq (r1, s1) (r2, s2) = r1 = r2 && floatEquals s1 s2 0.001
    assertSeqEqual eq defToString (Seq.zip msaes mscores) scores 

[<Fact>]
let testDetectGroundParrots () = 
    let md = GParrots_JB2_20090607_173000_wav_minute_3
    let ae = loadFloatEventsFile "EPRAE.csv" md
    let m = loadFloatEventsFile "EPRresults.csv" md
    assertSeqEqual (=) rectToString m (detectGroundParrots ae)