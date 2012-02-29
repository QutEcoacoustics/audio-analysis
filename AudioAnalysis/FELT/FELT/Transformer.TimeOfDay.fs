namespace FELT.Transformers
    open FELT.Tranformers
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Numerics
    open System

    type TimeOfDayTransformer(feature:string, newName:string) =
        inherit TransformerBase()

        override this.Transform (trainingData:Data) (testData:Data) =
            let tf (v:Value) : Value=
                match v with
                | IsDate d -> 
                    // take only the time component, round, convert to modular number
                    let z = d.Value.TimeOfDay.TotalMinutes |> round |> int |> Z1440 
                    upcast( new ModuloHour(z))
                | _ -> 
                    ErrorFail "Modulo tansformer was given date a date it could not decode! Boo!" |> failwith
                    upcast (new ModuloHour(0Z))

            let remake (instances:Map<ColumnHeader, Value array>) =
                let old = instances.[feature]
                let inst' = instances.Remove(feature)
                old |> Array.map tf |> (flip(Map.add feature)) inst'


            let newTrainingInstances = remake trainingData.Instances
            let newTestInstances = remake testData.Instances
            

            ({ trainingData with Instances = newTrainingInstances }, { testData with Instances = newTestInstances })
        
        

