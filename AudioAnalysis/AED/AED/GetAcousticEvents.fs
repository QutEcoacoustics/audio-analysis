module QutSensors.AudioAnalysis.AED.GetAcousticEvents

open Util

// TODO can we do p@(i,j) like Haskell?
let rec spider (m:matrix) xs (v:(int * int) Set) =
    match xs with
    | []    -> v
    | p::ps -> let (i,j) = p // TODO should this be part of the pattern?
               let (v', ps') = if j < 0 || j >= m.NumCols || i < 0 || i >= m.NumRows || m.[i,j] = 0.0 || v.Contains(p) then (v, ps)
                               // TODO this is an interesting computation, threading the state through a sequence of identical functions
                               // else spider m (i+1,j-1) (add p v) |> spider m (i+1,j) |> spider m (i+1,j+1) |> spider m (i,j+1) 
                               else (Set.add p v, [(i-1,j-1);(i-1,j);(i-1,j+1);(i,j-1);(i,j+1);(i+1,j-1);(i+1,j);(i+1,j+1)] @ ps) 
               spider m ps' v'
    
type AcousticEvent = {Bounds:int Rectangle; Elements:(int * int) Set}
let bounds aes = Seq.map (fun ae -> ae.Bounds) aes
    
let getAcousticEvents m =
    let m' = Math.Matrix.copy m
    let g xs = 
        Set.iter (fun (i,j) -> m'.[i,j] <- 0.0) xs // TODO how can we efficiently not mutate?
        let (rs, cs) = List.unzip (Set.toList xs) 
        let l,t = List.min cs, List.min rs
        {Bounds=lengthsToRect l t (List.max cs - l + 1) (List.max rs - t + 1); Elements=xs}
    let f i j a x = if x = 0.0 || m'.[i,j] = 0.0 then a else (g(spider m [(i,j)] Set.empty))::a
    Math.Matrix.foldi f [] m