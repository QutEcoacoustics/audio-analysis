module FsCheckArbs

open FsCheck
open Acoustics.AED.GetAcousticEvents
open Acoustics.AED.Util

let nonNeg = Arb.generate<int> |> Gen.map abs

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
        { new Arbitrary<AcousticEvent>() with
            override x.Generator = gen { let! l = nonNeg
                                         let! t = nonNeg
                                         let! w = nonNeg
                                         let! h = nonNeg
                                         let! c = Gen.choose (1, w * h)                                    
                                         let! elms = pairGen (Gen.choose (t, t + h)) (Gen.choose (l, l + w)) |> replicateGenM c 
                                         return {Bounds=(lengthsToRect l t w h);Elements=Set.ofList elms}}} 
     
let chk f = 
    Arb.register<ArbitraryModifiers>() |> ignore
    Check.Quick f