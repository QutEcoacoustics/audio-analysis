namespace FELT.Transformers

    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Math
    open Microsoft.FSharp.Numerics
    open Microsoft.FSharp.Collections
    open System
    open System.Diagnostics
    open QuickGraph

    module TimeOfDay = 



        let makeCyclicGraph (g: 'a array) =
            let todg = new QuickGraph.UndirectedGraph<'a, UndirectedEdge<'a>>()

            todg.AddVertexRange(g) |> ignore

            let addLink (a,b) =
                todg.AddEdge(new UndirectedEdge<'a>(a,b))
            let added = Seq.pairwise g |>  Seq.map addLink |> Seq.toArray
            
            addLink ( g.first, g.last) |> ignore
            Debug.Assert(Seq.tryFind not added |> Option.isNone)
         
            todg

        let timeOfDayGraph = makeCyclicGraph (SunCalc.detailedPhases)
            
        type DayPhaseTransformer(latName : string, lngName : string, timeTag : string, dayPhaseColumnName : string) =
            inherit TransformerBase()

            override this.Transform (trainingData: Data) (testData: Data) =
                // ensure all three given fields exist in the data and the data types are correct
                if 
                    trainingData.hasColumn latName DataType.Number &&
                    trainingData.hasColumn lngName DataType.Number &&
                    trainingData.hasColumn timeTag DataType.Date &&
                    testData.hasColumn latName DataType.Number &&
                    testData.hasColumn lngName DataType.Number &&
                    testData.hasColumn timeTag DataType.Date then

                    // transform function, takes a row (a set of columns) and collapses it down into a cell (single column row)
                    let tf (vs: Value[]) : Value =
                        // set automatically orders its values
                        let lat, lng, time = vs.[0] :?> Number, vs.[1] :?> Number, vs.[2] :?> Date

                        // this will be called many times with ***almost the same*** input - optimization needed?
                        let phases = SunCalc.getDayInfo time.Value lat.Value lng.Value
                        let phase = Map.tryPick (fun key value -> if Interval.isInRange value time.Value then Some(key) else None) phases
                        let phase' =
                            if phase.IsNone then
                                SunCalc.Night
                            else
                                phase.Value

                        upcast (new Text(phase'))


                    let remake' = Transformer.remake tf (set [latName; lngName; timeTag]) dayPhaseColumnName
                    let newTrainingInstances = remake' trainingData.Instances
                    let newTestInstances = remake' testData.Instances

                    let hdrsTr = trainingData.Headers.Remove(latName).Remove(lngName).Remove(timeTag).Add(dayPhaseColumnName, DataType.Text)
                    let hdrsTe =     testData.Headers.Remove(latName).Remove(lngName).Remove(timeTag).Add(dayPhaseColumnName, DataType.Text)

                    ({ trainingData with Instances = newTrainingInstances; Headers = hdrsTr }, { testData with Instances = newTestInstances; Headers = hdrsTe }, None)
                else
                    invalidArg "" "Missing correct columns for day phase transformation"


        type TimeOfDayTransformer(feature:string, newName:string) =
            inherit TransformerBase()

            override this.Transform (trainingData:Data) (testData:Data) =


                let tf (v:Value[]) : Value=
                    if (v.Length <> 1) then
                        invalidArg "v" "only supports one column input"

                    match Seq.first v with
                    | IsDate d -> 
                        // take only the time component, round, convert to modular number
                        let z = d.Value.TimeOfDay.TotalMinutes |> round |> int |> Z1440 
                        upcast( new ModuloMinute(z))
                    | _ -> 
                        ignore <| apply (ErrorFail, failwith) "Modulo tansformer was given date a date it could not decode! Boo!"
                        
                        upcast (new ModuloMinute(0Z))

                

                let remake' = Transformer.remake tf (set [feature]) newName
                let newTrainingInstances =  remake'  trainingData.Instances
                let newTestInstances = remake' testData.Instances
            
                let hdrsTr = trainingData.Headers.Remove(feature).Add(newName, DataType.Text)
                let hdrsTe =     testData.Headers.Remove(feature).Add(newName, DataType.Text)

                ({ trainingData with Instances = newTrainingInstances }, { testData with Instances = newTestInstances }, None)
        
        

