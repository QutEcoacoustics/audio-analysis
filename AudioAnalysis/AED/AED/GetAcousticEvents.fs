#light
module QutSensors.AudioAnalysis.AED.GetAcousticEvents

open Math.Matrix
open Set

// TODO can we do p@(i,j) like Haskell?
let rec spider (m:matrix) p (v:(int * int) Set) =
    let (i,j) = p
    if i >= m.NumRows or j < 0 or j >= m.NumCols or m.[i,j] = 0.0 or v.Contains(p) then v
    else spider m (i+1,j-1) (add p v) |> spider m (i+1,j) |> spider m (i+1,j+1) |> spider m (i,j+1) 
    
let getAcousticEvents m =
    let m' = copy m
    let g xs = 
        iter (fun (i,j) -> m'.[i,j] <- 0.0) xs
        let (rs, cs) = List.unzip (to_list xs)
        (List.min cs, List.min rs)
    let f i j a x = if x = 0.0 or m'.[i,j] = 0.0 then a else (g(spider m (i,j) Set.empty))::a
    foldi f [] m