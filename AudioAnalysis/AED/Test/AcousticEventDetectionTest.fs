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
let testToFillIn () =
    let m = Array2.create 5 10 0.0 |> of_array2
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
    
[<Fact>]
let testJoinHorizontalLines () =
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