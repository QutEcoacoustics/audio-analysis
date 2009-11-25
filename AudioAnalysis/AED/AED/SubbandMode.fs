module QutSensors.AudioAnalysis.AED.SubbandMode

open Matlab
open Math.Vector.Generic

(* This is one particular variation of the Matlab hist function, from the help:
    N = HIST(Y,X), where X is a vector, returns the distribution of Y among bins with centers
    specified by X. The first bin includes data between -inf and the first center and the last
    bin includes data between the last bin and inf.
*)
// TODO would have been nicer to have this in terms of vectors or 1D arrays
let rowHist (m:matrix) v = 
    let a = toArray v |> Array.map (fun (mn:float, mx:float) -> (mn, mx, Array.create (int (floor (mx - mn)) + 1) 0)) |> ofArray  // assumption: mx > mn
    let f (mn, mx, h) x =
        let o = x - mn
        let mo = floor (mx - mn)
        let inc (a:int[]) i =
            a.[i] <- a.[i] + 1 // TODO can we do this without mutation
            a
        match o with
        | _ when o > mo                 -> (mn, mx, inc h (int mo))
        | _ when (o - (floor o)) <= 0.5 -> (mn, mx, inc h (int (floor o)))
        | _                             -> (mn, mx, inc h (int (ceil o)))
    Math.Matrix.foldByRow f a m |> toArray |> Array.map (fun (_, _, h) -> h) |> ofArray 
    // TODO this mapping across a vector with f::a-> b, by going via Array is used twice here
    
// TODO tests
let removeSubbandModeIntensities2 (m:matrix) =
    let ms = 
        let f (mn, mx) x = (min mn x, max mx x)
        Math.Matrix.foldByRow f (init (m.NumRows)(fun r -> (m.[r,0], m.[r,0]))) m
    let hs = rowHist m ms
    let mo = toArray hs |> Array.map (fun a -> Array.findIndex (fun x -> x= (Array.max a)) a)
    let modes =
        let f o (mn, mx) = 
            let t = (mn - mx) / 2.0
            let mode = mn + (float o)
            if mode > t then t else mode
        Array.map2 f mo (toArray ms)
    let smoothModes = smooth modes 11  
    //let (modes:Math.Vector<float>) = Math.Vector.Generic.map g ms
    Math.Matrix.mapi (fun r _ x -> x - smoothModes.[r]) m