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
    
    
let mean m n = (Math.Matrix.fold (+) 0.0 m) / n
    
let variance a n m = (Math.Matrix.fold (fun z x -> z + (x*x)) 0.0 a) / n - (m*m)

// Assuming that the neighborhood dimensions n is odd so that it can be centred on a specific element       
let neighbourhoodBounds n w h x y = // TODO w h might be named oppositely
    let m = (n-1)/2
    let subBounds p l =
        let s = if p < m then 0 else p - m
        let t = match p with
                | _ when p < m     -> p + m + 1
                | _ when p + m < l -> n
                | _                -> l - s
        (s, t)
    let (xs, xl) = subBounds (int x) w
    let (ys, yl) = subBounds (int y) h
    (xs, ys, xl, yl)
    
let localMeansVariances n (m:matrix) =
    let f x y _ = 
        let nba = neighbourhoodBounds n (m.NumRows) (m.NumCols) x y |> m.Region
        let nbs = float (n*n)
        let m = mean nba nbs
        (m, variance nba nbs m)
    Math.Matrix.to_array2 m |> Array2D.mapi f // TODO does this do an array copy - perhaps write a function to do this better
    
(* In wiener2.m the local means are calculated using a sum of smaller neighbourhoods around the edges but
   are always divided by a constant neighbourhood size. Implemented the same here.
*)
let wiener2 n m =
    let lmv = localMeansVariances n m
    let a = Math.Matrix.to_array2 m
    let nv = (Util.Array2.a2fold (fun z (_, x) -> z + x) 0.0 lmv) / (float (a.GetLength(0) * a.GetLength(1))) // TODO single general mean function?
    let f x y e =
        let (m,v) = lmv.[x,y]
        m + ((max 0.0 (v - nv)) / max v nv) * ((float e) - m)
    Array2D.mapi f a