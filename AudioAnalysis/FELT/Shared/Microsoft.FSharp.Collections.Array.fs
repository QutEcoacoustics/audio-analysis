namespace Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared.Utilities

    [<AutoOpen>]
    module Array =
        /// Implementation stolen from Vector<_>.foldi
        let inline foldi folder state (array:array<_>) =
            let mA = array.zeroLength
            let mutable acc = state
            for i = 0 to mA do acc <- folder i acc array.[i]
            acc

        let inline mean xs = (Array.fold (+) 0.0<_> xs) / double (Array.length xs)

        let inline head (arr:'a array) = Seq.head arr
    
    //module Microsoft.FSharp.Core.ArrayExtensions 

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