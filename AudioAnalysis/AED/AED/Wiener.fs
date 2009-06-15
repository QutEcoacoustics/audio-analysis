#light

module Wiener

(* wiener2 is a Matlab function.

   In wiener2.m the local means are calculated using a sum of smaller neighbourhoods around the edges but
   are always divided by a constant neighbourhood size. Perhaps this is an image processing trick (e.g.
   padding zeros around the image) or perhaps it is a bug, but implemented the same here.
*)

open Util.Array2

let mean a n = (a2fold (+) 0.0 a) / n
    
let variance a s m = (a2fold (fun z x -> z + (x*x)) 0.0 a) / s - (m*m)
    
// TODO change Array2 file to Util with multiple modules? Move this out.
let uncurry4 f (w, x, y, z) = f w x y z
 
let localMeansVariances a n =
    let f x y _ = 
        let nba = uncurry4 (Array2.sub a) (neighborhoodBounds a n x y)
        let nbs = float (n*n)
        let m = mean nba nbs
        (m, variance nba nbs m)
    Array2.mapi f a
    
let wiener2 a n =
    let lmv = localMeansVariances a n
    let nv = (a2fold (fun z (_, x) -> z + x) 0.0 lmv) / (float (a.GetLength(0) * a.GetLength(1))) // TODO single general mean function?
    let f x y e =
        let (m,v) = lmv.[x,y]
        m + ((max 0.0 (v - nv)) / max v nv) * ((float e) - m)
    Array2.mapi f a