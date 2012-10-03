
module QutSensors.AudioAnalysis.AED.EventPatternRecog

open Microsoft.FSharp.Math.SI

open Util

type score = float

/// current EPR is currently hardwirded to use only the groundParrotTemplate
/// for detection
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

module EprInternals =

    /// function: indexMinMap f xs :  ('a -> 'b) -> seq<'a> -> int
    /// Runs f over sequence xs, then returns index of lowest value returned from f.
    let indexMinMap f xs =
        let ys = Seq.map f xs
        let m = Seq.min ys
        Seq.findIndex (fun y -> y = m) ys

    ///function: normaliseTimeFreq starttime startfreq timeDifferance frequencyRange normalisedTime normalisedFrequency (timeToNormalise, frequencyToNormalise)? : float -> float -> float -> float -> float -> float ->  (float * float) -> (float * float) 
    /// Normalises a given time/frequency tuple.
    /// ---
    ///	function g x start differance ?length? : float -> float -> float -> float -> float 
    ///		let x' =  round( x - s / d * l)  // get rid of start from x, don't get d*l???
    ///			in if x' < 1.0 then
    ///					1.0
    ///				else if x' > l then
    ///					l
    ///				else 
    ///					x'
    ///		// then, apply g twice in tuple, once each for time and frequency
    ///		(g t startTime timedimensions nt, g f startfrequency frequencyrange nf) 
    let normaliseTimeFreq (st:float<s>) (sf:float<Hz>) (td:float<s>) (fr:float<Hz>) (nt:pxf) (nf:pxf) ((t:float<s>),(f:float<Hz>)) =    
        let rnd l v = let x' = (round v) * px in if x' < px then px else if x' > l then l else x'
        let g x s d l = (x - s) / d * (float l) |> rnd l
        let h x s d l = (x - s) / d * (float l) |> rnd l
        (g t st td nt, h f sf fr nf)

    /// calculate centre point from rect    
    let centroids (rs:seq<EventRect>)  =
        let centre s (l:float<_>) = s + (l / 2.0)
        Seq.map (fun r -> (centre (left r) (width r), centre (bottom r) (height r))) rs
    
    // TODO investigate performance optimisation by normalising individual points in tuple computations
    // TODO same result  if move normaliszaton earlier?
    /// function centroidsBottomLefts startTime startFrequency timeDifference frequencyRange normalisedTime normalisedFrequency rectangles
    /// Returns a Seq of centroids, tupled with a Seq of bottom-lefts both obtained from rs.
    /// Both are results are normalised (i.e. they are pixels).
    /// ---
    /// return tuple
    ///     apply centroids func to rs |> then normalise results?
    ///     apply bottomLeft func to each element to rs |> normalize results?
    let centroidsBottomLefts st sf td fr nt nf (rs:seq<EventRect>) = 
        let f = Seq.map (normaliseTimeFreq st sf td fr nt nf)
        (centroids rs |> f, Seq.map bottomLeft rs |> f)

    /// Returns distance between two points
    let inline euclidianDist (x1, y1) (x2, y2) = 
        sqr (x1 - x2)  + sqr (y1 - y2) |> sqrt

    /// function overlap (templateLeft, templateBottom) (templateCentroidTime, templateCentroidFrequency) (left, bottom) (centroidLeft, centroidFrequency)
    /// Calculates an overlap. Returns a dimensionless float    
    let overlap (tl, tb) (tct, tcf) (l, b) (ct, cf) : score  =
        let tr = tl + (tct - tl) * 2.0<_>
        let tt = tb + (tcf - tb) * 2.0<_>
        let r = l + (ct - l) * 2.0<_>
        let t = b + (cf - b) * 2.0<_>
        let ol, or', ob, ot = max tl l, min tr r, max tb b, min tt t
        if or' < ol || ot < ob then 0.0 else let oa = (or'-ol) * (ot-ob) in 0.5 * (oa/((tr-tl)*(tt-tb)) + oa/((r-l)*(t-b)))
    
    let freqMax = 11025.0<Hz>
    let freqBins = 256.0
    let samplingRate = 22050.0<Hz>

    let candidates tb ttd tfr aes =
        let sfr = (boundedInterval tb 500.0<Hz> 500.0<Hz> 0.0<Hz> freqMax)
        let ss = Seq.filter (fun r -> r.Bottom >< sfr) aes // These are bottom left corners to overlay the template from
        let f x = Seq.filter (fun ae -> left ae >= left x && left ae < left x + ttd && bottom ae >= bottom x && bottom ae < bottom x + tfr) aes
        (ss, Seq.map f ss)

    // TODO: investigate these formulas - correct for ground parrot template
    // NOTE: this does not take into account overlap in fft, sampling rate is wrong
    /// function pixelAxisLengths templateimeDifference templateFrequency: float -> float -> (float * float)
    /// Length of x and y axis' to scale time and frequency back to.
    /// Returns the number of pixels occupied by the specified time/frequency domain and
    /// ---
    /// return tuple of 
    ///     templateimeDifference / (freqBins / samplingRate),   // e.g. 120 / (256 / 22050) = 10335 |> 10336
    ///     templateFrequency / freqMax * (freqBins - 1.0)  //e.g. 11025 / 22050 * (255) = 0.0??
    let pixelAxisLengths (ttd:float<s>) (tfr:float<Hz>) = 
        (
            ttd / (freqBins / samplingRate) |> round |> (+) 1.0 |> (*) 1.0<px>, 
            (tfr / freqMax) * (freqBins - 1.0) |> round |> (+) 1.0 |> (*) 1.0<px>
        )

    /// function: absLeftAbsBottom rectangleSequence : Seq<a' Rectangle> -> a' * a'
    /// returns the most leftest and bottomest point from a seq<'a rectangle>
    /// ---
    ///	 return tuple of
    ///		(minmap left rs, minmap bottom rs) // min map applies function to each rect to select dimension to use    
    let absLeftAbsBottom rs = (minmap left rs, minmap bottom rs)

    let absRightAbsTop rs = (maxmap right rs, maxmap top rs)
    
    /// function: templateBounds template
    /// Returns the template's most left point, most bottom point, it's total width, and total height
    /// ---
    ///	 let (templateLeft, templateBottom) = ...
    ///  let (templateRight, templateTop) = ...
    ///	 return tuple
    ///	 	(templateLeft, templateBottom, width-of-template, height-of-template)
    let templateBounds t =
        let (tl, tb) = absLeftAbsBottom t
        let (tr, tt) = absRightAbsTop t
        (tl, tb, tr - tl, tt - tb)

    /// function scoreEvents template acousticEvents
    /// ScoreEvents uses a template to score a list of acoustic events, 
    /// A list of candidates tupled with scores is returned.
    /// ---
    /// let (templateLeft, templateBottom, templateTimeDifference, templateFrequnecyRange) = ...
    /// let (xLength, yLength) = ... // number of pixels
    /// let ((templateCentroids:Seq<float * float>), (templateBottomLefts:Seq<float * float>)) = ... // values are normalised!
    /// function score restangles =
    ///     let (startTime, startFrequency) = ... // of the candidates
    ///     let (centroids, bottomLefts) = ... // normalised results!
    ///         function f templateCentroid templateBottomLeft
    ///             let index = given the centroids, find the distance between each point and given templateCentroid
    ///                             then, select the index of the element with the shortest distance
    ///             return overlap templateBottomLeft templateCentroid closest-Bottom-Left closest-Centroid
    ///     Seq.map2 f templateCentroids templateBottomLefts // from parent function, map2 == normal map, but with two lists pair-wise
    ///         |> return sum of results
    ///
    /// let (?saes?, candidates) = candidates templateBottom templateTimeDimensions templateFrequencyRange acousticEventSequence
    /// Seq.zip saes (Seq.map score cs) // ACTUAL SCORING, pairs an ?acoustic event? with a ?candidate?
    let scoreEvents (t:seq<EventRect>) (aes:seq<EventRect>) =
        let (tl, tb, ttd, tfr) = templateBounds t
        let (xl, yl) = pixelAxisLengths ttd tfr
        let (tcs, tbls) = centroidsBottomLefts tl tb ttd tfr xl yl t
        
        let score rs =
            let (st, sf) = absLeftAbsBottom rs
            let (cs, bls) = centroidsBottomLefts st sf ttd tfr xl yl rs // pixels
            let g candidateCentroid candidateBottomLeft= 
                let  indexOfTemplate = indexMinMap (euclidianDist candidateCentroid) tcs
                overlap (Seq.nth indexOfTemplate tbls) (Seq.nth indexOfTemplate tcs) candidateBottomLeft candidateCentroid
            Seq.map2 g cs bls |> Seq.sum
        
        let (saes, cs) = candidates tb ttd tfr aes // cs are the groups of acoustic events that are candiates for template matching

        Seq.zip saes (Seq.map score cs)

    /// function: detect template minScore acousticEventsSequence
    /// Generic EPR detection function. Accepts a template for comparison, and a minimum score used as a threshold
    /// ---
    /// add dimensions to input
    /// let total = get the number of rects in template
    /// next, scoreEvents is run. the results are a seq<float Rectangle * float> (rects with score)
    ///     a loop is run over the results
    ///         any Rect whise score is greater then min result,
    ///             has its value and a ?normalised? score yielded            
    let detect template minScore aes = 
        let convert r = addDimensions 1.0<s> 1.0<Hz> r

        let t' = Seq.map convert template
        let aes' = Seq.map convert aes

        let total = float (List.length template)
        seq {
            for (sae,s) in scoreEvents t' aes' do 
                if s >= minScore then yield (sae, s / total)
            }

//---------------------------------------------------------------------------//

let unnormalise template (normalisedMinScore:float) =  float (List.length template) * normalisedMinScore

/// function: detectGroundParrots acousticEventsSequence   
let DetectGroundParrots aes normalisedMinScore = EprInternals.detect groundParrotTemplate (unnormalise groundParrotTemplate normalisedMinScore) aes // 4.0
