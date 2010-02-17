module QutSensors.AudioAnalysis.AED.EventPatternRecog

open Util

let groundParrotTemplate =
    [ cornersToRect 5.166440 5.294150 4048.242188 3703.710938;
      cornersToRect 5.421859 5.619229 4005.175781 3531.445313;
      cornersToRect 5.746939 5.839819 4134.375000 3875.976563;
      cornersToRect 5.979138 6.199728 4220.507813 3875.976563; 
      cornersToRect 6.315828 6.420317 4306.640625 3962.109375; 
      cornersToRect 6.559637 6.780227 4392.773438 4048.242188; 
      cornersToRect 6.896327 7.024036 4565.039063 4220.507813; 
      cornersToRect 7.151746 7.337506 4651.171875 4306.640625; 
      cornersToRect 7.709025 7.918005 4995.703125 4392.773438; 
      cornersToRect 8.045714 8.161814 4780.371094 4478.906250; 
      cornersToRect 8.312744 8.521723 4823.437500 4478.906250; 
      cornersToRect 8.870023 8.962902 4866.503906 4694.238281 ]
      
let indexMinMap f xs =
    let ys = Seq.map f xs
    let m = Seq.min ys
    Seq.findIndex (fun y -> y = m) ys
    
// This is purely to deal with rounding differences (0.4999 in F# vs 0.5 in Matlab rounded to 1) in the tests
let rnd' x = if floatEquals 0.5 (x - (floor x)) 0.0001 then ceil x else round x

let normaliseTimeFreq st sf td fr nt nf (t,f) =
    let g x s d l = let x' = rnd' ((x - s) / d * l) in if x' < 1.0 then 1.0 else if x' > l then l else x'
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
    //let res = if or' < ol || ot < ob then 0.0
    if or' < ol || ot < ob then 0.0
                 else
                    let oa = (or'-ol) * (ot-ob)
                    let res' = 0.5 * (oa/((tr-tl)*(tt-tb)) + oa/((r-l)*(t-b)))
                    if System.Double.IsNaN res' then 0.0 else res'
                    //res'
    //if System.Double.IsNaN res then failwith (sprintf "tl: %f, tb: %f, %f, %f, l: %f, b: %f, ct: %f, cf: %f, tr: %f, tt: %f, r: %f, t: %f, ol: %f, or': %f, ob: %f, ot: %f"
    //    tl tb tct tcf l b ct cf tr tt r t ol or' ob ot) else res
    // Is this a genuine fix (the possibility of getting NAN due to t=b=cf) or is there a problem with transorming freq to pixels?
    
        
let freqMax = 11025.0
let freqBins = 256.0
let samplingRate = 22050.0

let candidates tb ttd tfr aes =
    let sfr = (boundedInterval tb 500.0 500.0 0.0 freqMax)
    let ss = Seq.filter (fun r -> r.Bottom >< sfr) aes // These are bottom left corners to overlay the template from
    // This is slightly different to matlab where the upper bounds are strictly less than
    let f x = Seq.filter (fun ae -> left ae >==< (left x, left x + ttd) && ae.Bottom >==< (bottom x, bottom x + tfr)) aes
    (ss, Seq.map f ss)

// Length of x and y axis' to scale time and frequency back to
// TODO investigate these formulas - correct for ground parrot template
let pixelAxisLengths ttd tfr = (ttd / (freqBins / samplingRate) |> rnd |> (+) 1.0, tfr / freqMax * (freqBins-1.0) |> rnd |> (+) 1.0)
    
let absLeftAbsBottom rs = (minmap left rs, minmap bottom rs)
    
let templateBounds t =
    let (tl, tb) = absLeftAbsBottom t
    (tl, tb, maxmap right t - tl, maxmap top t - tb)
        
let detectGroundParrots' aes =
    let t = groundParrotTemplate
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
    
let detectGroundParrots aes = seq {for (sae,score) in detectGroundParrots' aes do if score >= 4.0 then yield sae}