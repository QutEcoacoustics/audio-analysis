module FELT.FindEventsLikeThis 
    // then essential point of this file is that given a data-set,
    // it should be able to run analysis in a configurable,
    // consistent manner.
    // A run will typically look like this
    //
    //  input >> cleaning >> selection >> training >> classification >> results preperation
    
    open FELT.Cleaners
    open FELT.Classifiers
    open FELT.Transformers
    open FELT.Selectors
    open FELT.Trainers
    open FELT.Results
    open FELT.Workflows
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Reflection
    open System.Extensions
    

    // TODO: pipe/compose
    let workflow trainingData testData  operationsList (config:ReportConfig) = 
        Info "Started analysis workflow"

        let oplst' =
            if List.exists (function | Result r -> true | _ -> false) operationsList then
                operationsList
             else
                List.append operationsList [Result(Console(new ResultsComputation(config)))]
        


        let f (state: Data * Data * ClassifierResult * Map<string, obj>) (wfItem: WorkflowItem) =
            Infof "Started workflow item %A" (GetUnderlyingTypes wfItem)
            let trData, teData, results, extraDataStore = state
            match wfItem with
                | Cleaner c -> (c.Clean(trData), c.Clean(teData), results, extraDataStore)
                | Transformer t -> 
                    let (ttr, tte, extraData) = t.Transform trData teData
                    (
                        ttr,
                        tte, 
                        results,
                        (if extraData.IsSome then Map.add (t.GetType().Name) extraData.Value extraDataStore else extraDataStore)
                    )
                | Selection s -> (s.Pick(trData), teData, results, extraDataStore)
                | Trainer t -> (t.Train(trData), teData, results, extraDataStore)
                | Classifier c -> (trData, teData, (c.Classify trData teData), extraDataStore)
                | Result r ->
                    match r with
                        | Console rc -> 
                            // statefull
                            rc.Calculate trData teData results (toString oplst') |> ignore
                        | OutFile filePath ->
                            Felt.Results.ResultsOutFile.Output config filePath trData extraDataStore

                    (trData, teData, ClassifierResult.Nothing, extraDataStore)
                | Dummy ->
                    // noop
                    state
                | _ -> 
                    Errorf "Workflow item %A not supported" wfItem
                    failwith "Workflow error"
        
        List.scan f (trainingData, testData, ClassifierResult.Nothing, Map.empty) oplst' |> ignore

    
    let RunAnalysis (trainingData:Data) (testData:Data) (tests: WorkflowItem list) (transformList: List<string * string *string>) data =
        
        // inject transforms after cleaner in workflow
        let tf tuple =
            let feature = fst3 tuple
            let newName = snd3 tuple
            let (operation:string) = third3 tuple
            if (trainingData.Headers.ContainsKey(feature) && testData.Headers.ContainsKey(feature)) then
                match operation.Trim() with
                    | "ModuloTime" -> WorkflowItem.Transformer (new Transformers.TimeOfDay.TimeOfDayTransformer(feature, newName)) 
                    | _ -> 
                    ignore <| apply (ErrorFail, failwith) (sprintf "No transform is known by the name %s" operation) ; WorkflowItem.Dummy
            else
                Error "A transform was included for a feature not available in the data sets!"
                WorkflowItem.Dummy

        let txs = List.map tf transformList 
        let head, rest = 
            (match tests.Head with 
            | WorkflowItem.Cleaner c -> Some(WorkflowItem.Cleaner(c))
            | _  ->Error "Undefined workflow!!!!!!!!!!!!!!" ; Option.None
            )
            ,tests.Tail

        //let (_::rest) = tests
        let tests' = head.Value :: (List.append txs rest)

        let result = workflow trainingData testData tests' data
        result

//    [<EntryPoint>]
//    let Entry args =
//        printfn "This executable is not designed to run on its own yet... exiting"
//        
//        // error code (permanent fail)
//        1