module QutSensors.AudioAnalysis.AED.AcousticEventDetection

open GetAcousticEvents
open Matlab
open TowseyLib
open Util

// TODO should this return a matrix of int
let toBlackAndWhite t = Math.Matrix.map (fun e -> if e > t then 1.0 else 0.0)
    
let toFillIn (m:matrix) i j t =
    // TODO right & left should really be generalised to work with sequences? instead of matrix
    let rec right n = if j + n >= m.NumCols or n > t - 2 then 0 else if m.[i,j+n] = 1.0 then n else right (n+1)
    let rec left n  = if j - n < 0          or n > t - 2 then 0 else if m.[i,j-n] = 1.0 then n else left (n+1)
    let l, r = left 1, right 1
    l > 0 && r > 0 && l + r <= t

let joinHorizontalLines m = Math.Matrix.mapi (fun i j x -> if x = 1.0 or (toFillIn m i j 3) then 1.0 else 0.0) m
    
let joinVerticalLines = Math.Matrix.transpose << joinHorizontalLines << Math.Matrix.transpose

let smallFirstMin cs h t =
    let s = Seq.pairwise h |> Seq.map (fun (x,y) -> x-y) |> Seq.zip cs // TODO almost a copy from LargeEvents.lastMin
    let tf g = Seq.tryFind (fun (_,x) -> g x) s
    tf ((>) 0) |? tf ((=) 0) |> Option.map fst |?| t

let smallThreshold rs =
    let t = 200
    let cs = seq {for i in 0..9 -> (i * 20) + 10}
    let as' = areas rs |> Seq.filter (fun x -> x <= t)
    smallFirstMin cs (hist as' cs) t

let filterOutSmallEvents rs =
    let t = smallThreshold rs
    Seq.filter (fun r -> area r > t) rs

let detectEventsMatlab m =
    Matlab.wiener2 5 m 
    |> SubbandMode.removeSubbandModeIntensities2
    |> toBlackAndWhite 9.0
    |> joinVerticalLines
    |> joinHorizontalLines
    |> getAcousticEvents
    |> filterOutSmallEvents
    
let detectEvents a =
    Math.Matrix.of_array2 a |> Math.Matrix.transpose |> detectEventsMatlab
                            |> Seq.map (fun r -> new Oblong(r.Left, r.Top, right r, bottom r)) // transpose results back