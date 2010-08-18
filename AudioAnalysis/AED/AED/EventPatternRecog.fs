module QutSensors.AudioAnalysis.AED.EventPatternRecog

open Util

let groundParrotTemplate =
    [ cornersToRect 13.374694 13.548844 3832.910156 3617.578125;
      cornersToRect 13.664943 13.792653 3919.042969 3660.644531;
      cornersToRect 13.920363 14.117732 3962.109375 3703.710938;
      cornersToRect 14.257052 14.349932 4005.175781 3832.910156;
      cornersToRect 14.512472 14.640181 4048.242188 3919.042969;
      cornersToRect 14.814331 14.895601 4220.507813 4048.242188;
      cornersToRect 15.046531 15.232290 4349.707031 4048.242188;
      cornersToRect 15.371610 15.499320 4435.839844 4177.441406;
      cornersToRect 15.615420 15.812789 4478.906250 4220.507813;
      cornersToRect 16.277188 16.462948 4608.105469 4263.574219;
      cornersToRect 16.590658 16.695147 4694.238281 4392.773438;
      cornersToRect 16.834467 17.020227 4694.238281 4392.773438;
      cornersToRect 17.147937 17.264036 4737.304688 4478.906250;
      cornersToRect 17.391746 17.577506 4823.437500 4478.906250;
      cornersToRect 17.705215 17.821315 4780.371094 4521.972656 ]

let indexMinMap f xs =
    let ys = Seq.map f xs
    let m = Seq.min ys
    Seq.findIndex (fun y -> y = m) ys

let normaliseTimeFreq st sf td fr nt nf (t,f) =
    let g x s d l = let x' = round ((x - s) / d * l) in if x' < 1.0 then 1.0 else if x' > l then l else x'
    (g t st td nt, g f sf fr nf)
    
let centroids rs =
    let centre s l = s + l / 2.0
    Seq.map (fun r -> (centre (left r) (width r), centre (bottom r) (height r))) rs
    
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
    if or' < ol || ot < ob then 0.0 else let oa = (or'-ol) * (ot-ob) in 0.5 * (oa/((tr-tl)*(tt-tb)) + oa/((r-l)*(t-b)))
    
let freqMax = 11025.0
let freqBins = 256.0
let samplingRate = 22050.0

let candidates tb ttd tfr aes =
    let sfr = (boundedInterval tb 500.0 500.0 0.0 freqMax)
    let ss = Seq.filter (fun r -> r.Bottom >< sfr) aes // These are bottom left corners to overlay the template from
    let f x = Seq.filter (fun ae -> left ae >= left x && left ae < left x + ttd && bottom ae >= bottom x && bottom ae < bottom x + tfr) aes
    (ss, Seq.map f ss)

// Length of x and y axis' to scale time and frequency back to
// TODO investigate these formulas - correct for ground parrot template
let pixelAxisLengths ttd tfr = (ttd / (freqBins / samplingRate) |> round |> (+) 1.0, tfr / freqMax * (freqBins-1.0) |> round |> (+) 1.0)
    
let absLeftAbsBottom rs = (minmap left rs, minmap bottom rs)
    
let templateBounds t =
    let (tl, tb) = absLeftAbsBottom t
    (tl, tb, maxmap right t - tl, maxmap top t - tb)
        
let scoreEvents t aes =
    let (tl, tb, ttd, tfr) = templateBounds t
    let (xl, yl) = pixelAxisLengths ttd tfr
    let (tcs, tbls) = centroidsBottomLefts tl tb ttd tfr xl yl t
        
    let score rs =
        let (st, sf) = absLeftAbsBottom rs
        let (cs, bls) = centroidsBottomLefts st sf ttd tfr xl yl rs
        let f tc tbl =
            let i = indexMinMap (euclidianDist tc) cs   // index of closest centroid 
            overlap tbl tc (Seq.nth i bls) (Seq.nth i cs)
        Seq.map2 f tcs tbls |> Seq.sum
        
    let (saes, cs) = candidates tb ttd tfr aes // cs are the groups of acoustic events that are candiates for template matching
    Seq.zip saes (Seq.map score cs)

let detect template minScore aes = 
    let total = float (List.length template)
    seq {for (sae,s) in scoreEvents template aes do if s >= minScore then yield (sae, s / total)}
   
let detectGroundParrots aes = detect groundParrotTemplate 3.0 aes
