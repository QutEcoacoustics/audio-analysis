module QutSensors.AudioAnalysis.AED.AcousticEventDetection

open GetAcousticEvents
open Matlab
open TowseyLibrary
open Util
open Option
open System.Drawing

let frequencyToPixels rndFunc f = int (rndFunc ((255.0 * f) / Default.freqMax))

let removeSubbandModeIntensities (m:matrix) =
    let modes =
        let f r =
            let mn, mx = Seq.min r, Seq.max r
            let hs = histf r (seq{mn .. mx})
            let mi = Seq.findIndex ((=) (Seq.max hs)) hs
            let mode = mn + (float mi)
            let t = (mn - mx) / 2.0
            if mode > t then t else mode
        mapByRow f m
    let sModes = smooth 11 (Math.Vector.Generic.toArray modes)
    Math.Matrix.mapi (fun r _ x -> x - sModes.[r]) m

// TODO should this return a matrix of int
let toBlackAndWhite t = Math.Matrix.map (fun e -> if e > t then 1.0 else 0.0)
    
let toFillIn (m:matrix) i j t =
    // TODO right & left should really be generalised to work with sequences? instead of matrix
    let rec right n = if j + n >= m.NumCols || n > t - 2 then 0 else if m.[i,j+n] = 1.0 then n else right (n+1)
    let rec left n  = if j - n < 0          || n > t - 2 then 0 else if m.[i,j-n] = 1.0 then n else left (n+1)
    let l, r = left 1, right 1
    l > 0 && r > 0 && l + r <= t

let joinHorizontalLines m = Math.Matrix.mapi (fun i j x -> if x = 1.0 || (toFillIn m i j 3) then 1.0 else 0.0) m
    
let joinVerticalLines = mTranspose << joinHorizontalLines << mTranspose
    
let aeToMatrix ae =
    let r = ae.Bounds
    let m = Math.Matrix.zero (height r) (width r)
    Set.iter (fun (i,j) -> m.[i-(top r), j-(left r)] <- 1.0) ae.Elements
    m

let mapEventToAbsolute (absLeft, absTop) event =
    {
        Bounds =  lengthsToRect (absLeft + left event.Bounds) (absTop + top event.Bounds) (width event.Bounds) (height event.Bounds);
        Elements = Set.map (fun (i,j) -> absTop + i, absLeft + j) event.Elements;
    }

let separateLargeEvents aes =
    let areat = 3000
    let freqt = 20.0
    let timet = 100.0 / 3.0
    let maskOverExtendedBridgedElement originalAe bae =
        let oL, oT, oB = left originalAe.Bounds, top originalAe.Bounds, bottom originalAe.Bounds
        // map results back to absolute co-ordinates
        let absoluteEvent = mapEventToAbsolute (oL, oT) bae
        let absBounds, absElements = absoluteEvent.Bounds, absoluteEvent.Elements
        let l,w,t,b = left absBounds, width absBounds, top absBounds, bottom absBounds
        // reset the bounds to full height of parent event (matches old behaviour, we want overlapping events)
        let extendedBounds = {absBounds with Top = oT; Bottom = oB}
        // add the hit elements from the extended bounds
        let inline f r (y, x) =  isWithin r (x, y)
        let aboveRect = lengthsToRect l oT w (t - oT)
        let belowRect = lengthsToRect l b w (oB - b + 1)
        let aboveHits = Set.filter (f aboveRect) originalAe.Elements
        let belowHits = Set.filter (f belowRect) originalAe.Elements

        let hits = aboveHits + absElements + belowHits
        {Bounds = extendedBounds; Elements = hits}
    let separate ae =
        let m = aeToMatrix ae
        // measure widths
        let s = sumRows m |> Math.Vector.toArray |> Array.map (fun x -> x / (float) m.NumCols * 100.0 <= freqt)
        // actually chop the event in half at the thin points
        let m1 = Math.Matrix.mapi (fun i _ x -> if s.[i] then 0.0 else x) m
        // scan for new acoustic events (returned events have relative co-ordinates)
        let splitEvents = 
            getAcousticEvents m1
            // map results back to absolute co-ordinates
            |> List.map (mapEventToAbsolute ((left ae.Bounds), (top ae.Bounds)))
        // check for events in the sections that were cut out (negative mask)
        let m2 = m - m1
        let bridgingEvents = 
            getAcousticEvents m2
            |> List.filter (fun x -> (float) (height x.Bounds ) * 100.0 / (float) m2.NumRows >= timet)
            |> List.map (maskOverExtendedBridgedElement ae)

        // NOTE: no longer returns rectangles only (fixes coordiantes and returns full acoustic event)
        splitEvents @ bridgingEvents
    Seq.collect (fun ae -> if area ae.Bounds < areat then [ae] else separate ae) aes

let smallFirstMin cs h t =
    let s = Seq.pairwise h |> Seq.map (fun (x,y) -> x-y) |> Seq.zip cs
    let tf g = Seq.tryFind (fun (_,x) -> g x) s
    tf ((>) 0) |? lazy tf ((=) 0) |> Option.map fst |?| t

let smallThreshold t aes =
    let (%%) x y = (float x) * y |> round |> (int)
    let cs = seq {for i in 0..9 -> (i * (t %% 0.1)) + (t %% 0.05)}
    let as' = Seq.map (fun ae -> area  ae.Bounds) aes |> Seq.filter (fun x -> x <= t)
    smallFirstMin cs (histi as' cs) t

let filterOutSmallEvents t aes =
    let t' = smallThreshold t aes
    Seq.filter (fun ae -> area (ae.Bounds) > t') aes

let detectEventsMatlab intensityThreshold smallAreaThreshold doNoiseRemoval m =
    m
        |?> (doNoiseRemoval, Matlab.wiener2 5)  
        |?> (doNoiseRemoval, removeSubbandModeIntensities)
        |> toBlackAndWhite intensityThreshold
        |> joinVerticalLines
        |> joinHorizontalLines 
        |> getAcousticEvents
        |> separateLargeEvents
        |> filterOutSmallEvents smallAreaThreshold
    
let detectEventsMinor intensityThreshold smallAreaThreshold (bandPassFilter:float*float) doNoiseRemoval a =
    if (fst bandPassFilter > snd bandPassFilter) then failwith "bandPassFilter args invalid"
    let m = Math.Matrix.ofArray2D a |> mTranspose
    if m.NumRows = 257 
        then
            let (min, max) = fst bandPassFilter |> frequencyToPixels floor, snd bandPassFilter |> frequencyToPixels ceil 
            // remove first row (DC values) like in matlab and remove bandpass pixels (length i really needs that +1!)
            let mPrime = m.Region (1 + min, 0, 1 + max - min, m.NumCols) 
            detectEventsMatlab intensityThreshold smallAreaThreshold doNoiseRemoval mPrime    
            // transpose results back & compensate for removing first row & any bandpass
            |> Seq.map (mapEventToAbsolute (0, min + 1))
        else 
            failwith (sprintf "Expecting matrix with 257 frequency cols, but got %d" m.NumRows)

let detectEvents intensityThreshold smallAreaThreshold bandPassFilter doNoiseRemoval a =
    detectEventsMinor intensityThreshold smallAreaThreshold bandPassFilter doNoiseRemoval a
    |> Seq.map (fun ae ->
        let points = new System.Collections.Generic.HashSet<Point>(Seq.map toPoint ae.Elements)
        new Oblong(ae.Bounds.Left, ae.Bounds.Top, right ae.Bounds, bottom ae.Bounds, points))
    