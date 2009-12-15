module AcousticEventDetectionTest

open QutSensors.AudioAnalysis.AED.AcousticEventDetection
open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open Common
open FsCheckArbs
open Xunit

[<Fact>]
let testRemoveSubbandModeIntensities () =
    let f md =
        let i3 = loadTestFile "I2.csv" md |> removeSubbandModeIntensities
        let i3m = loadTestFile "I3.csv" md
        matrixFloatEquals i3 i3m 0.0001 |> Assert.True
    testAll f
    
[<Fact>]
let testToBlackAndWhite () =
    let f md =
        let i4 = loadTestFile "I3.csv" md |> toBlackAndWhite md.BWthresh
        let i4m = loadTestFile "I4.csv" md
        matrixFloatEquals i4 i4m 0.001 |> Assert.True
    testAll f
    
[<Fact>]
let testToFillIn () =
    let m = Array2D.create 5 10 0.0 |> Math.Matrix.ofArray2D
    Assert.False(toFillIn m 0 0 3)
    Assert.False(toFillIn m 1 9 3)
    m.[0,1] <- 1.0
    Assert.False(toFillIn m 0 0 3)
    Assert.False(toFillIn m 0 2 3)
    m.[0,3] <- 1.0
    Assert.True(toFillIn m 0 2 3)

(* TODO Investigate using FsCheck instead of xUnit for testJoinHorizontalLinesQuick
    forall m. forall i in m.NumRows. forall j in m.NumCols. m.[i,j] = 1 => (joinHorizontalLines m).[i,j] = 1
    m.[i,j] = 0 => [(m.[i,j] in gap => (joinHorizontalLines m).[i,j] = 1] xor (joinHorizontalLines m).[i,j] = 0
*)

[<Fact>]
let testJoinHorizontalLinesQuick () =
    let m = Math.Matrix.zero 5 10 
    Assert.Equal(m, joinHorizontalLines m)
    m.[0,1] <- 1.0
    Assert.Equal(m, joinHorizontalLines m)
    m.[0,2] <- 1.0
    Assert.Equal(m, joinHorizontalLines m)
    m.[0,4] <- 1.0
    let m' = Math.Matrix.copy m
    m'.[0,3] <- 1.0
    Assert.Equal(m', joinHorizontalLines m)
    
[<Fact>]
let testJoinHorizontalLines () =
    let f md =
        let i6b = loadTestFile "I5.csv" md |> joinHorizontalLines
        let i6bm = loadTestFile "I6.csv" md
        matrixFloatEquals i6b i6bm 0.001 |> Assert.True
    testAll f
    
[<Fact>]
let testJoinVerticalLines () =
    let f md =
        let i6a = loadTestFile "I4.csv" md |> joinVerticalLines
        let i6am = loadTestFile "I5.csv" md
        matrixFloatEquals i6a i6am 0.001 |> Assert.True
    testAll f

[<Fact>]
let aeToMatrixBounds () = chk (fun ae -> let m = aeToMatrix ae in m.NumRows = ae.Bounds.Height && m.NumCols = ae.Bounds.Width)
    
[<Fact>]
let aeToMatrixElements () =
    let f ae i j x = 
        let inSet = Set.contains (ae.Bounds.Top+i, ae.Bounds.Left+j) ae.Elements
        if x = 1.0 then inSet else not inSet
    chk (fun ae -> aeToMatrix ae |> Math.Matrix.foralli (f ae))
    
[<Fact>]
let testSeparateLargeEvents () =
    let f md =
        let ae2 = loadTestFile "I6.csv" md |> getAcousticEvents |> separateLargeEvents
        let ae2m = loadEventsFile "AE2.csv" md
        Assert.Equal(Seq.length ae2m, Seq.length ae2)
        Assert.Equal(Seq.sort ae2m, Seq.sort ae2)
    testAll f        
    
[<Fact>]
let testSmallFirstMin () =
    let t = 42
    Assert.Equal(0, smallFirstMin [0..3] [1;2;1;2] t)
    Assert.Equal(0, smallFirstMin [0..2] [1;1;1] t)
    Assert.Equal(t, smallFirstMin [0..1] [2;1] t) 

[<Fact>]
let testSmallThreshold () =
    let f md =
        let ae2m = loadEventsFile "AE2.csv" md
        Assert.Equal(md.smallThreshOut, smallThreshold md.smallThreshIn ae2m)
    testAll f

[<Fact>]
let testFilterOutSmallEvents () =
    let f md =
        let ae2m = loadEventsFile "AE2.csv" md
        let ae3 = filterOutSmallEvents md.smallThreshIn ae2m
        let ae3m = loadEventsFile "AE3.csv" md
        Assert.Equal(Seq.sort ae3m, Seq.sort ae3)
    testAll f