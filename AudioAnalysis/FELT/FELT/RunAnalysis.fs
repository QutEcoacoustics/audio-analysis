module FELT.FindEventsLikeThis 
    // then essential point of this file is that given a data-set,
    // it should be able to run analysis in a configurable,
    // consistent manner.
    // A run will typically look like this
    //
    //  input >> cleaning >> selection >> training >> classification >> results preperation

    open FELT.Cleaners
    open FELT.Classifiers
    open FELT.Tranformers
    open FELT.Selectors
    open FELT.Trainers
    open FELT.Results
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Reflection
    

    type WorkflowItem =
        | Dummy of obj
        | Cleaner of BasicCleaner
        | Transformer of TransformerBase
        | Selection of SelectorBase
        | Trainer of TrainerBase
        | Classifier of ClassifierBase
        | Result of ResultsComputation

    let Basic = 
        [ 
        Cleaner(new BasicCleaner()); 
        Selection(new OneForOneSelector());
        Trainer(new OneForOneTrainer()); 
        Classifier(new EuclideanClassifier())
        ]

    let BasicGrouped = 
        [ 
        Cleaner(new BasicCleaner()); 
        Selection(new OneForOneSelector());
        Trainer(new GroupTrainer()); 
        Classifier(new EuclideanClassifier())
        ]

    let BasicAnti = 
        [ 
        Cleaner(new BasicCleaner()); 
        Selection(new RandomiserSelector());
        Trainer(new GroupTrainer()); 
        Classifier(new EuclideanClassifier())
        ]

    let BasicGroupedAnti = 
        [ 
        Cleaner(new BasicCleaner()); 
        Selection(new RandomiserSelector());
        Trainer(new GroupTrainer()); 
        Classifier(new EuclideanClassifier())
        ]

    let ZScoreGrouped = 
        [ 
        Cleaner(new BasicCleaner()); 
        Selection(new OneForOneSelector());
        Trainer(new GroupAndKeepStatsTrainer()); 
        Classifier(new ZScoreClassifier())
        ]

    let ZScoreAnti = 
        [ 
        Cleaner(new BasicCleaner()); 
        Selection(new RandomiserSelector());
        Trainer(new GroupAndKeepStatsTrainer()); 
        Classifier(new ZScoreClassifier())
        ]

        


    let wfItemCases = FSharpType.GetUnionCases typeof<WorkflowItem>

    let GetUnderlyingTypes (du:'d) =
        let info = FSharpValue.GetUnionFields(du, typeof<'d>, System.Reflection.BindingFlags.Public)
        let types = Array.map (fun x -> x.GetType().Name) (snd info)
        ((fst info).Name, (String.concat ", " types))

    let toString opList =
        let rec f ops bld = 
            match ops with
            | a :: rest -> 
                let pair = GetUnderlyingTypes a
                f rest ( pair :: bld)
            | _ -> bld

        f opList []

    // TODO: pipe/compose
    let workflow trainingData testData  operationsList (data:ReportConfig) = 
        Info "Started analysis workflow"

        let oplst' = List.append operationsList [Result(new ResultsComputation(data ))]
        
        let f (state: Data * Data * Result[]) (wfItem: WorkflowItem) =
            Infof "Started workflow item %A" (GetUnderlyingTypes wfItem)
            let trData, teData, results = state
            match wfItem with
                | Cleaner c -> (c.Clean(trData), c.Clean(teData), results)
                | Transformer t -> 
                    let (ttr, tte) = t.Transform trData teData
                    (ttr, tte, results)
                | Selection s -> (s.Pick(trData), teData, results)
                | Trainer t -> (t.Train(trData), teData, results)
                | Classifier c -> (trData, teData, c.Classify(trData, teData))
                | Result r -> 
                    // statefull
                    r.Calculate trData teData results (toString oplst') |> ignore
                    state
                | _ -> 
                    Errorf "Workflow item %A not supported" wfItem
                    failwith "Workflow error"

        List.scan f (trainingData, testData, null) oplst'

    
    let RunAnalysis (trainingData:Data) (testData:Data) (tests: WorkflowItem list) (transformList: seq<string * string *string>) data =
        
        // inject transforms after cleaner in workflow
        let tf (feature, newName, operation:string) : WorkflowItem =
            if (trainingData.Headers.ContainsKey feature && testData.Headers.ContainsKey feature) then
                match operation.Trim().ToLowerInvariant() with
                    | "ModuloTime" -> WorkflowItem.Transformer (new Transformers.TimeOfDayTransformer(feature, newName)) 
                    | _ -> ErrorFailf "No transform is known by the name %s" operation; WorkflowItem.Dummy(null)
            else
                Error "A transform was included for a feature not available in the data sets!"
                failwith "Transform error"

        let txs = Seq.map tf transformList |> Seq.toList
        let head : WorkflowItem option= 
            match tests.Head with 
            | WorkflowItem.Cleaner c -> Some(WorkflowItem.Cleaner(c))
            | _  ->Error "Undefined workflow!!!!!!!!!!!!!!" ; Option.None

        let (_::rest) = tests
        let tests' = head.Value :: (List.append txs rest)

        let result = workflow trainingData testData tests data
        result

    [<EntryPoint>]
    let Entry args =
        printfn "This executable is not designed to run on its own yet... exiting"
        
        // error code (permanent fail)
        1