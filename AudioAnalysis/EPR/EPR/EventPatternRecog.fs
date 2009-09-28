module QutSensors.AudioAnalysis.EPR.EventPatternRecog

open QutSensors.AudioAnalysis.AED.Util

type Rectangle = {Left:float; Top:float; Right:float; Bottom:float;}
let left r = r.Left
let bottom r = r.Bottom
let bottomLeft r = (r.Left, r.Bottom)

let roundUpTo v x = if x < v then v else x
let roundDownTo v x = if x > v then v else x

let (><) x (l,u) = x > l && x < u // in open interval
let (>==<) x (l,u) = x >= l && x <= u // in closed interval

let normaliseTimeFreq st sf nt nf (t,f) =
    let g x s l = let x' = rnd ((x - s) / l) in if x' < 1.0 then 1.0 else if x' > l then l else x'
    (g t st nt, g f sf nf)
    
let normaliseTimeFreqs st sf nt nf = Seq.map (normaliseTimeFreq st sf nt nf)
    
// assuming l <= r
let centre l r = l + (r - l) / 2.0

// TODO investigate comprehension notation
let centroids rs = Seq.map (fun r -> (centre r.Left r.Right, centre r.Bottom r.Top)) rs
    
// TODO broken assumption that the same event will have both bottom and left? Same as matlab?
let absBottomLeft rs = let minmap f = Seq.min << Seq.map f in (minmap left rs, minmap bottom rs)

// TODO investigate performance optimisation by normalising individual points in tuple computations
let centroidsBottomLefts st sf nt nf rs = 
    let f = normaliseTimeFreqs st sf nt nf 
    (centroids rs |> f, Seq.map (fun r -> (r.Left, r.Bottom)) rs |> f)
    
let euclidianDist (x1, y1) (x2, y2) = (x1 - x2) ** 2.0 + (y1 - y2) ** 2.0 |> sqrt

// TODO generalise this to indexMinMap?
let closestCentroidIndex tc cs =
    let ds = Seq.map (euclidianDist tc) cs
    let m = Seq.min ds
    Seq.findIndex (fun d -> d = m) ds
    
let overlap (tl, tb) (tct, tcf) (b, l) (ct, cf) =
    let tr = tl + (tct - tl) * 2.0
    let tt = tb + (tcf - tb) * 2.0
    let r = l + (ct - l) * 2.0
    let t = b + (cf - b) * 2.0
    let ol, or', ob, ot = min tl l, max tr r, max tb b, min tt t
    if or' < ol || ot < ob then 0.0
        else let oa = or'-ol * ot-ob in 0.5 * ((oa/(tr-tl)*(tt-tb)) + (oa/(r-l)*(t-b)))
        
let detectGroundParrots t aes =
    // TODO use bottomLeft here
    let ttd = Seq.max (Seq.map (fun r -> r.Right) t) - Seq.min (Seq.map (fun r -> r.Left) t) // this is close but not quite exactly the same as matlab
    let tb = Seq.map (fun r -> r.Bottom) t |> Seq.min
    let tfr = Seq.max (Seq.map (fun r -> r.Right) t) - tb
    let fr = (tb - 500.0 |> roundUpTo 0.0, tb + 500.0 |> roundDownTo 11025.0) // TODO hardcoded upper frequency band
    let ys = Seq.filter (fun r -> r.Bottom >< fr) aes
                |> Seq.map (fun x -> Seq.filter (fun ae -> ae.Left >==< (x.Left, x.Left + ttd) && ae.Bottom >==< (x.Bottom, x.Bottom + tfr)) aes)
                
    // Template centroids and bottom left corners normalised
    let xl = ttd / 11.0  // TODO correct values
    let yl = tfr / 11025.0 * 256.0 // TODO correct values, fix freq max constant
    let tst = 1.0 // TODO
    let (tcs, tbls) = centroidsBottomLefts tst tb xl yl t
    let tcbls = Seq.zip tcs tbls
    
    let score tc tbl rs =
        let (st, sf) = absBottomLeft rs
        let (cs, bls) = centroidsBottomLefts st sf xl yl rs // TODO don't need to compute all bottom lefts here
        let i = closestCentroidIndex tc cs
        overlap tbl tc (normaliseTimeFreq st sf xl yl (bottomLeft (Seq.nth i rs))) (Seq.nth i cs)
    
    let ss = Seq.map (fun y -> Seq.fold (fun z (tc, tbl) -> z + score tc tbl y) 0.0 tcbls) ys
    // TODO return sequence AEs where score is higher than ?
    ss