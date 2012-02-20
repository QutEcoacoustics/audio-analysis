namespace Microsoft.FSharp.Collections

    open Microsoft.FSharp.Core
    open System.Collections.Generic

    [<AutoOpen>]
    module Map =
        let keys map =  Map.toSeq map |> fst |> Seq.toArray

        let getKey (kvp:KeyValuePair<'a, 'b>) = kvp.Key

        let getValue (kvp:KeyValuePair<'a, 'b>) = kvp.Value

        let getFirstValue (map:Map<_,_>) = (Seq.nth 1 map).Value

        let getNthValue (map:Map<_,_>) index = (Seq.nth index map).Value