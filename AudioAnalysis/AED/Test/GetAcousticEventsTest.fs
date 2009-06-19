#light

open Math.Matrix
open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open Xunit

[<Fact>]
let spiderTest () = 
    let m = Array2.create 2 3 0.0 |> of_array2
    Assert.Equal(Set.empty, spider m (0,0) Set.empty)
    m.[0,1] <- 1.0
    Assert.Equal(Set.of_list[(0,1)], spider m (0,1) Set.empty)
    m.[1,1] <- 1.0
    m.[1,2] <- 1.0
    Assert.Equal(Set.of_list[(0,1);(1,1);(1,2)], spider m (0,1) Set.empty)