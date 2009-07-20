module QutSensors.AudioAnalysis.AED.Matlab

// TODO amalgamate with hist in subbandmode

(* This is one particular variation of the Matlab hist function, from the help:
    N = HIST(Y,X), where X is a vector, returns the distribution of Y among bins with centers
    specified by X. The first bin includes data between -inf and the first center and the last
    bin includes data between the last bin and inf.
*)
let hist xs cs =
    let ub = Seq.append (Seq.pairwise cs |> Seq.map (fun (x,y) -> x + ((y - x)/2))) [999999999] |> Seq.to_array // TODO what is MAX_INT?
    let a = Array.create (Seq.length ub) 0
    // TODO nasty bit of imperative code
    let f x = 
        let i = Array.findIndex (fun b -> x <= b) ub
        a.[i] <- a.[i] + 1
    Seq.iter f xs
    a