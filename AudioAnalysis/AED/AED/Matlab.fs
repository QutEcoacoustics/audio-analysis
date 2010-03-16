module QutSensors.AudioAnalysis.AED.Matlab

open Util

(* This is one particular variation of the Matlab hist function, from the help:
    N = HIST(Y,X), where X is a vector, returns the distribution of Y among bins with centers
    specified by X. The first bin includes data between -inf and the first center and the last
    bin includes data between the last bin and inf.
    
    Tried using (o:'a Math.INumeric) instead of individual arguments, but couldn't figure out
    to get float and int implementations of INumeric.
*)
let inline hist add sub half mx xs cs =
    let ub = Seq.append (Seq.pairwise cs |> Seq.map (fun (x,y) -> add x (half (sub y x)))) [mx] |> Seq.toArray
    let a = Array.create (Seq.length ub) 0
    let f x = 
        let i = Array.findIndex (fun b -> x <= b) ub
        a.[i] <- a.[i] + 1
    Seq.iter f xs
    a
    
let histf xs cs = hist (+) (-) (fun x -> x / 2.0) System.Double.MaxValue xs cs
let histi xs cs = hist (+) (-) (fun x -> x / 2) System.Int32.MaxValue xs cs
      
    
(* 
yy = smooth(y,span) sets the span of the moving average to span. span must be odd.

If span = 5 then the first few elements of yy are given by:
yy(1) = y(1)
yy(2) = (y(1) + y(2) + y(3))/3
yy(3) = (y(1) + y(2) + y(3) + y(4) + y(5))/5
yy(4) = (y(2) + y(3) + y(4) + y(5) + y(6))/5
...
*)
let smooth s (a:float []) = 
    let n = (s - 1) / 2
    let f i _ =
        let (b, l) = match i with
                     | _ when i < n                  -> (0, i*2+1)
                     | _ when i + n < a.GetLength(0) -> (i-n, s)
                     | _                             -> (i-(a.GetLength(0)-1-i), (a.GetLength(0)-1-i)*2+1)
        Array.average (Array.sub a b l)
    Array.mapi f a
    
let findPeaks (a:'a[]) =
    let rec f z i =
        if i >= a.GetLength(0) || (i+1) = a.GetLength(0) then z
            else if a.[i] > a.[i-1] && a.[i] > a.[i+1]
                    then f (i::z) (i+2)
                    else f z (i+1)
    if a.GetLength(0) > 2 then f [] 1 else []
    
let mean (m:matrix) = Math.Matrix.sum m / (float) (m.NumRows * m.NumCols)
    
let variance (a:matrix) n m = Math.Matrix.sum (a .* a) / n - (m*m)

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
    let ms = m .* m
    let n' = (n-1)/2
    let sums = Math.Matrix.zero m.NumRows m.NumCols
    let sumsSquares = Math.Matrix.zero m.NumRows m.NumCols
    
    let imax, jmax = m.NumRows-1, m.NumCols-1
    let inline g (m:matrix) i j = if i < 0 || j < 0 || i > imax || j > jmax then 0.0 else m.[i,j]
    
    for oi=0 to imax do
        let mutable s = 0.0;
        let mutable ss = 0.0;
        for i=oi-n' to oi+n' do
            for j=0 to n' do
                s <- s + g m i j
                ss <- ss + g ms i j
            done
        done
        sums.[oi,0] <- s
        sumsSquares.[oi,0] <- ss
    done
    
    for oi=0 to imax do
      for oj=1 to jmax do
        let mutable s = 0.0;
        let mutable ss = 0.0;
        for i=oi-n' to oi+n' do
            let jl, jh = oj-n'-1, oj+n'
            s <- s - g m i jl + g m i jh
            ss <- ss - g ms i jl + g ms i jh
        done
        sums.[oi,oj] <- sums.[oi, oj-1] + s
        sumsSquares.[oi,oj] <- sumsSquares.[oi, oj-1] + ss
      done
    done
    
    let nf = float (n*n)
    let means = sums * (1.0/nf)
    let variances = sumsSquares * (1.0/nf)
    Math.Matrix.inplaceSub variances (means .* means)
    (means, variances)   
 
(* In wiener2.m the local means are calculated using a sum of smaller neighbourhoods around the edges but
   are always divided by a constant neighbourhood size. Implemented the same here.
*)
let wiener2 n m =
    let (ms, vs) = localMeansVariances n m
    let mv = mean vs
    let vs' = Math.Matrix.map (fun v -> (max 0.0 (v - mv)) / max v mv) vs
    ms + (vs' .* (m - ms))