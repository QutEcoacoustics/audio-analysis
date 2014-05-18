module UtilTest

open QutSensors.AudioAnalysis.AED.Util
open Xunit

[<Fact>]
let testMatrixMap2 () =
    let m = Math.Matrix.ofList [[1.0]; [3.0]]
    let n = Math.Matrix.ofList [[1.0]; [1.0]]
    let r = Math.Matrix.ofList [[2.0]; [4.0]]
    Assert.Equal(r, matrixMap2 (+) m n)
    
[<Fact>]
let testMatrixMapi2Unzip () =
    let m = Math.Matrix.ofList [[1.0]; [3.0]]
    let x = Math.Matrix.ofList [[0.0]; [2.0]]
    let y = Math.Matrix.ofList [[2.0]; [4.0]]
    let (r,s) = matrixMapi2Unzip (fun _ _ x -> (x-1.0, x+1.0)) m
    Assert.Equal((x,y), (r,s))