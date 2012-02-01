namespace FELT.Classifiers
    open FELT.Classifiers

    type ConsineSimilarityClassifier() =
        inherit ClassifierBase()
        
        override this.Classify (dataA, dataB) =
            Array.empty<Result>