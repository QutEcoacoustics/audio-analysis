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
let testCentroidsBottomLefts () =
    let (aes, msaes, mcs, scores)= fromCsv
    let (_, _, ttd, tfr) = templateBounds groundParrotTemplate
    let (xl, yl) = pixelAxisLengths ttd tfr
    
    let f i cs' bls' =
        let rs = Seq.nth i mcs
        let (st, sf) = absLeftAbsBottom rs
        let (cs, bls) = centroidsBottomLefts st sf ttd tfr xl yl rs
        assertSeqEqual (=) defToString bls' bls
        assertSeqEqual (=) defToString cs' cs
        
    let bls0 = [(1.0,1.0);(60.0,3.0);(223.0,6.0);(228.0,20.0);(261.0,1.0);(305.0,26.0);(328.0,19.0)]
    let cs0 = [(8.0,24.0);(62.0,8.0);(226.0,13.0);(235.0,35.0);(264.0,3.0);(309.0,28.0);(328.0,23.0)]
    f 0 cs0 bls0
    
    //Assert.True(false, Seq.skip 8 bls |> defToString)
    
    let bls5 = [(1.0, 1.0); (44.0, 25.0); (67.0, 18.0); (92.0, 25.0);(106.0, 24.0); (162.0, 11.0); (186.0, 11.0); (208.0, 7.0); (236.0, 15.0); (256.0, 15.0); (277.0, 1.0); (285.0, 18.0);(294.0, 1.0); (306.0, 20.0); (323.0, 2.0)]
    let cs5 = [(4.0, 2.0); (48.0, 27.0); (69.0, 22.0); (98.0, 29.0);(109.0, 29.0); (169.0, 17.0); (191.0, 15.0); (216.0, 13.0);(240.0, 19.0); (265.0, 20.0); (282.0, 4.0); (289.0, 22.0);(297.0, 6.0); (315.0, 24.0); (326.0, 6.0)]
    f 5 cs5 bls5
    
    let bls24 = [(1.0, 1.0); (15.0, 28.0); (184.0, 13.0); (223.0, 18.0);(240.0, 1.0); (263.0, 19.0); (276.0, 21.0); (291.0, 26.0);(292.0, 12.0); (315.0, 28.0); (316.0, 11.0)]    
    let cs24 = [(3.0, 3.0); (23.0, 35.0); (196.0, 35.0); (227.0, 20.0);(243.0, 3.0); (268.0, 35.0); (281.0, 35.0); (298.0, 35.0);(297.0, 17.0); (316.0, 33.0); (322.0, 21.0)]
    f 24 cs24 bls24

//[<Fact>]
let testScores () =
    let (aes, msaes, mcs, mscores)= fromCsv
    let scores = detectGroundParrots' aes
    let eq (r1, s1) (r2, s2) = r1 = r2 && floatEquals s1 s2 0.001
    //assertSeqEqual eq defToString (Seq.zip msaes mscores) scores 
    Assert.True(false, Seq.nth 24 scores |> defToString) 
    //Assert.True(false, defToString scores) 

[<Fact>]
let detectGroundParrotsTest () = 
    let md = GParrots_JB2_20090607_173000_wav_minute_3
    let ae = loadFloatEventsFile "EPRAE.csv" md
    let m = loadFloatEventsFile "EPRresults.csv" md
    assertSeqEqual (=) rectToString m (detectGroundParrots ae)