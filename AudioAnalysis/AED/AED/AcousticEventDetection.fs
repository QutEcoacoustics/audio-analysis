module QutSensors.AudioAnalysis.AED.AcousticEventDetection

open GetAcousticEvents
open Matlab
open TowseyLib
open Util
open Option

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

let separateLargeEvents aes =
    let areat = 3000
    let freqt = 20.0
    let timet = 100.0 / 3.0
    let f ae =
        let m = aeToMatrix ae
        let s = sumRows m |> Math.Vector.toArray |> Array.map (fun x -> x / (float) m.NumCols * 100.0 <= freqt) 
        let m1 = Math.Matrix.mapi (fun i _ x -> if s.[i] then 0.0 else x) m
        let rs = getAcousticEvents m1
                 |> List.map (fun x -> let b1, b2 = ae.Bounds, x.Bounds in lengthsToRect (left b1 + left b2) (top b1 + top b2) (width b2) (height b2))
        let m2 = m - m1
        rs @ (getAcousticEvents m2
              |> List.filter (fun x -> (float) (height x.Bounds ) * 100.0 / (float) m2.NumRows >= timet)
              |> List.map (fun x -> let b1, b2 = ae.Bounds, x.Bounds in lengthsToRect (left b1 + left b2) (top b1) (width b2) (height b1)))         
    Seq.collect (fun ae -> if area ae.Bounds < areat then [ae.Bounds] else f ae) aes

let smallFirstMin cs h t =
    let s = Seq.pairwise h |> Seq.map (fun (x,y) -> x-y) |> Seq.zip cs
    let tf g = Seq.tryFind (fun (_,x) -> g x) s
    tf ((>) 0) |? lazy tf ((=) 0) |> Option.map fst |?| t

let smallThreshold t rs =
    let (%%) x y = (float x) * y |> round |> (int)
    let cs = seq {for i in 0..9 -> (i * (t %% 0.1)) + (t %% 0.05)}
    let as' = Seq.map area rs |> Seq.filter (fun x -> x <= t)
    smallFirstMin cs (histi as' cs) t

let filterOutSmallEvents t rs =
    let t' = smallThreshold t rs
    Seq.filter (fun r -> area r > t') rs

let detectEventsMatlab intensityThreshold smallAreaThreshold m =
    m
        |> Matlab.wiener2 5  
        |> removeSubbandModeIntensities
        |> toBlackAndWhite intensityThreshold
        |> joinVerticalLines
        |> joinHorizontalLines 
        |> getAcousticEvents
        |> separateLargeEvents
        |> filterOutSmallEvents smallAreaThreshold
    
let detectEventsMinor intensityThreshold smallAreaThreshold (bandPassFilter:float*float) a =
    if (fst bandPassFilter > snd bandPassFilter) then failwith "bandPassFilter args invalid"
    let m = Math.Matrix.ofArray2D a |> mTranspose
    if m.NumRows = 257 
        then
            let (min, max) = fst bandPassFilter |> frequencyToPixels floor, snd bandPassFilter |> frequencyToPixels ceil 
            // remove first row (DC values) like in matlab and remove bandpass pixels (length i really needs that +1!)
            let mPrime = m.Region (1 + min, 0, 1 + max - min, m.NumCols) 
            detectEventsMatlab intensityThreshold smallAreaThreshold mPrime
                // transpose results back & compensate for removing first row & any bandpass
                |> Seq.map (fun r -> cornersToRect r.Left (right r) (r.Top + 1 + min)  (bottom r + 1 + min)) 
                
        else 
            failwith (sprintf "Expecting matrix with 257 frequency cols, but got %d" m.NumRows)

let detectEvents intensityThreshold smallAreaThreshold (bandPassFilter:float*float) a =
    detectEventsMinor intensityThreshold smallAreaThreshold bandPassFilter a
    |> Seq.map (fun r -> new Oblong(r.Left, r.Top, right r, bottom r )) 