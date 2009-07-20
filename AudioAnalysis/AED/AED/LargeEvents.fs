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
