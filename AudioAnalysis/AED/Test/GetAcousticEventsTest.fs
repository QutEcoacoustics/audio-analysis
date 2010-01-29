module GetAcousticEventsTest

open Common
open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open QutSensors.AudioAnalysis.AED.Util
open Xunit

[<Fact>]
let spiderTest () = 
    let m = Math.Matrix.zero 3 4
    Assert.Equal(Set.empty, spider m [(0,0)] Set.empty)
    m.[0,2] <- 1.0
    Assert.Equal(Set.ofList [(0,2)], spider m [(0,2)] Set.empty)
    m.[1,2] <- 1.0 // resulting matrix: 0 0 1 0 0
    m.[2,1] <- 1.0 //                   1 0 1 0 0
    m.[1,0] <- 1.0 //                   0 1 0 0 0 
    Assert.Equal(Set.ofList [(0,2);(1,0);(1,2);(2,1)], spider m [(0,2)] Set.empty)
 
[<Fact>]   
let getAcousticEventsTestQuick () =
    let m = Math.Matrix.zero 4 3
    Assert.Equal([], getAcousticEvents m)
    m.[0,1] <- 1.0
    Assert.Equal([{Bounds=lengthsToRect 1 0 1 1; Elements=Set.ofList [(0,1)]}], (getAcousticEvents m))
    m.[1,1] <- 1.0
    m.[1,2] <- 1.0
    Assert.Equal([{Bounds=lengthsToRect 1 0 2 2; Elements=Set.ofList [(0,1);(1,1);(1,2)]}], (getAcousticEvents m))
    m.[3,0] <- 1.0
    Assert.Equal(2, List.length (getAcousticEvents m))

[<Fact>]
let getAcousticEventsTest () =
    let f md =
        let ae = loadTestFile "I6.csv" md |> getAcousticEvents |> bounds
        let aem = loadEventsFile "AE1.csv" md  
        Assert.Equal(Seq.length aem, Seq.length ae)
        Assert.Equal(Seq.sort aem, Seq.sort ae)
    testAll f