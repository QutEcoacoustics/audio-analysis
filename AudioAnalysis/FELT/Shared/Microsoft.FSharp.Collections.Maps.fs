namespace Microsoft.FSharp.Collections

    open Microsoft.FSharp.Core
    open System.Collections.Generic

    [<AutoOpen>]
    module Map =
        let keys map =  Map.toSeq map |> fst |> Seq.toArray

        let getKey (kvp:KeyValuePair<'a, 'b>) = kvp.Key

        let getValue (kvp:KeyValuePair<'a, 'b>) = kvp.Value