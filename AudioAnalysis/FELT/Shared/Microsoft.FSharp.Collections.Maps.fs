namespace Microsoft.FSharp.Collections
    open System
    open Microsoft.FSharp.Core
    open System.Collections.Generic

    [<AutoOpen>]
    module Map =
        let keys map =  Map.toSeq map |> fsts |> Seq.toArray

        let getKey (kvp:KeyValuePair<'a, 'b>) = kvp.Key

        let getValue (kvp:KeyValuePair<'a, 'b>) = kvp.Value

        let getFirstValue (map:Map<_,_>) = (Seq.nth 1 map).Value

        let getNthValue (map:Map<_,_>) index = (Seq.nth index map).Value

        let keepThese map keys =
            Map.fold (fun m key value -> if Set.contains key keys then Map.add key value m else m) Map.empty map

        /// Merges two maps together. If there is a key conflict simple keeps value from the second list.
        let merge a b = Map.fold (fun acc key value -> Map.add key value acc) a b
            
        let scanAll (map:Map<_, _[]>) =
            let kvps =  Map.toArray map
            let hdrs, cols = Array.unzip kvps
            let numRows = (first cols).Length

            let pickRow i =  Array.map (flip(Array.get)  i) cols

            hdrs, seq {
                for i in 0..numRows-1 do
                    yield pickRow i
            }

        let getRow (map:Map<_, _[]>) r =

            let result = Array.ofSeq ( 
                            seq{
                                for col in map do
                                    yield col.Value.[r]
                                }   )
            result
        
        let getRowf (map:Map<_, _[]>) r f =

            let result = Array.ofSeq ( 
                            seq{
                                for col in map do
                                    yield f r (col.Key) (col.Value.[r])
                                }   )
            result
