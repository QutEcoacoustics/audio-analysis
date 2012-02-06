// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#load "Operators.fs"


//open Felt.Shared.Utilities
//open Felt.Shared.CSV

//rowToList "Hello, this is  a,test,\"hell,lo\", to see,, what happen, s" ',';;

//
//type aClass(valA:int) =
//    //let aVal:int = valA
////    static member ci = ComparisonIdentity.Structural<aClass>
////    static member hi = HashIdentity.Structural<aClass>
//
//    member this.Value
//            with get() = valA //aVal
//
////    override x.Equals(yobj) =
////        match yobj with
////        | :? aClass as y ->  aClass.hi.Equals(x, y)
////        | _ -> false
////    
////    override x.GetHashCode() =    aClass.hi.GetHashCode(x)
//// 
////    interface System.IComparable with
////        member x.CompareTo yobj =
////            match yobj with
////            | :? aClass as y -> Unchecked.compare x.Value y.Value
////            | _ -> invalidArg "yobj" "cannot compare values of different types"
////   
////        end
//
//
//
//
//let a1 = new aClass(3)
//let a2 = new aClass(3)
//let aEquals = a1 = a2
#load "Equality.fs"
#load "Operators.fs"
#load "Microsoft.FSharp.Collections.Array.fs"
#load "DataSet.fs"

#r @"D:\Work\Sensors\AudioAnalysis\FELT\Shared\bin\Debug\MQUTeR.FSharp.Shared.dll"
open Microsoft.FSharp.Collections
open System.Diagnostics
open MQUTeR.FSharp.Shared

let stopPrint (sw:Stopwatch) = sw.Stop(); printfn "elapsed time: %i" sw.ElapsedMilliseconds

let s1 = Stopwatch.StartNew()
let arr = Array2D.init 15000 1000 (fun i j -> new Number(float (i * j)))
stopPrint s1

let s2 = Stopwatch.StartNew()
let arr2 = 
    Array.init 1000 (fun j -> Array.init 15000 (fun i -> new Number(float(i * j))))
stopPrint s2

let s3 = Stopwatch.StartNew()
let arr3 = array2D arr2
stopPrint s3

let s4 = Stopwatch.StartNew()
let check = arr3 = arr
stopPrint s4