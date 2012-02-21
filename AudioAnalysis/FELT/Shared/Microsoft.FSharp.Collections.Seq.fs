namespace Microsoft.FSharp.Collections
    open System
    open Microsoft.FSharp.Collections
    open System.Linq

    [<AutoOpen>]
    module Seq =

            let tupleWith y xs =
                Seq.map (fun x -> (x, y)) xs

            type Count = int
            let ZeroCount:Count = 0

            let histogramBy f bins collection =
                let m = bins |> tupleWith 0 |>  Map.ofSeq
                let g (map:Map<_, Count>) item = 
                    let x = f item
                    // this is meant to throw an exception if element does not exist (i.e. there is no bin)
                    let count = map.[x]
                    Map.add x (count + 1) map
 
                PSeq.fold (g) m collection