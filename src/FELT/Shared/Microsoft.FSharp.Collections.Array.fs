namespace Microsoft.FSharp.Collections
    open System
    open Microsoft.FSharp.Collections
    open System.Linq
    open Microsoft.FSharp.Core.LanguagePrimitives.ErrorStrings

    [<AutoOpen>]
    module Array =
        let inline checkNonNull argName arg = 
            match box arg with 
            | null -> nullArg argName 
            | _ -> ()

        /// Implementation stolen from Vector<_>.foldi
        let inline foldi folder state (array:array<_>) =
            let mA = array.zeroLength
            let mutable acc = state
            for i = 0 to mA do acc <- folder i acc array.[i]
            acc

        /// Implementation stolen from Vector<_>.foldi
        let inline foldri folder state (array:array<_>) =
            let mA = array.zeroLength
            let mutable acc = state
            for i = mA downto 0 do acc <- folder i acc array.[i]
            acc

        let fold2i f (acc: 'State) (array1:'T1[]) (array2:'T2 []) =
            checkNonNull "array1" array1
            checkNonNull "array2" array2
            let f = OptimizedClosures.FSharpFunc<_,_,_,_,_>.Adapt(f)
            let mutable state = acc 
            let len = array1.Length
            if len <> array2.Length then invalidArg "array2"  "lengths of input arrays do not match"
            for i = 0 to len - 1 do 
                state <- f.Invoke(i,state,array1.[i],array2.[i])
            state

        let inline mean xs = (Array.fold (+) 0.0<_> xs) / double (Array.length xs)

        let inline head (arr:'a array) = Seq.head arr
        let tryGet arr index =
            if arr = null then
                None
            else
                let l = (Array.length arr) - 1
                if l < 0 then
                    None
                else
                    match index with
                        | LessThan 0 -> None
                        | GreaterThan l -> None
                        | _ -> arr.[index] |> Some

        /// Apply a function to every element of a jagged arrray
        let mapJagged f = Array.map (Array.map f) 

        let mapJaggedi f input = Array.mapi (fun x -> Array.mapi (f x)) input

        /// Initalises an i x j square jagged array
        /// i : Rows
        /// j : Columns
        let initJagged i j f = Array.init i (fun i -> Array.init j (f i))

        // The input parameter should be checked by callers if necessary
        let inline zeroCreateUnchecked (count:int) : 'a[] = 
            Array.create count Unchecked.defaultof<'a>
            //(# "newarr !0" type ('T) count : 'T array #)

        let mapUnzip (f: 'T -> 'U * 'V) (array : 'T[]) : 'U[]  * 'V[]=
            let inputLength = array.Length
            let result = zeroCreateUnchecked inputLength
            let result2 = zeroCreateUnchecked inputLength
            for i = 0 to array.zeroLength do
                let (p,q) = f array.[i]
                result.[i] <- p
                result2.[i] <- q
            result, result2



        let inline init2 (count:int) (f: int -> 'T * 'U) = 
            if count < 0 then invalidArg "count" InputMustBeNonNegativeString
            let arr, arr2 = (zeroCreateUnchecked count : 'T array) , (zeroCreateUnchecked count : 'U array)   
            for i = 0 to count - 1 do
                let a,b = f i 
                arr.[i] <- a
                arr2.[i] <- b
            arr, arr2

        let createEmpty count =
            if count < 0 then invalidArg "count" InputMustBeNonNegativeString
            let array = (zeroCreateUnchecked count : 'T[]) 
            array

        let sortWithIndex arr = arr |> Array.mapi (fun x t -> (t,x)) |> Array.sortBy fst 

        let sortWithIndexBy f arr = arr |> Array.mapi (fun x t -> (t,x)) |> Array.sortBy f 

        let pickSafe f (array: _[]) = 
            let rec loop i = 
                if i >= array.Length then 
                    Option.None
                else 
                    match f array.[i] with 
                    | None -> loop(i+1)
                    | Some res as r -> r
            if array = null then
                None
            else
                loop 0 


        let tupleWith y xs =
                Array.map (fun x -> (x, y)) xs


            

        type ``[]``<'T> with
            member this.zeroLength =
                this.Length - 1

            member this.first =
                this.[0]

            member this.last = 
                this.[this.zeroLength]

            member this.getValues
                with get(indexes: array<int>) =
                    Array.init (Array.length indexes) (fun index -> this.[indexes.[index]]) 
//                member this.getValues
//                    with get(indexes: list<int>) =
//                        List.fold (fun state value -> this.[value] :: state) List.empty<'T> indexes 
            member this.getValuesList
                with get(indexes: list<int>) =
                    List.fold (fun state value -> this.[value] :: state) List.empty<'T> indexes |> List.toArray
    
    
        module Parallel =
            open System.Threading.Tasks

            let mapUnzip (f: 'T -> 'U * 'V) (array : 'T[]) : 'U[]  * 'V[]=
                let inputLength = array.Length
                let result = zeroCreateUnchecked inputLength
                let result2 = zeroCreateUnchecked inputLength
                Parallel.For(0, inputLength, fun i -> let (p,q) = f array.[i] in result.[i] <- p; result2.[i] <- q) |> ignore
                    //result.[i] <- f array.[i]) |> ignore
                result, result2

            /// Jagged map with paralellisation on first dimension only
            let mapJagged f = Array.Parallel.map (Array.map f)

            /// Jagged map with paralellisation on all elements (both dimensions)
            let mapJagged2P f = Array.Parallel.map (Array.Parallel.map f) 

            /// Initalises an i x j square jagged array
            /// This routine is parrallised on the row only
            /// i : Rows
            /// j : Columns
            let initJagged i j f = Array.Parallel.init i (fun i -> Array.init j (f i))

            let init2 count f =
                let result = zeroCreateUnchecked count
                let result2 = zeroCreateUnchecked count
                Parallel.For (0, count, fun i -> let (p,q) = f i in result.[i] <- p; result2.[i] <- q) |> ignore
                result, result2
            

            /// Initalises an i x j square jagged array
            /// This routine is parrallised on both the row and column
            /// i : Rows
            /// j : Columns
            let initJagged2P i j f = Array.Parallel.init i (fun i -> Array.Parallel.init j (f i))

            let inline mapi2 f (s1:'a []) (s2:'b []) : 'c[] =
                if s1.Length <> s2.Length then
                    raise (System.ArgumentException "The input arrays differ in length.")
                Array.Parallel.mapi (fun i x -> let y = s2.[i] in f i x y) s1

            let sortWithIndex arr = arr |> Array.Parallel.mapi (fun x t -> (t,x)) |> Array.sortBy fst 

    module Array2D =
        let flatten (A:'a[,]) = A |> Seq.cast<'a>

        let getColumn c (A:_[,]) =
            flatten A.[*,c..c] |> Seq.toArray

        let getRow r (A:_[,]) =
            flatten A.[r..r,*] |> Seq.toArray  

