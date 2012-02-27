namespace FELT.Trainers
    open FELT.Trainers
    open System
    open Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared.CSV
    open MQUTeR.FSharp.Shared
    open MathNet.Numerics
    open MQUTeR.FSharp.Shared.StringStats

    type SingleInstanceBehaviour =
        | Leave = 0
        | Merge = 1
        

    /// this implentation overrides the default grouper so it can
    /// attach more statiscal values used in classifiers like the z-score classifier
    type GroupAndKeepStatsTrainer(singleInstanceBehaviour:SingleInstanceBehaviour) =
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

        
        override this.PostProcess instances =
            match singleInstanceBehaviour with
            | SingleInstanceBehaviour.Leave -> instances
            | SingleInstanceBehaviour.Merge ->
                // in this case we want to ensure no single group is without a standard deviation.
                // to do this, we scan though all other means and find the closest, within the same feature
                // then we "borrow" its deviation, keeping the other values
                
                // we assume all columns have same number of values
                let unwrapped = Map.toArray instances |> Array.map snd
                let numGroups = unwrapped |> Seq.nth 1 |> Array.length

                let checkForErrorAndFix vs (v:AveragedNumber) =
                    if (v.DescriptiveStatistics.Count = 1) then
                        // fix
                        let f testIndex (idealIndex, diff) (testVal:AveragedNumber) =
                            if testVal.DescriptiveStatistics.Count = 1 then
                                // ignore
                                (idealIndex, diff)
                            else
                                let meanDelta = testVal.DescriptiveStatistics.Mean - v.DescriptiveStatistics.Mean
                                if meanDelta < diff then
                                    (testIndex, meanDelta)
                                else 
                                    (idealIndex, diff)

                        let indexToTake, _ = Array.foldi (f) (-1, System.Double.MaxValue) vs
                        let newStdDev = vs.[indexToTake].DescriptiveStatistics.StandardDeviation
                        new AveragedNumber(v.DescriptiveStatistics, newStdDev)
                    else
                        v
                
                let fixSingleCases (vs:Value array) =
                    let vs' = (testAndCastArray<AveragedNumber> vs).Value
                    // check each value (group)
                    Array.map (checkForErrorAndFix vs') vs'

                // since we are not adding or removing rows, we can scan columnwise
                let corrected = Array.map (fixSingleCases) unwrapped

                instances
            | _ -> failwith "not implemented"
            
                              
