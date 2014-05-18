module MatlabTest

open Common
open FsCheckArbs
open QutSensors.AudioAnalysis.AED.Matlab
open Xunit

[<Fact>]
let testHist () =
    let cs = seq {for i in 0..10 -> i * 1000}
    Assert.Equal([|2; 1; 0; 0; 0; 0; 0; 0; 0; 0; 1|], histi [1;500;501;20000] cs)
    
[<Fact>]
let testFindPeaks () =
    let p (a:int []) = 
        let r = findPeaks a
        if r.Length = 0 then true
            else List.min r > 0 && List.max r < (a.GetLength(0)-1) && List.forall (fun i -> a.[i] > a.[i-1] && a.[i] > a.[i+1]) r
    chk p
    
[<Fact>]
let testMean () = Assert.Equal(2.5, mean (Math.Matrix.ofList [[1.0; 2.0]; [3.0; 4.0]]))

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
let wiener2 () = 
    let f md = 
        let i2 = loadTestFile "I1.csv" md |> wiener2 5
        let i2m = loadTestFile "I2.csv" md 
        Assert.True (matrixFloatEquals i2 i2m 0.0001)
    testAll f