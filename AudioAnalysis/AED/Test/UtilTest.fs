open QutSensors.AudioAnalysis.AED.Util
open Xunit

[<Fact>]
let testMatrixMap2 () =
    let m = Math.Matrix.of_list [[1.0]; [3.0]]
    let n = Math.Matrix.of_list [[1.0]; [1.0]]
    let r = Math.Matrix.of_list [[2.0]; [4.0]]
    Assert.Equal(r, matrixMap2 (+) m n)
    
[<Fact>]
let testMatrixMapi2Unzip () =
    let m = Math.Matrix.of_list [[1.0]; [3.0]]
    let x = Math.Matrix.of_list [[0.0]; [2.0]]
    let y = Math.Matrix.of_list [[2.0]; [4.0]]
    let (r,s) = matrixMapi2Unzip (fun _ _ x -> (x-1.0, x+1.0)) m
    Assert.Equal((x,y), (r,s))