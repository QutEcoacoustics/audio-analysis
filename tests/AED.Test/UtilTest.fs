module UtilTest

open QutSensors.AudioAnalysis.AED.Util
open Xunit
open Microsoft.FSharp
open Microsoft.FSharp.Math


[<Fact>]
let ``test within rect`` () =
    let r = lengthsToRect 5 9 15 5
    
    System.Diagnostics.Debug.Write("Rect: " + r.ToString())
    Assert.True  ( isWithin r (6, 13)        , " --> test a")
    Assert.False ( isWithin r (0, 0)         , " --> test b")
    Assert.False ( isWithin r (100, 100)     , " --> test c")
    Assert.False ( isWithin r (4, 10)        , " --> test d")
    Assert.False ( isWithin r (5, 8)         , " --> test e")
    Assert.False ( isWithin r (21, 8)        , " --> test f")
    Assert.False ( isWithin r (10, 16)       , " --> test g")
    Assert.True  ( isWithin r (5, 9)         , " --> test h")
    Assert.True  ( isWithin r (10, 13)       , " --> test i")
    Assert.True  ( isWithin r (19, 13)       , " --> test j")
    Assert.False ( isWithin r (20, 13)       , " --> test k")
    Assert.False ( isWithin r (19, 14)       , " --> test l")

[<Fact>]
let testMatrixMap2 () =
    let m = Math.Matrix.ofList [[1.0]; [3.0]]
    let n = Math.Matrix.ofList [[1.0]; [1.0]]
    let r = Math.Matrix.ofList [[2.0]; [4.0]]
    Assert.Equal<matrix>(r, matrixMap2 (+) m n)
    
[<Fact>]
let testMatrixMapi2Unzip () =
    let m = Math.Matrix.ofList [[1.0]; [3.0]]
    let x = Math.Matrix.ofList [[0.0]; [2.0]]
    let y = Math.Matrix.ofList [[2.0]; [4.0]]
    let (r,s) = matrixMapi2Unzip (fun _ _ x -> (x-1.0, x+1.0)) m
    Assert.Equal((x,y), (r,s))


