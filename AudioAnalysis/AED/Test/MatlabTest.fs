open QutSensors.AudioAnalysis.AED.Matlab
open Xunit

[<Fact>]
let testHist () =
    let cs = seq {for i in 0..10 -> i * 1000}
    Assert.Equal([|2; 1; 0; 0; 0; 0; 0; 0; 0; 0; 1|], hist [1;500;501;20000] cs)