namespace FELT.Trainers
    open FELT.Trainers
    open System
    open Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared.CSV
    open MQUTeR.FSharp.Shared
    open MathNet.Numerics
    open MQUTeR.FSharp.Shared.StringStats

    /// this implentation overrides the default grouper so it can
    /// attach more statiscal values used in classifiers like the z-score classifier
    type GroupAndKeepStatsTrainer() =
        inherit GroupTrainer()
        
        /// given an array of values to average, average them
        /// this implentation overrides the default grouper so it can
        /// attach more statiscal values used in classifiers like the z-score classifier
        override this.AvgValue (xs:Value array) : Value =
            
            match xs with
            | IsNumbers ns -> 
                let doubles = ns |> Seq.map (MQUTeR.FSharp.Shared.DataHelpers.value)
                let stats = new Statistics.DescriptiveStatistics(doubles)
                upcast (new AveragedNumber(stats))
            | IsTexts ss ->
                failwith "not implemented"
                let ss' = averageStrings ss
                upcast new AverageText(ss' |> head |> fst , ss') 
            | _ -> failwith "not implemented"

        
            
                              
