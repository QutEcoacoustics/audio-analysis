#light
module QutSensors.AudioAnalysis.AED.LargeEvents
    
let hist xs cs =
    let ub = Seq.append (Seq.pairwise cs |> Seq.map (fun (x,y) -> x + ((y - x)/2))) [999999999] |> Seq.to_array // TODO what is MAX_INT?
    let a = Array.create (Seq.length ub) 0
    // TODO nasty bit of imperative code
    let f x = 
        let i = Array.find_index (fun b -> x <= b) ub
        a.[i] <- a.[i] + 1
    Seq.iter f xs
    a
    
// seq {for i in 0..10 -> i * 1000}