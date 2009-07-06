#light

open Common
open Math.Matrix
open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open Xunit

[<Fact>]
let spiderTest () = 
    let m = Array2.create 3 4 0.0 |> of_array2
    Assert.Equal(Set.empty, spider m (0,0) Set.empty)
    m.[0,2] <- 1.0
    Assert.Equal(Set.of_list[(0,2)], spider m (0,2) Set.empty)
    m.[1,2] <- 1.0 // resulting matrix: 0 0 1 0 0
    m.[2,1] <- 1.0 //                   1 0 1 0 0
    m.[1,0] <- 1.0 //                   0 1 0 0 0 
    Assert.Equal(Set.of_list[(0,2);(1,0);(1,2);(2,1)], spider m (0,2) Set.empty)
 
[<Fact>]   
let getAcousticEventsTestQuick () =
    let m = Array2.create 4 3 0.0 |> of_array2
    Assert.Equal([], getAcousticEvents m)
    m.[0,1] <- 1.0
    Assert.Equal([{Left=1;Top=0}], (getAcousticEvents m))
    m.[1,1] <- 1.0
    m.[1,2] <- 1.0
    Assert.Equal([{Left=1;Top=0}], (getAcousticEvents m))
    m.[3,0] <- 1.0
    Assert.Equal(2, List.length (getAcousticEvents m))

[<Fact>]
let getAcousticEventsTest () =
    let i6bm = loadTestFile "I6b.txt" |> of_array2
    let ae = getAcousticEvents i6bm
    let MATLAB_LENGTH = 1229
    Assert.Equal(MATLAB_LENGTH, List.length ae)
    
    // matlab matrix indicies are 1 based, F# is 0 based
    let aem = loadTestFile2 "AE.txt" 6 MATLAB_LENGTH    
    let dec x = (int x) - 1
    let m = seq {for i in 0..(MATLAB_LENGTH-1) -> {Left=dec aem.[0,i]; Top=dec aem.[1,i]}}
    Assert.Equal(Seq.sort m, Seq.sort ae)