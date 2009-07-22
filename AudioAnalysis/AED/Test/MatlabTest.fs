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
let testWiener2 () = 
    let i1 = loadTestFile "I1.txt" |> Math.Matrix.of_array2
    let i2 = wiener2 5 i1
    let i2m = loadTestFile "I2.txt"
    Assert.True (Util.Array2.a2FloatEquals i2 i2m 0.00001)