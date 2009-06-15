#light

module Util.Array2

open System
open System.IO

let a2fold f z (a:'a[,]) =
     let mutable x = z
     for i=0 to (a.GetLength(0)-1) do
       for j=0 to (a.GetLength(1)-1) do
         x <- f x (a.[i,j])
       done
     done
     x
     
// TODO z should be an array of initial accumulator values - one for each row
// TODO would have been more elegant to just split the Array2 into an Array with elements of type Array
let rowFold f z (a:'a[,]) = 
    //let r = [] TODO don't know how to return a list instead of an Array
    let mutable r = Array.create (a.GetLength(0)) z
    for i=0 to (a.GetLength(0)-1) do
       let mutable x = z
       for j=0 to (a.GetLength(1)-1) do
         x <- f x (a.[i,j])
       done     
       //let y = x
       //y :: r
       r.[i] <- x
    done
    r
     
// TODO rename to proper spelling     
let neighborhoodBounds (a:'a[,]) n x y =
    // Assuming that the neighborhood dimensions n is odd so that it can be centred on a specific element
    let m = (n-1)/2
    let subBounds p l =
        let s = if p < m then 0 else p - m
        let t = match p with
                | _ when p < m     -> p + m + 1
                | _ when p + m < l -> n
                | _                -> l - s
        (s, t)
    let (xs, xl) = subBounds (int x) (a.GetLength(0))
    let (ys, yl) = subBounds (int y) (a.GetLength(1))
    (xs, ys, xl, yl)
    
    
(* This is currently done the easy, inefficient way.

   The following Matlab code will write the matrix I1 to the file I1.txt, with one element per line
   by descending each column in turn.
   
    fid = fopen('I1.txt', 'wt');
    fprintf(fid, '%f\n', I1);
    fclose(fid);
 *)
let fileToMatrix f r c =
    let ls = File.ReadAllLines f
    let a = Array2.create r c 0.0
    Array.iteri (fun i (s:string) -> a.[i % r, i / r] <- Convert.ToDouble(s)) ls
    a
    
let floatEquals f1 f2 d = abs(f1 - f2) <= d
        
// TODO would rather use Either than an exception here
let a2FloatEquals (a:float[,]) (b:float[,]) d = 
    // TODO blow up if not same size (a.GetLength(0) = b.GetLength(0) etc)
     for i=0 to (a.GetLength(0)-1) do
       for j=0 to (a.GetLength(1)-1) do
         let fe = floatEquals a.[i,j] b.[i,j] d
         if not fe then failwith (sprintf "Floats at [%d,%d] not equal to distance %f: %f %f" i j d a.[i,j] b.[i,j])
       done
     done
     true