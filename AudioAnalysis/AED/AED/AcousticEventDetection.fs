#light
module QutSensors.AudioAnalysis.AED.AcousticEventDetection

open Math.Matrix

// TODO should this return a matrix of int
let toBlackAndWhite t = map (fun e -> if e > t then 1.0 else 0.0)
    
let toFillIn (m:matrix) i j t =
    // TODO right & left should really be generalised to work with sequences? instead of matrix
    let rec right n = if j + n >= m.NumCols or n > t - 2 then 0 else if m.[i,j+n] = 1.0 then n else right (n+1)
    let rec left n  = if j - n < 0          or n > t - 2 then 0 else if m.[i,j-n] = 1.0 then n else left (n+1)
    let l, r = left 1, right 1
    l > 0 && r > 0 && l + r <= t

let joinHorizontalLines m = mapi (fun i j x -> if x = 1.0 or (toFillIn m i j 3) then 1.0 else 0.0) m
    
let joinVerticalLines = transpose << joinHorizontalLines << transpose