module QutSensors.AudioAnalysis.AED.AcousticEventDetection

open GetAcousticEvents
open Matlab
open TowseyLibrary
open Util
open Option
open Default
open System.Drawing

let frequencyToPixels rndFunc maxPixels maxFreq freq = int (rndFunc (((float maxPixels) * freq) / maxFreq))

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

let separateLargeEvents serparateStyle aes =
    let maskOverExtendedBridgedElement p originalAe bae =
        let oL, oR, oT, oB = left originalAe.Bounds, right originalAe.Bounds, top originalAe.Bounds, bottom originalAe.Bounds
        // map results back to absolute co-ordinates
        let absoluteEvent = mapEventToAbsolute (oL, oT) bae
        let absBounds, absElements = absoluteEvent.Bounds, absoluteEvent.Elements
        let l,r,w,t,b,h = left absBounds, right absBounds, width absBounds, top absBounds, bottom absBounds, height absBounds

        if p.ExtrapolateBridgeEvents then
            // reset the bounds to full height XOR width of parent event (matches old behaviour, we want overlapping events)
            let extendedBounds, sectionA, sectionB =
                match serparateStyle with
                | Horizontal _ -> 
                    {absBounds with Top = oT; Bottom = oB}, 
                    lengthsToRect l oT w (t - oT), // above
                    lengthsToRect l b w (oB - b + 1) // below
                | Vertical _ -> 
                    {absBounds with Left = oL; Right = oR},
                    lengthsToRect oL t (l - oL) h, // left
                    lengthsToRect r t (oR - r + 1) h // right
                | _ -> failwith "Invalud separateStyle"
            
            // add the hit elements from the extended bounds
            let inline f r (y, x) =  isWithin r (x, y)
            let sectionAHits = Set.filter (f sectionA) originalAe.Elements
            let sectionBHits = Set.filter (f sectionB) originalAe.Elements

            let hits = sectionAHits + absElements + sectionBHits            
            elementsToAcousticEvent hits         
        else
            {Bounds = absBounds; Elements = absElements}
    let blackoutNarrowSections summer getMax threshold indexChooser m =
        // measure widths
        let chopIndexes = summer m |> Math.Vector.toArray |> Array.map (fun x -> percent (x / (m |> getMax |> float))  <= threshold)
        // actually chop the event in half at the thin points
        Math.Matrix.mapi (fun i j x -> if  (i, j) |> indexChooser |> Array.get chopIndexes then 0.0 else x) m
    let separate parameters ae =
        let m = aeToMatrix ae
        let m1 = match serparateStyle with
                 | Horizontal _ -> blackoutNarrowSections sumRows (fun m -> m.NumCols) parameters.MainThreshold fst m
                 | Vertical _ -> blackoutNarrowSections sumColumns (fun m -> m.NumRows) parameters.MainThreshold snd m
                 | _ -> failwith "Invalud separateStyle"

        // scan for new acoustic events (returned events have relative co-ordinates)
        let splitEvents = 
            getAcousticEvents m1
            // map results back to absolute co-ordinates
            |> List.map (mapEventToAbsolute ((left ae.Bounds), (top ae.Bounds)))
        // check for events in the sections that were cut out (negative mask)
        let m2 = m - m1
        let bridgingEvents = 
            getAcousticEvents m2
            // filter out briding events that too small... i.e. less the x% of the original event's height/width
            |> List.filter (fun x -> percent ((x.Bounds |> height |> float) / (float m2.NumRows)) >= parameters.OrthogonalThreshold)
            |> List.map (maskOverExtendedBridgedElement parameters ae)

        // NOTE: no longer returns rectangles only (fixes coordiantes and returns full acoustic event)
        splitEvents @ bridgingEvents

    match serparateStyle with
    | Horizontal p | Vertical p -> Seq.collect (fun ae -> if areaUnits ae.Bounds < p.AreaThreshold then [ae] else separate p ae) aes
    | Skip -> aes

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

let detectEventsMatlab (options: AedOptions) m =
    m
        |?> (options.DoNoiseRemoval, Matlab.wiener2 5)  
        |?> (options.DoNoiseRemoval, removeSubbandModeIntensities)
        |> toBlackAndWhite options.IntensityThreshold
        |> joinVerticalLines
        |> joinHorizontalLines 
        |> getAcousticEvents
        |> separateLargeEvents options.LargeAreaHorizontal
        |> separateLargeEvents options.LargeAreaVeritical
        |> filterOutSmallEvents options.SmallAreaThreshold
    
let detectEventsMinor (options: AedOptions) a =
    let m = Math.Matrix.ofArray2D a |> mTranspose
    
    // remove first row (DC values) like in matlab and remove bandpass pixels (length i really needs that +1!)
    let dc, actualRows = 
        match m.NumRows with
        | 257 | 513 | 1025 | 2049 -> 1, m.NumRows - 1
        | 256 | 512 | 1024 | 2048 -> 0, m.NumRows
        | _ -> failwith (sprintf "Expecting matrix with 256, 51, 1024, or 2048 frequency cols, but got %d" m.NumRows)

    let min, max = 
        match options.BandPassFilter with
        | Some (low, high) ->
            if (low > high) then failwith "bandPassFilter args invalid"
            low |> frequencyToPixels floor actualRows options.NyquistFrequency, high |> frequencyToPixels ceil actualRows options.NyquistFrequency
        | None -> (0, actualRows - 1) // -1 for zero indexing

    let mPrime =  m.Region (dc + min, 0, dc + max - min, m.NumCols) 

    detectEventsMatlab options mPrime    
    // transpose results back & compensate for removing first row & any bandpass
    |> Seq.map (mapEventToAbsolute (0, dc + min))
 

let detectEvents options a =
    detectEventsMinor options a
    |> Seq.map (fun ae ->
        let points = new System.Collections.Generic.HashSet<Point>(Seq.map toPoint2 ae.Elements)
        new Oblong(ae.Bounds.Left, ae.Bounds.Top, right ae.Bounds, bottom ae.Bounds, points))
    