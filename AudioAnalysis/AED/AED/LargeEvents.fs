module QutSensors.AudioAnalysis.AED.LargeEvents
    
open GetAcousticEvents
    
// TODO amalgamate with hist in wiener2
let hist xs cs =
    let ub = Seq.append (Seq.pairwise cs |> Seq.map (fun (x,y) -> x + ((y - x)/2))) [999999999] |> Seq.to_array // TODO what is MAX_INT?
    let a = Array.create (Seq.length ub) 0
    // TODO nasty bit of imperative code
    let f x = 
        let i = Array.find_index (fun b -> x <= b) ub
        a.[i] <- a.[i] + 1
    Seq.iter f xs
    a
    
let lastMin cs h = 
    let s = Seq.pairwise h |> Seq.map (fun (x,y) -> y-x) |> Seq.zip ( Seq.skip 1 cs)
    Seq.take (Seq.length s - 1) s |> Seq.fold (fun z (c,x) -> if x < 0 then c else z) (Seq.hd cs)
    
let threshold rs =
    let cs = seq {for i in 0..10 -> i * 1000}
    let t = hist (Seq.map (fun r -> r.Width * r.Height) rs) cs |> lastMin cs
    let d = 3000
    if t < d then d else t
