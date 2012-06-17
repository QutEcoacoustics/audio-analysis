namespace Felt.Results

    open System.Reflection
    open System
    open MQUTeR.FSharp.Shared
    open MQUTeR.FSharp.Shared.Maths
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core
    open System.IO
    open FELT.Classifiers
    open MQUTeR.FSharp.Shared.IO
    open FELT.Results
    

    module ResultsOutFile =
        
        let Output (config:ReportConfig) (fileDestination: FileInfo) trainingData extraDataStore : unit =
            let assembly = Assembly.GetAssembly(typeof<ResultsComputation>).GetName() 
          

            let cacheItem =
                {
                    RunDate = config.RunDate;
                    SaveDate = DateTime.Now;
                    AnalysisType = config.AnalysisType

                    TrainingDataBytes= config.TrainingDataBytes;

                    TrainingOriginalCount = config.TrainingOriginalCount;

                    CachedData = trainingData;
                    ExtraData = extraDataStore;
                    Assembly = assembly;
                }

            // side affect
            Serialization.serializeBinaryToFile cacheItem (fileDestination.FullName)

            ()