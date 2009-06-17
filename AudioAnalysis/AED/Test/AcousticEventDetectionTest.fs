#light

open QutSensors.AudioAnalysis.AED.AcousticEventDetection
open Common
open Math.Matrix
open Util.Array2
open Xunit

[<Fact>]
let testToBlackAndWhite () =
    let i3m = loadTestFile "I3.txt"
    let i4 = toBlackAndWhite 9.0 (of_array2 i3m) |> to_array2
    let i4m = loadTestFile "I4.txt"
    a2FloatEquals i4 i4m 0.001 |> Assert.True
    
[<Fact>]
let testFindRowDist () =
    let m = Array2.create 5 10 0.0 |> of_array2
    Assert.Equal(0, findRowDist m 0 0 3)
    Assert.Equal(0, findRowDist m 1 9 3)
    m.[0,1] <- 1.0
    Assert.Equal(0, findRowDist m 0 0 3)
    m.[1,2] <- 1.0
    Assert.Equal(1, findRowDist m 1 0 3)
    m.[2,8] <- 1.0
    Assert.Equal(0, findRowDist m 2 8 3)
       
[<Fact>]
let testJoinHorizontalLines () =
    let m = Array2.create 5 10 0.0 |> of_array2
    Assert.Equal(m, joinHorizontalLines m)
    m.[0,1] <- 1.0
    Assert.Equal(m, joinHorizontalLines m)
    m.[0,2] <- 1.0
    Assert.Equal(m, joinHorizontalLines m)
    m.[0,4] <- 1.0
    let m' = copy m
    m'.[0,3] <- 1.0
    Assert.Equal(m', joinHorizontalLines m)
    let i6am = loadTestFile "I6a.txt"
    let i6b = joinHorizontalLines (of_array2 i6am) |> to_array2
    let i6bm = loadTestFile "I6b.txt"
    a2FloatEquals i6b i6bm 0.001 |> Assert.True
    
[<Fact>]
let TestJoinVerticalLines () =
    let i4m = loadTestFile "I4.txt"
    let i6a = joinVerticalLines (of_array2 i4m) |> to_array2
    let i6am = loadTestFile "I6a.txt"
    a2FloatEquals i6a i6am 0.001 |> Assert.True