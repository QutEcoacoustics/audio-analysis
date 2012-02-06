module FELT.FindEventsLikeThis 
    // then essential point of this file is that given a data-set,
    // it should be able to run analysis in a configurable,
    // consistent manner.
    // A run will typically look like this
    //
    //  input >> cleaning >> selection >> training >> classification >> results preperation

    open FELT.Cleaners
    open FELT.Classifiers
    open FELT.Selectors
    open FELT.Trainers
    open FELT.Results
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Reflection
    

    type WorkflowItem =
        | Cleaner of BasicCleaner
        | Selection of SelectorBase
        | Trainer of TrainerBase
        | Classifier of ClassifierBase
        | Result of ResultsComputation

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
        let oplst' = List.append operationsList [Result(new ResultsComputation(data ))]

        
        
        let f (state: Data * Data * Result[]) (wfItem: WorkflowItem) =
            let trData, teData, results = state
            match wfItem with
                | Cleaner c -> (c.Clean(trData), c.Clean(teData), results)
                | Selection s -> (s.Pick(trData), teData, results)
                | Trainer t -> (t.Train(trData), teData, results)
                | Classifier c -> (trData, teData, c.Classify(trData, teData))
                | Result r -> 
                    // statefull
                    r.Calculate trData teData results (toString oplst') |> ignore
                    state
        List.scan f (trainingData, testData, null) operationsList


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


    let RunAnalysis trainingData testData tests  data =
        let result = workflow trainingData testData tests data
        result
        
