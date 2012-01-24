// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#load "Operators.fs"


//open Felt.Shared.Utilities
//open Felt.Shared.CSV

//rowToList "Hello, this is  a,test,\"hell,lo\", to see,, what happen, s" ',';;


type aClass(valA:int) =
    //let aVal:int = valA
//    static member ci = ComparisonIdentity.Structural<aClass>
//    static member hi = HashIdentity.Structural<aClass>

    member this.Value
            with get() = valA //aVal

//    override x.Equals(yobj) =
//        match yobj with
//        | :? aClass as y ->  aClass.hi.Equals(x, y)
//        | _ -> false
//    
//    override x.GetHashCode() =    aClass.hi.GetHashCode(x)
// 
//    interface System.IComparable with
//        member x.CompareTo yobj =
//            match yobj with
//            | :? aClass as y -> Unchecked.compare x.Value y.Value
//            | _ -> invalidArg "yobj" "cannot compare values of different types"
//   
//        end




let a1 = new aClass(3)
let a2 = new aClass(3)
let aEquals = a1 = a2