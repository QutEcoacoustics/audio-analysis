module FsCheckArbs

open FsCheck
open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open QutSensors.AudioAnalysis.AED.Util

let nonNeg = Arb.generate<int>|> Gen.map abs

let pos = nonNeg |> Gen.map ((+) 1)

let pairGen = Gen.zip

let sequenceGen ms =
    let k m' m = gen { let! x = m
                       let! xs = m'
                       return (x::xs)}
    List.fold k (gen {return []}) ms
    
let replicateGenM n g = List.replicate n g |> sequenceGen

type ArbitraryModifiers = 
    static member Rectangle () =
         { 
            new Arbitrary<Rectangle<int, int>>() with
                override x.Generator = Gen.map4 (fun l t w h -> lengthsToRect l t w h) nonNeg nonNeg pos pos
         }
    static member AcousticEvent () =
        { new Arbitrary<AcousticEvent>()with
            override x.Generator = gen { let! rect = Arb.generate
                                         let r, b = right rect, bottom rect
                                         let! c = Gen.choose (1, (height rect) * (width rect))                                    
                                         let! elms = pairGen (Gen.choose (rect.Top, b)) (Gen.choose (rect.Left, r)) |> replicateGenM c 
                                         return {Bounds=rect;Elements=Set.ofList elms}}} 
     
let chk f = 
    Arb.register<ArbitraryModifiers>() |> ignore
    Check.Quick f