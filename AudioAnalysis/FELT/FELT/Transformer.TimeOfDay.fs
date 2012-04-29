namespace FELT.Transformers

    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Numerics
    open Microsoft.FSharp.Collections
    open System
    open System.Diagnostics
    open QuickGraph


    module TimeOfDay = 
        type DayPhase = string

        let phases : DayPhase array = [| "Dawn" ; "TwilightA" ; "Sunrise" ; "Morning" ; "Daylight" ; "Evening" ; "Sunset" ; "TwilightB" ; "Dusk" ; "Night" |]

        let makeCyclicGraph (g: 'a array) =
            let todg = new QuickGraph.UndirectedGraph<'a, UndirectedEdge<'a>>()

            todg.AddVertexRange(g) |> ignore

            let addLink (a,b) =
                todg.AddEdge(new UndirectedEdge<'a>(a,b))
            let added = Seq.pairwise g |>  Seq.map addLink |> Seq.toArray
            
            addLink ( g.first, g.last) |> ignore
            Debug.Assert(Seq.tryFind not added |> Option.isNone)
         
            todg

        let timeOfDayGraph = makeCyclicGraph phases
            

        type TimeOfDayTransformer(feature:string, newName:string) =
            inherit TransformerBase()

            override this.Transform (trainingData:Data) (testData:Data) =


                

                let tf (v:Value) : Value=
                    match v with
                    | IsDate d -> 
                        // take only the time component, round, convert to modular number
                        let z = d.Value.TimeOfDay.TotalMinutes |> round |> int |> Z1440 
                        upcast( new ModuloMinute(z))
                    | _ -> 
                        ErrorFail "Modulo tansformer was given date a date it could not decode! Boo!" |> failwith
                        upcast (new ModuloMinute(0Z))

                let remake (instances:Map<ColumnHeader, Value array>) =
                    let old = instances.[feature]
                    let inst' = instances.Remove(feature)
                    old |> Array.map tf |> (flip(Map.add feature)) inst'


                let newTrainingInstances = remake trainingData.Instances
                let newTestInstances = remake testData.Instances
            

                ({ trainingData with Instances = newTrainingInstances }, { testData with Instances = newTestInstances })
        
        

