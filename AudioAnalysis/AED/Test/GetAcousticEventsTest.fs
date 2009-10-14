open Common
open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open Xunit

[<Fact>]
let spiderTest () = 
    let m = Array2D.create 3 4 0.0 |> Math.Matrix.of_array2
    Assert.Equal(Set.empty, spider m [(0,0)] Set.empty)
    m.[0,2] <- 1.0
    Assert.Equal(Set.of_list [(0,2)], spider m [(0,2)] Set.empty)
    m.[1,2] <- 1.0 // resulting matrix: 0 0 1 0 0
    m.[2,1] <- 1.0 //                   1 0 1 0 0
    m.[1,0] <- 1.0 //                   0 1 0 0 0 
    Assert.Equal(Set.of_list [(0,2);(1,0);(1,2);(2,1)], spider m [(0,2)] Set.empty)
 
[<Fact>]   
let getAcousticEventsTestQuick () =
    let m = Array2D.create 4 3 0.0 |> Math.Matrix.of_array2
    Assert.Equal([], getAcousticEvents m)
    m.[0,1] <- 1.0
    Assert.Equal([{Bounds={Left=1;Top=0;Width=1;Height=1}; Elements=Set.of_list [(0,1)]}], (getAcousticEvents m))
    m.[1,1] <- 1.0
    m.[1,2] <- 1.0
    Assert.Equal([{Bounds={Left=1;Top=0;Width=2;Height=2}; Elements=Set.of_list [(0,1);(1,1);(1,2)]}], (getAcousticEvents m))
    m.[3,0] <- 1.0
    Assert.Equal(2, List.length (getAcousticEvents m))

[<Fact>]
let getAcousticEventsTest () =
    let f d i j ael =
        let ae = loadTestFile3 d "I6b.txt" i j |> getAcousticEvents |> bounds
        Assert.Equal(ael, Seq.length ae)
        let aem = loadEventsFile d "AE.txt" ael    
        Assert.Equal(Seq.sort aem, Seq.sort ae)
    // TODO duplicated with Common
    f "BAC2_20071015-045040" 256 5188 1229
    f "GParrots_JB2_20090607-173000.wav_minute_3" 256 5166 5229