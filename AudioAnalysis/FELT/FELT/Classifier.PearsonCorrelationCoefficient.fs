namespace FELT.Classifiers
    open FELT.Classifiers

    type PearsonCorrelationCoefficientClassifier() =
        inherit ClassifierBase()
        
        override this.Classify (dataA, dataB) =
            ClassifierResult.ResultArray Array.empty<Result>