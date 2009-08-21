module QutSensors.AudioAnalysis.AED.LargeEvents
    
open GetAcousticEvents
    
let lastMin cs h = 
    let s = Seq.pairwise h |> Seq.map (fun (x,y) -> y-x) |> Seq.zip ( Seq.skip 1 cs)
    Seq.take (Seq.length s - 1) s |> Seq.fold (fun z (c,x) -> if x < 0 then c else z) (Seq.hd cs)
    
let threshold rs =
    let cs = seq {for i in 0..10 -> i * 1000}
    let t = Matlab.hist (areas rs) cs |> lastMin cs
    let d = 3000
    if t < d then d else t
    
let aeToMatrix ae =
    let r = ae.Bounds
    Math.Matrix.init r.Height r.Width (fun i j -> if ae.Elements.Contains (r.Top + i, r.Left + j) then 1.0 else 0.0)

let separateLargeEvents aes =
    let t = bounds aes |> threshold
    let f ae =
        let m = aeToMatrix ae
        ae.Bounds
    Seq.collect (fun ae -> if area ae.Bounds < t then [ae.Bounds] else [f ae]) aes