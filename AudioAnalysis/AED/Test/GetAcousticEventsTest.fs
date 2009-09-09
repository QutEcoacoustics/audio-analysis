open Common
open FsCheck
open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open Xunit

(*
type NonNegativeInt = NonNegative of int

type ArbitraryModifiers =
    static member NonNegativeInt () =
        { new Arbitrary<NonNegativeInt>() with
            override x.Arbitrary = arbitrary |> fmapGen (NonNegative << abs)
            override x.CoArbitrary (NonNegative i) = coarbitrary i
            override x.Shrink (NonNegative i) = shrink i |> Seq.filter ((<) 0) |> Seq.map NonNegative }
*)

// TODO how to type annotate this version: let nonNeg = arbitrary |> fmapGen abs
let nonNeg = gen { let! (x:int) = arbitrary
                   return (abs x) }

let pos = nonNeg |> fmapGen ((+) 1)

let pairGen g1 g2 = gen { let! x = g1
                          let! y = g2
                          return (x,y) }
                          
let sequenceGen ms =
    let k m' m = gen { let! x = m
                       let! xs = m'
                       return (x::xs)}
    List.fold k (gen {return []}) ms
    
let replicateGenM n g = List.replicate n g |> sequenceGen

// TODO liftGen4
type ArbitraryModifiers = 
    static member Rectangle () =
         { new Arbitrary<Rectangle>() with
            override x.Arbitrary = gen { let! top = nonNeg 
                                         let! left = nonNeg
                                         let! height = pos
                                         let! width = pos
                                         return {Left=left;Top=top;Width=width;Height=height} }}
    static member AcousticEvent () =
        { new Arbitrary<AcousticEvent>()with
            override x.Arbitrary = gen { let! rect = arbitrary
                                         let r = right rect
                                         let b = bottom rect
                                         let! c = choose (1, rect.Height * rect.Width)
                                         // elms <- replicateM c $ pairM (choose (t,b)) (choose (l,r))                                         
                                         let! elms = pairGen (choose (rect.Top,b)) (choose (rect.Left,r)) |> replicateGenM c 
                                         // return $ AE rect $ Set.fromList elms
                                         return {Bounds=rect;Elements=Set.of_list elms}}} 

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
    let ae = loadTestFile "I6b.txt" |> getAcousticEvents |> bounds
    let MATLAB_LENGTH = 1229
    Assert.Equal(MATLAB_LENGTH, Seq.length ae)
    let aem = loadEventsFile "AE.txt" MATLAB_LENGTH    
    Assert.Equal(Seq.sort aem, Seq.sort ae)