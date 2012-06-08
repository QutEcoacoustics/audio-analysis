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
    type RemoveFeatures(featuresToRemove: ColumnHeader list) =
        inherit TransformerBase()

        override this.Transform (trainingData: Data)  (testData: Data):  Data * Data =
            
            // do standard feature check
            Helpers.headersMatch trainingData testData
            if not (Helpers.IsTestData testData && Helpers.IsTrainingData trainingData) then
                invalidArg "trainingData, testData" "Input of data must be the correct way round"

            if (List.forall (fun feature -> trainingData.Headers.ContainsKey feature ) featuresToRemove) then
                invalidArg "" "All features listed to be removed, must actually exist"

            let rmf =
                

        

            let hdrsTr = trainingData.Headers.Remove(latName).Remove(lngName).Remove(timeTag).Add(dayPhaseColumnName, DataType.Text)
            let hdrsTe =     testData.Headers.Remove(latName).Remove(lngName).Remove(timeTag).Add(dayPhaseColumnName, DataType.Text)

            ({ trainingData with Instances = newTrainingInstances; Headers = hdrsTr }, { testData with Instances = newTestInstances; Headers = hdrsTe })
