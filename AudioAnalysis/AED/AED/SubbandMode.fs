module QutSensors.AudioAnalysis.AED.SubbandMode

open Matlab
open Math.Vector.Generic
open Util
    
let removeSubbandModeIntensities2 (m:matrix) =
    let ms = 
        let f (mn, mx) x = (min mn x, max mx x)
        Math.Matrix.foldByRow f (init (m.NumRows)(fun r -> (m.[r,0], m.[r,0]))) m
    let hs = mapByRow (fun r -> histf r (seq{Seq.min r .. Seq.max r})) m
    let mo = toArray hs |> Array.map (fun a -> Array.findIndex (fun x -> x= (Array.max a)) a)
    let modes =
        let f o (mn, mx) = 
            let t = (mn - mx) / 2.0
            let mode = mn + (float o)
            if mode > t then t else mode
        Array.map2 f mo (toArray ms)
    let smoothModes = smooth 11 modes  
    //let (modes:Math.Vector<float>) = Math.Vector.Generic.map g ms
    Math.Matrix.mapi (fun r _ x -> x - smoothModes.[r]) m