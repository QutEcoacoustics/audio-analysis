module QutSensors.AudioAnalysis.AED.Util.Core

(* If the first Option is not empty return it, else return the second.
   Copy of Scala Option.orElse.
*)
let (|?) o p = if Option.isSome o then o else p 

(* If the Option is not empty return its value, otherwise return d.
   Copy of Scala Option.getOrElse
*)
let (|?|) o d = match o with | Some x -> x | _ -> d

// Assume matricies m,n are exactly same dimensions
let matrixMap2 f (m:matrix) (n:matrix) = Math.Matrix.init m.NumRows m.NumCols (fun i j -> f m.[i,j] n.[i,j])

// Assume matricies m,n,o are exactly same dimensions
let matrixMap3 f (m:matrix) (n:matrix) (o:matrix) = Math.Matrix.init m.NumRows m.NumCols (fun i j -> f m.[i,j] n.[i,j] o.[i,j])

let matrixMapi2Unzip f (m:matrix) =
    let r = Math.Matrix.zero m.NumRows m.NumCols
    let s = Math.Matrix.zero m.NumRows m.NumCols
    let mutable x = (0.0, 0.0)
    for i=0 to (m.NumRows-1) do
      for j=0 to (m.NumCols-1) do
        x <- f i j m.[i,j]
        let (y,z) = x
        r.[i,j] <- y
        s.[i,j] <- z
      done
    done
    (r,s) 