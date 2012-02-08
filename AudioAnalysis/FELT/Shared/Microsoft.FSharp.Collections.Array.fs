namespace Microsoft.FSharp.Collections
    open System

    [<AutoOpen>]
    module Array =
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

        let inline mean xs = (Array.fold (+) 0.0<_> xs) / double (Array.length xs)

        let inline head (arr:'a array) = Seq.head arr

        /// Apply a function to every element of a jagged arrray
        let mapJagged f = Array.map (Array.map f) 

        let mapJaggedi f input = Array.mapi (fun x -> Array.mapi (f x)) input

        /// Initalises an i x j square jagged array
        /// i : Rows
        /// j : Columns
        let initJagged i j f = Array.init i (fun i -> Array.init j (f i))

        let sortWithIndex arr = arr |> Array.mapi (fun x t -> (t,x)) |> Array.sortBy fst 

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

        type ``[]``<'T> with
            member this.zeroLength =
                this.Length - 1
            member this.getValues
                with get(indexes: array<int>) =
                    Array.init (Array.length indexes) (fun index -> this.[indexes.[index]]) 
//                member this.getValues
//                    with get(indexes: list<int>) =
//                        List.fold (fun state value -> this.[value] :: state) List.empty<'T> indexes 
            member this.getValues
                with get(indexes: list<int>) =
                    List.fold (fun state value -> this.[value] :: state) List.empty<'T> indexes |> List.toArray
    
    module Array2D =
        let flatten (A:'a[,]) = A |> Seq.cast<'a>

        let getColumn c (A:_[,]) =
            flatten A.[*,c..c] |> Seq.toArray

        let getRow r (A:_[,]) =
            flatten A.[r..r,*] |> Seq.toArray  

