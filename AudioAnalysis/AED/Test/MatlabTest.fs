open Common
open QutSensors.AudioAnalysis.AED.Matlab
open Xunit

[<Fact>]
let testHist () =
    let cs = seq {for i in 0..10 -> i * 1000}
    Assert.Equal([|2; 1; 0; 0; 0; 0; 0; 0; 0; 0; 1|], hist [1;500;501;20000] cs)
    
[<Fact>]
let testMean () = Assert.Equal(2.5, mean (Math.Matrix.of_list [[1.0; 2.0]; [3.0; 4.0]]) 4.0)

[<Fact>]
let testNeighbourhoodBounds () =
    let d = 100
    let a = Array2D.create d d 0 
    let f e x y = Assert.Equal(e, neighbourhoodBounds 5 d d x y)
    f (0, 0, 3, 3) 0 0
    f (0, 0, 4, 3) 1 0
    f (0, 0, 5, 3) 2 0
    f (1, 0, 5, 3) 3 0
    f (1, 1, 5, 5) 3 3
    f (95, 0, 5, 3) 97 0
    f (96, 0, 4, 3) 98 0
    f (97, 0, 3, 3) 99 0
    
[<Fact>]
let testWiener2 () = 
    let i2 = loadTestFile "I1.txt" |> wiener2 5
    let i2m = loadTestFile "I2.txt"
    Assert.True (matrixFloatEquals i2 i2m 0.00001)