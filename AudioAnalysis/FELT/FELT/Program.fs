
namespace FELT
    open Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared
    
    module Helpers =

        let headersMatch trainingData testData =
            let trKeys = Map.keys (trainingData.Instances)
            let teKeys = Map.keys (testData.Instances)
            if trKeys <> teKeys then
                invalidArg "trainingData" "the columns in test and training data should be the same"

    type WorkflowItemDescriptor =
        abstract member Description : string
             with get