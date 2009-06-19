#light
module QutSensors.AudioAnalysis.AED.GetAcousticEvents

open Set

// TODO can we do p@(i,j) like Haskell?
let rec spider (m:matrix) p (v:(int * int) Set) =
    let (i,j) = p
    if i >= m.NumRows or j < 0 or j >= m.NumCols or m.[i,j] = 0.0 or v.Contains(p) then v
    else spider m (i,j-1) (add p v) |> spider m (i+1,j-1) |> spider m (i+1,j) |> spider m (i+1,j+1) |> spider m (i,j+1) 