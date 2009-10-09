module QutSensors.AudioAnalysis.AED.EventPatternRecog

open AudioAnalysis
open Util

type Rectangle = {Left:float; Top:float; Right:float; Bottom:float;}
let left r = r.Left
let right r = r.Right
let bottom r = r.Bottom
let top r = r.Top
let bottomLeft r = (r.Left, r.Bottom)
let absLeftAbsBottom rs = (minmap left rs, minmap bottom rs)

let groundParrotTemplate =
    [ {Left=13.359148; Right=13.591436; Bottom=3545.294118; Top=3977.647059};
      {Left=14.242014; Right=14.381387; Bottom=3631.764706; Top=4064.117647};
      {Left=14.497580; Right=14.787940; Bottom=3847.941176; Top=4237.058824};
      {Left=14.811229; Right=14.962216; Bottom=3977.647059; Top=4323.529412};
      {Left=15.043562; Right=15.287464; Bottom=4020.882353; Top=4366.764706};
      {Left=15.380445; Right=15.531432; Bottom=4107.352941; Top=4539.705882};
      {Left=16.263311; Right=16.495599; Bottom=4280.294118; Top=4712.647059};
      {Left=16.588577; Right=16.774407; Bottom=4366.764706; Top=4712.647059};
      {Left=17.134560; Right=17.320390; Bottom=4323.529412; Top=4755.882353};
      {Left=17.703775; Right=17.866377; Bottom=4366.764706; Top=4842.352941};
      {Left=17.889642; Right=18.354218; Bottom=4237.058824; Top=4885.588235}]

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
        
let detectGroundParrots' aes =
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
    seq {for (sae,score) in Seq.zip saes (Seq.map score cs) do if score >= 3.5 then yield sae}
    
// TODO This adds a circular dependency back to AudioAnalysis
let detectGroundParrots aes =
    Seq.map (fun (ae:AcousticEvent) -> {Left=ae.StartTime; Right=ae.StartTime + ae.Duration; Bottom=(float) ae.MinFreq; Top=(float) ae.MaxFreq}) aes
        |> detectGroundParrots'
        |> Seq.map (fun r -> new AcousticEvent(r.Left, r.Right - r.Left, r.Bottom, r.Top))