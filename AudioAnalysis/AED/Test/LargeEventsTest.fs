open Common
open FsCheckArbs
open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open QutSensors.AudioAnalysis.AED.LargeEvents
open Xunit
    
[<Fact>]
let testLastMin () =
    Assert.Equal(1, lastMin [0..2] [2;1;2])
    Assert.Equal(2, lastMin [0..3] [1;2;1;1])
    Assert.Equal(0, lastMin [0..2] [1;1;2])
    Assert.Equal(3, lastMin [0..4] [2;1;1;0;1])
    Assert.Equal(1, lastMin [0..3] [2;1;2;1])
    
[<Fact>]
let testThreshold () =
    let f d l t =
        let aem = loadEventsFile d "AE.txt" l
        Assert.Equal(t, threshold aem)
    // TODO duplicated with GetAcousticEventsTest
    f "BAC2_20071015-045040" 1229 9000
    f "GParrots_JB2_20090607-173000.wav_minute_3" 5229 3000

[<Fact>]
let aeToMatrixBounds () = chk (fun ae -> let m = aeToMatrix ae in m.NumRows = ae.Bounds.Height && m.NumCols = ae.Bounds.Width)
    
[<Fact>]
let aeToMatrixElements () =
    let f ae i j x = 
        let inSet = Set.contains (ae.Bounds.Top+i, ae.Bounds.Left+j) ae.Elements
        if x = 1.0 then inSet else not inSet
    chk (fun ae -> aeToMatrix ae |> Math.Matrix.foralli (f ae))
    
[<Fact>]
let separateLargeEventsTest () =
    let f d i j ael =
        let ae2 = loadTestFile3 d "I6b.txt" i j |> getAcousticEvents |> separateLargeEvents
        let ae2m = loadEventsFile d "AE2.txt" ael
        Assert.Equal(Seq.length ae2m, Seq.length ae2)
        Assert.Equal(Seq.sort ae2m, Seq.sort ae2)
    // TODO duplicated with GetAcousticEventsTest
    f "BAC2_20071015-045040" 256 5188 1249
    f "GParrots_JB2_20090607-173000.wav_minute_3" 256 5166 5291