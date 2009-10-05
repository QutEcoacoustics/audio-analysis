module QutSensors.AudioAnalysis.EPR.EventPatternRecog

open QutSensors.AudioAnalysis.AED.Util

type Rectangle = {Left:float; Top:float; Right:float; Bottom:float;}
let left r = r.Left
let right r = r.Right
let bottom r = r.Bottom
let top r = r.Top
let bottomLeft r = (r.Left, r.Bottom)

let normaliseTimeFreq st sf nt nf (t,f) =
    let g x s l = let x' = rnd ((x - s) / l) in if x' < 1.0 then 1.0 else if x' > l then l else x'
    (g t st nt, g f sf nf)
    
let normaliseTimeFreqs st sf nt nf = Seq.map (normaliseTimeFreq st sf nt nf)
    
// assuming l <= r
let centre l r = l + (r - l) / 2.0

// TODO investigate comprehension notation
let centroids rs = Seq.map (fun r -> (centre r.Left r.Right, centre r.Bottom r.Top)) rs
    
let absLeftAbsBottom rs = (minmap left rs, minmap bottom rs)

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
        
let candidates sfr ttd tfr aes =
    let ss = Seq.filter (fun r -> r.Bottom >< sfr) aes
    let f x = Seq.filter (fun ae -> ae.Left >==< (x.Left, x.Left + ttd) && ae.Bottom >==< (x.Bottom, x.Bottom + tfr)) aes
    (ss, Seq.map f ss)
    
let freqMax = 11025.0
let freqBins = 256.0
let samplingRate = 22050.0
        
let detectGroundParrots t aes =
    let (tl, tb) = absLeftAbsBottom t
    // template right is close (3 decimal places) but not quite exactly the same as matlab
    let ttd, tfr = maxmap right t - tl, maxmap top t - tb
    
    // Length of x and y axis' to scale time and frequency back to
    let xl = ttd / (freqBins / samplingRate) |> rnd |> (+) 1.0
    let yl = tfr / freqMax * (freqBins-1.0) |> rnd |> (+) 1.0
    
    // Template centroids and bottom left corners normalised
    let tst = 1.0 // TODO
    let (tcs, tbls) = centroidsBottomLefts tst tb xl yl t
    let tcbls = Seq.zip tcs tbls
    
    let score tc tbl rs =
        let (st, sf) = absLeftAbsBottom rs // TODO broken assumption that the same event will have both bottom and left? Same as matlab?
        let (cs, bls) = centroidsBottomLefts st sf xl yl rs // TODO don't need to compute all bottom lefts here
        let i = closestCentroidIndex tc cs
        overlap tbl tc (normaliseTimeFreq st sf xl yl (bottomLeft (Seq.nth i rs))) (Seq.nth i cs)
        
    let (saes, cs) = candidates (boundedInterval tb 500.0 500.0 0.0 freqMax) ttd tfr aes
    let scores = Seq.map (fun c -> Seq.map (fun (tc, tbl) -> score tc tbl c) tcbls |> Seq.sum) cs
    seq {for (sae,score) in Seq.zip saes scores do if score >= 3.5 then yield sae}    