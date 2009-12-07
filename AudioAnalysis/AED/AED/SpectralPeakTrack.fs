module QutSensors.AudioAnalysis.AED.SpectralPeakTrack    

open GetAcousticEvents
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
    
let allPeaks t (m:matrix) =
    let f = horizontalTracks << verticalPeaks t
    colIndicesToMatrix m.NumRows m.NumCols (f m) + (mTranspose m |> f |> colIndicesToMatrix m.NumCols m.NumRows |> mTranspose)
    
let removeSmall m =
    let aes = getAcousticEvents m |> List.filter (fun ae -> ae.Elements.Count < 15)
    let m' = Math.Matrix.copy m
    List.iter (fun ae -> Set.iter (fun (i,j) -> m'.[i,j] <- 0.0) ae.Elements) aes
    m'
    
let dilate t (m:matrix) ps =
    let m' = Math.Matrix.zero m.NumRows m.NumCols
    let f i j x =
        if x = 0.0 then () else
            let (si, sj, li, lj) = neighbourhoodBounds 3 (m.NumRows) (m.NumCols) i j
            for ic = si to (si + li - 1) do
                for jc = sj to (sj + lj - 1) do
                    if m.[ic,jc] >= t then m'.[ic,jc] <- m.[ic,jc] else ()
                done
            done
    Array2D.iteri f (Math.Matrix.toArray2D ps)
    m'
    
let spt t a = let m = Math.Matrix.ofArray2D a in allPeaks t m |> removeSmall |> dilate t m |> Math.Matrix.toArray2D
