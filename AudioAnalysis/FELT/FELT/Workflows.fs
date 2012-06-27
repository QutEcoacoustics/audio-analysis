module FELT.Workflows
    
    open FELT.Cleaners
    open FELT.Classifiers
    open FELT.Transformers
    open FELT.Selectors
    open FELT.Trainers
    open FELT.Results
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Reflection
    open System.IO
    
    type ResultComputationType =
        | Console of ResultsComputation
        | OutFile of FileInfo

    type WorkflowItem =
        | Dummy
        | Cleaner of BasicCleaner
        | Transformer of TransformerBase
        | Selection of SelectorBase
        | Trainer of TrainerBase
        | Classifier of ClassifierBase
        | Result of ResultComputationType

    let Analyses = 
        new Map<string, WorkflowItem list>(
            [
            (
                "Basic",
                [ 
                Cleaner(new BasicCleaner()); 
                Selection(new OneForOneSelector());
                Trainer(new OneForOneTrainer()); 
                Classifier(new EuclideanClassifier())
                ]
            );
            (
                "BasicGrouped", 
                [ 
                Cleaner(new BasicCleaner()); 
                Selection(new OneForOneSelector());
                Trainer(new GroupTrainer()); 
                Classifier(new EuclideanClassifier())
                ]
            );
            (
                "BasicAnti", 
                [ 
                Cleaner(new BasicCleaner()); 
                Selection(new RandomiserSelector());
                Trainer(new OneForOneTrainer()); 
                Classifier(new EuclideanClassifier())
                ]
            );
            (
                "BasicGroupedAnti", 
                [ 
                Cleaner(new BasicCleaner()); 
                Selection(new RandomiserSelector());
                Trainer(new GroupTrainer()); 
                Classifier(new EuclideanClassifier())
                ]
            );
            (
                "ZScoreGrouped", 
                [ 
                Cleaner(new BasicCleaner()); 
                Selection(new OneForOneSelector());
                Trainer(new GroupAndKeepStatsTrainer(SingleInstanceBehaviour.Leave)); 
                Classifier(new ZScoreClassifier())
                ]
            );
            (
                "ZScoreGroupedAnti", 
                [ 
                Cleaner(new BasicCleaner()); 
                Selection(new RandomiserSelector());
                Trainer(new GroupAndKeepStatsTrainer(SingleInstanceBehaviour.Leave)); 
                Classifier(new ZScoreClassifier())
                ]
            );
            (
                "ZScoreGroupedSingleFix", 
                [ 
                Cleaner(new BasicCleaner()); 
                Selection(new OneForOneSelector());
                Trainer(new GroupAndKeepStatsTrainer(SingleInstanceBehaviour.Merge)); 
                Classifier(new ZScoreClassifier())
                ]
            );
            (
                "ZScoreGroupedAntiSingleFix", 
                [ 
                Cleaner(new BasicCleaner()); 
                Selection(new RandomiserSelector());
                Trainer(new GroupAndKeepStatsTrainer(SingleInstanceBehaviour.Merge)); 
                Classifier(new ZScoreClassifier())
                ]
            );
            (
                "GlobalZScore", 
                [ 
                Cleaner(new BasicCleaner()); 
                Selection(new OneForOneSelector());
                Transformer(new Transformers.ZScoreNormalise());
                Trainer(new GroupTrainer()); 
                Classifier(new EuclideanClassifier())
                ]
            );
            (
                "GlobalZScoreAnti", 
                [ 
                Cleaner(new BasicCleaner()); 
                Selection(new RandomiserSelector());
                Transformer(new Transformers.ZScoreNormalise());
                Trainer(new GroupTrainer()); 
                Classifier(new EuclideanClassifier())
                ]
            );
            (
                "BasicGrouped-ReferenceOnly", 
                [ 
                Cleaner(new BasicCleaner()); 
                Selection(new PredicateSelector(fun headers values -> values.["ReferenceTag"] :?> FuzzyBit |> (fun fb -> fb.Value >= 1.0) ));
                Trainer(new GroupTrainer()); 
                Classifier(new EuclideanClassifier())
                ]
            );

            (
                "WebsiteWorkFlow-SaveBinary", 
                [ 
                Cleaner(new BasicCleaner()); 
                Selection(new OneForOneSelector());
                Transformer(new Transformers.ZScoreNormalise());
                Trainer(new GroupTrainer()); 
                Result(OutFile(new FileInfo("C:\Work\Sensors9\AudioDataStorage\suggestions\cachedFile.feltcache")))
                //Classifier(new EuclideanClassifier())
                ]
            );


            // end declaration
            ])
    



    let wfItemCases = FSharpType.GetUnionCases typeof<WorkflowItem>

    let GetUnderlyingTypes (du:'d) =
        let info = FSharpValue.GetUnionFields(du, typeof<'d>, System.Reflection.BindingFlags.Public)
        
        let types = Array.map (fun (x:obj) -> (x.GetType().Name, if x :? FELT.WorkflowItemDescriptor then (x:?>WorkflowItemDescriptor).Description else "")) (snd info)
        if types.Length = 0 then
            ((fst info).Name, "[NO_UNDERLYING_TYPE]", "")
        else
            ((fst info).Name, fst types.[0], snd types.[0])

    let toString opList =
        let rec f ops bld = 
            match ops with
            | a :: rest -> 
                let pair = GetUnderlyingTypes a
                f rest ( pair :: bld)
            | _ -> bld

        f opList []
