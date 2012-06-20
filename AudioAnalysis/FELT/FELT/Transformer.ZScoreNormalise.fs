namespace FELT.Transformers
    open FELT
    open FELT.Transformers
    open System
    open Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared.CSV
    open MQUTeR.FSharp.Shared
    open MathNet.Numerics
    open MQUTeR.FSharp.Shared.StringStats
    open Microsoft.FSharp.Numerics
    open Microsoft.FSharp.Math


    // this class converts all numerical values into a z-score... ignoring any grouping or clustering
    // only converts number types
    type ZScoreNormalise() =
        inherit TransformerBase()

        let zscore mean stddev s : Value = upcast new Number(Maths.zscore s mean stddev)

        member this.NormaliseWithValues (anyData:Data) (featureMap) =
            let normalise2 featureName vs = 
                if Map.containsKey featureName featureMap then
                    let mean, stddev = featureMap.[featureName]
                    
                    let numbers = 
                        match vs with
                        | IsAnyNumbers ns ->
                            ns
                        | _ -> failwith "not implemented"
                
                    let zs = Array.map (zscore mean stddev) numbers
                    zs
                else
                    vs

            let normedVs = Map.map (normalise2) anyData.Instances
            { anyData with Instances = normedVs }

            

        override this.Transform (trainingData: Data)  (testData: Data):  Data * Data * Option<obj> =
            
            // do standard feature check
            Helpers.headersMatch trainingData testData
            if not (Helpers.IsTestData testData && Helpers.IsTrainingData trainingData) then
                invalidArg "trainingData, testData" "Input of data must be the correct way round"


            // scan headers for features that are numbers
            let subFeatures = Map.filter (fun featureName dataType -> dataType = DataType.Number) trainingData.Headers |> Map.keys

            // for each feature, normalise
            let normalise (featuresToNormalise) featureName vs =
                if Array.exists ((=) featureName) featuresToNormalise then
                    let numbers = 
                        match vs with
                        | IsAnyNumbers ns ->
                            ns
                        | _ -> failwith "not implemented"
                    let stddev, mean = Maths.Array.stdDeviationAndMean numbers
                
                    let zs = Array.map (zscore mean stddev) numbers
                    zs, Some(mean, stddev)
                else
                    vs, None

            // all numbers in training data are normalised properly
            let normedVsTr = Map.map (normalise subFeatures) trainingData.Instances

            // all scores in test data should be normalised, w/respect to training scores, 
            // i.e. z-score in test data based off mean and stddev of training data
            let subF = Map.filter (fun name (vs, avgInfo) -> Option.isSome avgInfo) normedVsTr |> Map.map (fun key (vs, op) -> op.Value )

            let normedTestData = this.NormaliseWithValues testData subF

            // TODO: need someway to return subF

            ({ trainingData with Instances = (Map.map (fun _ (v, _) -> v) normedVsTr) }, normedTestData, Some(upcast subF))

