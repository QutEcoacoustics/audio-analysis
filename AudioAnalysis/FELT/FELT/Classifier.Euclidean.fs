namespace FELT.Classifiers
    open FELT.Classifiers

    type EuclideanClassifier() =
        inherit ClassifierBase()
        
        override this.Classify (dataA, dataB) =
            new obj()