module FELT.Workflows
    
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
        Trainer(new GroupAndKeepStatsTrainer(SingleInstanceBehaviour.Leave)); 
        Classifier(new ZScoreClassifier())
        ]

    let ZScoreGroupedAnti = 
        [ 
        Cleaner(new BasicCleaner()); 
        Selection(new RandomiserSelector());
        Trainer(new GroupAndKeepStatsTrainer(SingleInstanceBehaviour.Leave)); 
        Classifier(new ZScoreClassifier())
        ]

    let ZScoreGroupedSingleFix = 
        [ 
        Cleaner(new BasicCleaner()); 
        Selection(new OneForOneSelector());
        Trainer(new GroupAndKeepStatsTrainer(SingleInstanceBehaviour.Merge)); 
        Classifier(new ZScoreClassifier())
        ]

    let ZScoreGroupedAntiSingleFix = 
        [ 
        Cleaner(new BasicCleaner()); 
        Selection(new RandomiserSelector());
        Trainer(new GroupAndKeepStatsTrainer(SingleInstanceBehaviour.Merge)); 
        Classifier(new ZScoreClassifier())
        ]


    let wfItemCases = FSharpType.GetUnionCases typeof<WorkflowItem>

    let GetUnderlyingTypes (du:'d) =
        let info = FSharpValue.GetUnionFields(du, typeof<'d>, System.Reflection.BindingFlags.Public)
        
        let types = Array.map (fun (x:obj) -> (x.GetType().Name, if x :? FELT.WorkflowItemDescriptor then (x:?>WorkflowItemDescriptor).Description else "")) (snd info)
        System.Diagnostics.Debug.Assert(types.Length = 1)
        ((fst info).Name, fst types.[0], snd types.[0])

    let toString opList =
        let rec f ops bld = 
            match ops with
            | a :: rest -> 
                let pair = GetUnderlyingTypes a
                f rest ( pair :: bld)
            | _ -> bld

        f opList []
