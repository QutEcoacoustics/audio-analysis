module QutSensors.AudioAnalysis.AED.SpectralPeakTrack    

open Matlab
open Util

let verticalPeaks t (m:matrix) =
    let f c =
        let s = Math.Vector.toArray c |> smooth 3
        (s, findPeaks s |> List.filter (fun i -> s.[i] > t) |> Set.ofList)
    mapByCol f m
   
let indexMaxNeighbour i ((a:float array), ni)  =
    if Set.isEmpty ni then None else 
        let s = Set.intersect ni (Set.ofSeq (seq{i-2..i+2}))
        if Set.isEmpty s then None else
            Set.map (fun e -> (e, a.[e])) s |> Set.fold (fun (zi,zm) (i,m) -> if m > zm then (i,m) else (zi,zm)) (0,0.0) |> fst |> Some
            // TODO check if this pattern of finding the max of one side of a tuple is already implemented

let horizontalTracks ps =
    let g (next, thisRes, nextRes) i =
        indexMaxNeighbour i next |> Option.fold (fun (nx,tr,nr) j -> (nx, Set.add i tr, Set.add j nr)) (next, thisRes, nextRes)
                                        
    let f (prevRes, r) ((_, tp), next) = let (_, tr, nr) = Set.fold g (next, prevRes, Set.empty) tp in (nr, tr::r)
        
    let (last, r) = Seq.fold f (Set.empty, []) (Seq.pairwise ps)
    last::r |> List.rev

let colIndicesToMatrix m n (ps:int Set seq) =
        let m = Math.Matrix.zero m n
        Seq.iteri (fun j -> Set.iter (fun i -> m.[i,j] <- 1.0)) ps
        m
    
let allPeaks (m:matrix) =
    let f = horizontalTracks << verticalPeaks 9.0
    colIndicesToMatrix m.NumRows m.NumCols (f m) + (mTranspose m |> f |> colIndicesToMatrix m.NumCols m.NumRows |> mTranspose)
    
let peakTracks m =
    let ps = allPeaks m
    ()