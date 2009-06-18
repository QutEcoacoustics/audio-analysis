#light
module QutSensors.AudioAnalysis.AED.AcousticEventDetection

open Math.Matrix

let toBlackAndWhite t = map (fun e -> if e > t then 1.0 else 0.0)

let findRowDist (m:matrix) i j t = 
    let mutable c = j + 1
    let mutable n = 0
    while (c < m.NumCols && c < j + t && m.[i,c] = 0.0) do
        n <- n + 1
        c <- c + 1
    if n = (min (m.NumCols - j - 1) (t - 1)) then 0 else n
    
let joinHorizontalLines m =
    let m' = copy m 
    let g x i j =
        set x i j 1.0
        x
    let f i j (m',n) e = if n > 0 then (g m' i j, n-1)
                                  else if e = 0.0 then (m',n)
                                       else (m', findRowDist m' i j 3)
    let (_,_) = foldi f (m',0) m
    m'
    
let joinVerticalLines = transpose << joinHorizontalLines << transpose