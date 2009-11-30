module QutSensors.AudioAnalysis.AED.SpectralPeakTrack    

open Matlab

(* TODO idea for generalisation
let byCol f (m:matrix) = 
    let m' = Math.Matrix.zero m.NumRows m.NumCols
    let g j i e = m'.[i,j] <- e
    for j=0 to m.NumCols do
        m.Column j |> f (g j)
    done
    m'
    
let verticalPeaks' m t =
    let f g v =
        let a = Math.Vector.toArray v
        smooth a 3 |> findPeaks |> List.iter (fun i -> if a.[i] > t then g i a.[i])
    byCol f m
*)

let verticalPeaks t (m:matrix) =
    let m' = Math.Matrix.zero m.NumRows m.NumCols
    for j=0 to (m.NumCols-1) do
        let a = m.Column j |> Math.Vector.toArray
        let s = smooth a 3 // TODO flip args here?
        findPeaks s |> List.iter (fun i -> if s.[i] > t then m'.[i,j] <- s.[i])
    done
    m'

