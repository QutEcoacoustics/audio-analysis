namespace FELT.Transformers
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Collections
    open System
    
    [<AbstractClass>]
    type TransformerBase() = class
        
        abstract member Transform : Data -> Data -> Data * Data * Option<Object>

        end
        

    module Transformer =
        let remake transformFunction features newName (instances:Map<ColumnHeader, Value array>) =
            let old = Map.filter (fun k v -> Set.contains k features) instances
            //let old = instances.[feature]
            let inst' = Set.fold (fun map feature -> Map.remove feature map) instances features
            //let inst' = instances.Remove(feature)
            
            let hdrs, rows = Map.scanAll old
            
            rows 
            |> Seq.map transformFunction 
            |> (fun finalCol -> Map.add newName (finalCol |> Seq.toArray) inst')