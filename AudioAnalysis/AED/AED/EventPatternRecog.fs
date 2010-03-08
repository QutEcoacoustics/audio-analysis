module QutSensors.AudioAnalysis.AED.EventPatternRecog

open Util

let groundParrotTemplate =
    [ cornersToRect 5.166439909297052 5.294149659863946 4048.24218750 3703.7109375;
      cornersToRect 5.421859410430839 5.619229024943310 4005.17578125 3531.4453125;
      cornersToRect 5.746938775510204 5.839818594104308 4134.37500000 3875.9765625;
      cornersToRect 5.979138321995465 6.199727891156463 4220.50781250 3875.9765625; 
      cornersToRect 6.315827664399093 6.420317460317460 4306.64062500 3962.1093750; 
      cornersToRect 6.559637188208616 6.780226757369614 4392.77343750 4048.2421875; 
      cornersToRect 6.896326530612245 7.024036281179138 4565.03906250 4220.5078125; 
      cornersToRect 7.151746031746032 7.337505668934241 4651.17187500 4306.6406250; 
      cornersToRect 7.709024943310658 7.918004535147392 4995.70312500 4392.7734375; 
      cornersToRect 8.045714285714286 8.161814058956917 4780.37109375 4478.9062500; 
      cornersToRect 8.312743764172335 8.521723356009071 4823.43750000 4478.9062500; 
      cornersToRect 8.870022675736962 8.962902494331066 4866.50390625 4694.23828125 ]
      
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

let detect template minScore aes = seq {for (sae,s) in scoreEvents template aes do if s >= minScore then yield sae}

// TODO should this return a score    
let detectGroundParrots aes = detect groundParrotTemplate 4.0 aes