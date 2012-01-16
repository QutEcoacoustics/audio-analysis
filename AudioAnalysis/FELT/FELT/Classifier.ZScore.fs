namespace FELT.Classifiers
    open FELT.Classifiers

    type ZScoreClassifier() =
        inherit ClassifierBase()
        
        
        override this.Classify (dataA, dataB) =
            new obj()