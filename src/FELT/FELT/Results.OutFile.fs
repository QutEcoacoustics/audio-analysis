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

            let fDest =
                if fileDestination.Exists then
                    new FileInfo(fileDestination.Directory.FullName + "\\" + Path.GetFileNameWithoutExtension(fileDestination.Name)+ (DateTime.Now.ToString "yyyy-MM-dd HH_mm_ss") + fileDestination.Extension)
                else
                    fileDestination
            
            Warnf "Outputting serialized results file to %s" fDest.FullName    

            // side affect
            Serialization.serializeBinaryToFile cacheItem (fDest.FullName)

            ()