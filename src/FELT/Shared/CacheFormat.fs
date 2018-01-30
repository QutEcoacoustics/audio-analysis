namespace MQUTeR.FSharp.Shared
    
    open System
    open System.Reflection

    type CacheFormat =
        {
            RunDate : DateTime
            SaveDate: DateTime
            AnalysisType : string

            TrainingDataBytes: int64

            TrainingOriginalCount: int

            CachedData : Data
            ExtraData : Map<string, obj>
            Assembly : AssemblyName
        }
