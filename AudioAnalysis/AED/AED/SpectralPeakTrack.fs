module QutSensors.AudioAnalysis.AED.SpectralPeakTrack    

open Matlab

// TODO move to Util
let mapByCol f (m:matrix) = seq{m.NumCols-1..-1..0} |> Seq.fold (fun z j -> f (m.Column j) ::z) [] |> Math.Vector.Generic.ofList

let verticalPeaks t (m:matrix) =
    let f c =
        let s = Math.Vector.toArray c |> smooth 3
        (s, findPeaks s |> List.filter (fun i -> s.[i] > t) |> Set.ofList)
    mapByCol f m
   
let indexMaxNeighbour i ni (m:matrix) nj =
    if Set.isEmpty ni then None else 
        let s = Set.intersect ni (Set.ofSeq (seq{i-2..i+2}))
        if Set.isEmpty s then None else
            Set.map (fun e -> (e, m.[e,nj])) s |> Set.fold (fun (zi,zm) (i,m) -> if m > zm then (i,m) else (zi,zm)) (0,0.0) |> fst |> Some
            // TODO check if this pattern of finding the max of one side of a tuple is already implemented

// TODO modify this to be: verticalTracks ps:float [] * int Set Vector
let verticalTracks m (ps:int Set List) =
    let f (nj, prevRes, r) (this, next) =
        let g (thisRes, nextRes) i = indexMaxNeighbour i next m nj
                                        |> Option.fold (fun (tr,nr) j -> (Set.add i tr, Set.add j nr)) (thisRes, nextRes)
        let (tr,nr) = Set.fold g (prevRes, Set.empty) this
        (nj+1, nr, tr::r)
    let (_, last, r) = Seq.fold f (1, Set.empty, []) (Seq.pairwise ps)
    last::r |> List.rev

let colIndicesToMatrix m n (ps:int Set seq) =
        let m = Math.Matrix.zero m n
        Seq.iteri (fun j -> Set.iter (fun i -> m.[i,j] <- 1.0)) ps
        m
        
let peakTracks m =
    let vp = verticalPeaks 9.0 m
    //let vt = verticalTracks vp
    ()