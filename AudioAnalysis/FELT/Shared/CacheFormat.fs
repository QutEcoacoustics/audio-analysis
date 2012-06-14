namespace MQUTeR.FSharp.Shared
    
    open System

    type CacheFormat =
        {
            RunDate : DateTime
            SaveDate: DateTime
            AnalysisType : string

            TrainingDataBytes: int64

            TrainingOriginalCount: int

            CachedData : Data
            Assembly: Version
        }
