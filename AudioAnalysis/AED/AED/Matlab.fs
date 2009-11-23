module QutSensors.AudioAnalysis.AED.Matlab

open Util

// TODO amalgamate with hist in subbandmode

(* This is one particular variation of the Matlab hist function, from the help:
    N = HIST(Y,X), where X is a vector, returns the distribution of Y among bins with centers
    specified by X. The first bin includes data between -inf and the first center and the last
    bin includes data between the last bin and inf.
*)
let hist xs cs =
    let ub = Seq.append (Seq.pairwise cs |> Seq.map (fun (x,y) -> x + ((y - x)/2))) [999999999] |> Seq.toArray // TODO what is MAX_INT?
    let a = Array.create (Seq.length ub) 0
    // TODO nasty bit of imperative code
    let f x = 
        let i = Array.findIndex (fun b -> x <= b) ub
        a.[i] <- a.[i] + 1
    Seq.iter f xs
    a
    
    
let mean m n = Math.Matrix.sum m / n
    
let variance a n m = Math.Matrix.sum (a .* a) / n - (m*m)

// Assuming that the neighborhood dimensions n is odd so that it can be centred on a specific element       
let neighbourhoodBounds n h w x y =
    let m = (n-1)/2
    let subBounds p l =
        let s = if p < m then 0 else p - m
        let t = match p with
                | _ when p < m     -> p + m + 1
                | _ when p + m < l -> n
                | _                -> l - s
        (s, t)
    let (is, il) = subBounds (int x) h
    let (js, jl) = subBounds (int y) w
    (is, js, il, jl)
       
let localMeansVariances n (m:matrix) =
    let nbs = float (n*n)
    let f x y _ = 
        let nba = neighbourhoodBounds n (m.NumRows) (m.NumCols) x y |> m.Region
        let m = mean nba nbs
        (m, variance nba nbs m)
    matrixMapi2Unzip f m
    
(* In wiener2.m the local means are calculated using a sum of smaller neighbourhoods around the edges but
   are always divided by a constant neighbourhood size. Implemented the same here.
*)
let wiener2 n m =
    let (ms, vs) = localMeansVariances n m
    let mv = mean vs ((float) (m.NumRows * m.NumCols))
    let vs' = Math.Matrix.map (fun v -> (max 0.0 (v - mv)) / max v mv) vs
    ms + (vs' .* (m - ms))