namespace Microsoft.FSharp.Collections

    open Microsoft.FSharp.Core

    [<AutoOpen>]
    module Map =
        let keys map =  Map.toSeq map |> fst |> Seq.toArray
