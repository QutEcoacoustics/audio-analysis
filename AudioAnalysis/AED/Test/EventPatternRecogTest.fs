module EventPatternRecogTest

open Common
open Microsoft.FSharp.Math.SI
open Xunit

open QutSensors.AudioAnalysis.AED.EventPatternRecog
open QutSensors.AudioAnalysis.AED.EventPatternRecog.EprInternals
open QutSensors.AudioAnalysis.AED.Util

let convert s = Seq.map (fun r -> addDimensions 1.0<s> 1.0<Hz> r) s
let rem r = removeDimensions r 1.0<s> 1.0<Hz>
let unconvert s = Seq.map (fun r -> removeDimensions r 1.0<s> 1.0<Hz>) s
let groundParrotTemplate = convert groundParrotTemplate

[<Fact>]
let testTemplateBounds () =
    let (tl, tb, ttd, tfr) = templateBounds groundParrotTemplate
    Assert.Equal(13.374694<s>, tl)
    Assert.Equal(3617.578125<Hz>, tb)
    Assert.Equal(17.821315<s> - 13.374694<s>, ttd)
    Assert.Equal(4823.437500<Hz> - 3617.578125<Hz>, tfr)
        
let fromCsv =
    let md = GParrots_JB2_20090607_173000_wav_minute_3
    let aes = loadFloatEventsFile "EPRAE.csv" md
    let f = Common.matlabPath + md.Dir + @"\" + "EPRCandidates.csv"
    let ls = System.IO.File.ReadAllLines f |> List.ofArray
    
    let g x = split [|' '|] x |> Seq.map (fun s -> let n = System.Convert.ToInt32 s - 1 in Seq.nth n aes)    
    let (mcs, scores) = List.map (fun l -> let es = split [|','|] l in (Seq.nth 5 es |> g, Seq.nth 6 es |> System.Convert.ToDouble)) ls |> List.unzip
    let msaes = Seq.map (Seq.nth 0 << Seq.sort) mcs
    (aes, msaes, mcs, scores)
        
[<Fact>]
let testCandidates () =
    let (aes, msaes, mcs, scores)= fromCsv
    let (_, tb, ttd, tfr) = templateBounds groundParrotTemplate
    let (saes, cs) = candidates tb ttd tfr (convert aes)
    assertSeqEqual (=) rectToString msaes (unconvert saes)
    // check each sequence of candidates
    Seq.zip mcs cs |> Seq.mapi (fun i (mc, c) -> (i, seqEqual (=) rectToString mc (unconvert c)))
        |> Seq.tryFind (fun (i, ss) -> not (Seq.isEmpty ss))
        |> Option.iter (fun (i, ss) -> Assert.True(false,
                                           sprintf "First difference in outside sequence at position %i %s" i "\r\n\r\n" +
                                           (String.concat "\r\n\r\n" ss) + "\r\n"))

[<Fact>]
let testPixelAxisLengths () =
    let (_, _, ttd, tfr) = templateBounds groundParrotTemplate
    let (xl, yl) = pixelAxisLengths ttd tfr
    Assert.Equal(384.0<px>, xl)
    Assert.Equal(29.0<px>, yl)

[<Fact>]
let testTemplateCentroidsBottomLefts () =
    let md = GParrots_JB2_20090607_173000_wav_minute_3
    let mToTuples = mapByRow (fun v -> (v.[0] * 1.0<px>, v.[1] * 1.0<px>))
    let mtcs = loadTestFile "EPRtemplatecentroids.csv" md |> mToTuples
    let mtbls = loadTestFile "EPRtemplatebottomlefts.csv" md |> mToTuples
    
    let (tl, tb, ttd, tfr) = templateBounds groundParrotTemplate
    let (xl, yl) = pixelAxisLengths ttd tfr
    
    System.Diagnostics.Trace.WriteLine (sprintf "INFO: pixel axis lengths: %f, %f" (float xl) (float yl))
    let (tcs, tbls) = centroidsBottomLefts tl tb ttd tfr xl yl groundParrotTemplate
    //let r = Seq.map2 (fun (i:pxf*pxf) (j:pxf*pxf) -> sprintf "%f,%f,%f,%f" (float (fst i)) (float (snd i)) (float (fst j)) (float (snd j))) tcs tbls
    assertSeqEqual (=) defToString mtbls tbls
    assertSeqEqual (=) defToString mtcs tcs
    

[<Fact>]
let testScores () =
    let (aes, msaes, mcs, mscores)= fromCsv |> (fun (a, b, c, d) -> (convert a, convert b, List.map convert c, d))
    let scores = scoreEvents groundParrotTemplate aes
    let eq (r1, s1) (r2, s2) = r1 = r2 && floatEquals s1 s2 0.3
    assertSeqEqual eq defToString (Seq.zip msaes mscores) scores 

[<Fact>]
let testDetectGroundParrots () = 
    let md = GParrots_JB2_20090607_173000_wav_minute_3
    let ae = loadFloatEventsFile "EPRAE.csv" md 
    let m = loadFloatEventsFile "EPRresults.csv" md 
    ////let r = Seq.map (fun (r:Rectangle<_,_>)-> (left(r)).ToString() + "," + (width(r)).ToString() + "," + (bottom(r)).ToString() + "," + (top(r)).ToString()) (unconvert (Seq.map fst (DetectGroundParrots ae)))
    ////Assert.True(false, (String.concat "\r\n" r))
    assertSeqEqual (=) rectToString m (unconvert (Seq.map fst (DetectGroundParrots ae QutSensors.AudioAnalysis.AED.Default.eprNormalisedMinScore)))
    