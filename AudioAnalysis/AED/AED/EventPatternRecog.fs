module QutSensors.AudioAnalysis.AED.EventPatternRecog

open Util

let absLeftAbsBottom rs = (minmap left rs, minmap bottom rs)

let groundParrotTemplate =
    [ cornersToRect 5.166440 5.305733 4048.242188 3703.710938;
      cornersToRect 5.421859 5.630799 4005.175781 3531.445313
      cornersToRect 5.746939 5.851408 4134.375000 3875.976563
      cornersToRect 5.979138 6.211293 4220.507813 3875.976563 
      cornersToRect 6.315828 6.431905 4306.640625 3962.109375 
      cornersToRect 6.559637 6.791792 4392.773438 4048.242188 
      cornersToRect 6.896327 7.035619 4565.039063 4220.507813 
      cornersToRect 7.151746 7.349077 4651.171875 4306.640625 
      cornersToRect 7.709025 7.929572 4995.703125 4392.773438 
      cornersToRect 8.045714 8.173399 4780.371094 4478.906250 
      cornersToRect 8.312744 8.533291 4823.437500 4478.906250 
      cornersToRect 8.870023 8.974492 4866.503906 4694.238281 ]
      
let indexMinMap f xs =
    let ys = Seq.map f xs
    let m = Seq.min ys
    Seq.findIndex (fun y -> y = m) ys
    
// TODO review all the normalising code
let normaliseTimeFreq st sf td fr nt nf (t,f) =
    let g x s d l = let x' = rnd ((x - s) / d * l) in if x' < 1.0 then 1.0 else if x' > l then l else x'
    (g t st td nt, g f sf fr nf)
    
let centroids rs =
    let centre l r = l + (r - l) / 2.0  // assuming l <= r
    Seq.map (fun r -> (centre r.Left r.Right, centre r.Bottom r.Top)) rs
    
// TODO investigate performance optimisation by normalising individual points in tuple computations
let centroidsBottomLefts st sf td fr nt nf rs = 
    let f = Seq.map (normaliseTimeFreq st sf td fr nt nf)
    (centroids rs |> f, Seq.map bottomLeft rs |> f)
    
let euclidianDist (x1, y1) (x2, y2) = (x1 - x2) ** 2.0 + (y1 - y2) ** 2.0 |> sqrt
    
let overlap (tl, tb) (tct, tcf) (l, b) (ct, cf) =
    let tr = tl + (tct - tl) * 2.0
    let tt = tb + (tcf - tb) * 2.0
    let r = l + (ct - l) * 2.0
    let t = b + (cf - b) * 2.0
    let ol, or', ob, ot = max tl l, min tr r, max tb b, min tt t
    if or' < ol || ot < ob then 0.0
        else let oa = (or'-ol) * (ot-ob) in 0.5 * (oa/((tr-tl)*(tt-tb)) + oa/((r-l)*(t-b)))
        
let candidates sfr ttd tfr aes =
    let ss = Seq.filter (fun r -> r.Bottom >< sfr) aes
    let f x = Seq.filter (fun ae -> ae.Left >==< (x.Left, x.Left + ttd) && ae.Bottom >==< (x.Bottom, x.Bottom + tfr)) aes
    (ss, Seq.map f ss)
    
let freqMax = 11025.0
let freqBins = 256.0
let samplingRate = 22050.0
        
let detectGroundParrots aes =
    let t = groundParrotTemplate
    let (tl, tb) = absLeftAbsBottom t
    // template right is close (3 decimal places) but not quite exactly the same as matlab
    let ttd, tfr = maxmap right t - tl, maxmap top t - tb
    
    // Length of x and y axis' to scale time and frequency back to
    // TODO investigate these formulas - correct for ground parrot template
    let xl = ttd / (freqBins / samplingRate) |> rnd |> (+) 1.0
    let yl = tfr / freqMax * (freqBins-1.0) |> rnd |> (+) 1.0
    
    let tcbls = centroidsBottomLefts tl tb ttd tfr xl yl t |> uncurry Seq.zip
        
    let score rs =
        let (st, sf) = absLeftAbsBottom rs // TODO broken assumption that the same event will have both bottom and left? Same as matlab?
        let (cs, bls) = centroidsBottomLefts st sf ttd tfr xl yl rs
        let f tc tbl =
            let i = indexMinMap (euclidianDist tc) cs   // index of closest centroid 
            overlap tbl tc (Seq.nth i bls) (Seq.nth i cs)
        Seq.map (fun (tc, tbl) -> f tc tbl) tcbls |> Seq.sum
        
    let (saes, cs) = candidates (boundedInterval tb 500.0 500.0 0.0 freqMax) ttd tfr aes
    seq {for (sae,score) in Seq.zip saes (Seq.map score cs) do if score >= 4.0 then yield sae}