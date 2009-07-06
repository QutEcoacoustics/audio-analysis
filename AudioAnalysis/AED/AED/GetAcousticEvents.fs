#light
module QutSensors.AudioAnalysis.AED.GetAcousticEvents

open Math.Matrix
open Set

// TODO can we do p@(i,j) like Haskell?
let rec spider (m:matrix) p (v:(int * int) Set) =
    let (i,j) = p
    if i >= m.NumRows or j < 0 or j >= m.NumCols or i < 0 or i >= m.NumRows or m.[i,j] = 0.0 or v.Contains(p) then v
    // TODO this is an interesting computation, threading the state through a sequence of identical functions
    // else spider m (i+1,j-1) (add p v) |> spider m (i+1,j) |> spider m (i+1,j+1) |> spider m (i,j+1) 
    else List.fold_left (fun z p -> spider m p z) (add p v) [(i-1,j-1);(i-1,j);(i-1,j+1);(i,j-1);(i,j);(i,j+1);(i+1,j-1);(i+1,j);(i+1,j+1)]
    
type Rectangle = {Left:int; Top:int; Width:int; Height:int;}
    
let getAcousticEvents m =
    let m' = copy m
    let g xs = 
        iter (fun (i,j) -> m'.[i,j] <- 0.0) xs // TODO how can we efficiently not mutate?
        let (rs, cs) = List.unzip (to_list xs) 
        {Left=List.min cs; Top=List.min rs; Width=List.max cs - List.min cs + 1; Height=List.max rs - List.min rs + 1} // TODO memoize the mins
    let f i j a x = if x = 0.0 or m'.[i,j] = 0.0 then a else (g(spider m (i,j) Set.empty))::a
    foldi f [] m