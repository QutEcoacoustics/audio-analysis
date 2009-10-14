open QutSensors.AudioAnalysis.AED.AcousticEventDetection
open Common
open Xunit

[<Fact>]
let testToBlackAndWhite () =
    let f t d i j =
        let i4 = loadTestFile3 d "I3.txt" i j |> toBlackAndWhite t
        let i4m = loadTestFile3 d "I4.txt" i j
        matrixFloatEquals i4 i4m 0.001 |> Assert.True
    // TODO duplicated with Common
    f 9.0 "BAC2_20071015-045040" 256 5188
    f 3.0 "GParrots_JB2_20090607-173000.wav_minute_3" 256 5166
    
       
[<Fact>]
let testToFillIn () =
    let m = Array2D.create 5 10 0.0 |> Math.Matrix.of_array2
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
    let m = Array2D.create 5 10 0.0 |> Math.Matrix.of_array2
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
    let f d i j =
        let i6b = loadTestFile3 d "I6a.txt" i j |> joinHorizontalLines
        let i6bm = loadTestFile3 d "I6b.txt" i j
        matrixFloatEquals i6b i6bm 0.001 |> Assert.True
    test f
    
[<Fact>]
let testJoinVerticalLines () =
    let f d i j =
        let i6a = loadTestFile3 d "I4.txt" i j |> joinVerticalLines
        let i6am = loadTestFile3 d "I6a.txt" i j
        matrixFloatEquals i6a i6am 0.001 |> Assert.True
    test f
    
[<Fact>]
let testSmallFirstMin () =
    let t = 42
    Assert.Equal(0, smallFirstMin [0..3] [1;2;1;2] t)
    Assert.Equal(0, smallFirstMin [0..2] [1;1;1] t)
    Assert.Equal(t, smallFirstMin [0..1] [2;1] t) 

[<Fact>]
let testSmallThreshold () =
    let f d l t r =
        let ae2m = loadEventsFile d "AE2.txt" l
        Assert.Equal(r, smallThreshold t ae2m)
    // TODO duplicated with LargeEventsTest
    f "BAC2_20071015-045040" 1249 200 200
    f "GParrots_JB2_20090607-173000.wav_minute_3" 5291 100 55

[<Fact>]
let testFilterOutSmallEvents () =
    let f d l2 t l3 =
        let ae2m = loadEventsFile d "AE2.txt" l2
        let ae3 = filterOutSmallEvents t ae2m
        let ae3m = loadEventsFile d "AE3.txt" l3
        Assert.Equal(Seq.sort ae3m, Seq.sort ae3)
    // TODO duplicated with testSmallThreshold
    f "BAC2_20071015-045040" 1249 200 97
    f "GParrots_JB2_20090607-173000.wav_minute_3" 5291 100 811