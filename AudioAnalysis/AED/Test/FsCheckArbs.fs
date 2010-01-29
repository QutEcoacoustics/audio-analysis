module FsCheckArbs

open FsCheck
open FsCheckXunit
open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open QutSensors.AudioAnalysis.AED.Util

let nonNeg = arbitrary |> fmapGen abs

let pos = nonNeg |> fmapGen ((+) 1)

let pairGen = liftGen2 (fun x y -> (x,y))
                          
let sequenceGen ms =
    let k m' m = gen { let! x = m
                       let! xs = m'
                       return (x::xs)}
    List.fold k (gen {return []}) ms
    
let replicateGenM n g = List.replicate n g |> sequenceGen

type ArbitraryModifiers = 
    static member Rectangle () =
         { new Arbitrary<Rectangle<int>>() with
            override x.Arbitrary = liftGen4 (fun l t w h -> lengthsToRect l t w h) nonNeg nonNeg pos pos }
    static member AcousticEvent () =
        { new Arbitrary<AcousticEvent>()with
            override x.Arbitrary = gen { let! rect = arbitrary
                                         let r, b = right rect, bottom rect
                                         let! c = choose (1, rect.Height * rect.Width)                                    
                                         let! elms = pairGen (choose (rect.Top,b)) (choose (rect.Left,r)) |> replicateGenM c 
                                         return {Bounds=rect;Elements=Set.ofList elms}}} 
     
let chk f = 
    overwriteGenerators<ArbitraryModifiers>()
    check config f