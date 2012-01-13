module FindEventsLikeThis =
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

    type WorkflowItem =
        | Cleaner of BasicCleaner
        | Selection of SelectorBase
        | Trainer of TrainerBase
        | Classifier of ClassifierBase
        | Results of ResultsComputation

    // TODO: pipe/compose
    let workflow data operationsList = 
        let oplst' = List.append operationsList [Results(new ResultsComputation())]
        let cleaned = Cleaner.clean data
        
        FELT.Selectors.SelectorBase >> training >> classi


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
        Trainer(new OneForOneTrainer()); 
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


    let RunAnalysis data tests =
        let result = workflow data tests
        
