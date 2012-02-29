namespace Microsoft.FSharp.Core
//    [<Core.AutoOpenAttribute()>]
//module ArrayExtensions 
//        type ``[]``<'T> with
//                member this.zeroLength =
//                    this.Length - 1
//                member this.getValues
//                    with get(indexes: array<int>) =
//                        Array.init (Array.length indexes) (fun index -> this.[indexes.[index]]) 
//                member this.getValues
//                    with get(indexes: list<int>) =
//                        List.fold (fun state value -> this.[value] :: state) List.empty<'T> indexes
                        
    [<AutoOpen>]
    module Operators =
        let fsts tuples = Seq.map (fst) tuples

        let third3 (_,_,c) = c
        let fst3 (a,_,_) = a
        let snd3 (_,b,_) = b