namespace FELT.Selectors
    open FELT.Selectors
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Collections


    type PredicateSelector(predicate: Class -> Map<ColumnHeader,Value> -> bool) =
        inherit SelectorBase()

        override this.Pick data =
            
            let headers, allSeq = Map.scanAll data.Instances

            let f i row = 
                let c = data.Classes.[i]
                let b = predicate c ( Array.zip headers row |> Map.ofArray)
                if b then
                    Option.Some(i)
                else None

            let chosen = 
                Seq.mapi (f) allSeq
                |> Seq.choose id
                |> Seq.toArray

            let instances' = Map.map (fun key (value: Value[]) -> value.getValues chosen) data.Instances
            let classes' = data.Classes.getValues chosen
            let ``Hello anthony`` = 3

            { data with Instances = instances' ; Classes = classes'}
        
        

