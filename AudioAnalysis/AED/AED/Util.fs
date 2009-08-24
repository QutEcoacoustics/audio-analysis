module QutSensors.AudioAnalysis.AED.Util

// If the first Option is not empty return it, else return the second. Copy of Scala Option.orElse.
let orElse o (p:'a option Lazy) = if Option.isSome o then o else p.Force()

let (|?) = orElse

let (|?|) = defaultArg

// TODO: should I/can I fix the overloaded round instead?
let rnd x = if x - 0.5 = floor x then ceil x else round x

let sumRows (m:matrix) = Math.Matrix.foldByRow (+) (Math.Vector.zero m.NumRows) m

let array2Dfold f z (a:'a[,]) =
     let mutable x = z
     for i=0 to (a.GetLength(0)-1) do
       for j=0 to (a.GetLength(1)-1) do
         x <- f x (a.[i,j])
       done
     done
     x

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
    
(* This is currently done the easy, inefficient way.

   The following Matlab code will write the matrix I1 to the file I1.txt, with one element per line
   by descending each column in turn.
   
    fid = fopen('I1.txt', 'wt');
    fprintf(fid, '%f\n', I1);
    fclose(fid);
 *)
let fileToMatrix f r c =
    let ls = System.IO.File.ReadAllLines f
    let a = Array2D.create r c 0.0
    Array.iteri (fun i (s:string) -> a.[i % r, i / r] <- System.Convert.ToDouble(s)) ls
    a