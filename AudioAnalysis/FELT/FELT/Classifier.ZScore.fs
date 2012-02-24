namespace FELT.Classifiers
    open FELT.Classifiers
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared
    open MQUTeR.FSharp.Shared.DataHelpers
    open System.Diagnostics

    type ZScoreClassifier() =
        inherit ClassifierBase()

        let zScore (sample:Value) (avg:Value) =
            let s = sample
            let a = avg
            match s with
             | IsNumber n ->
                match a with
                    | IsAvgNumber aavg -> Maths.zscore n.Value aavg.DescriptiveStatistics.Mean aavg.DescriptiveStatistics.StandardDeviation
                    | _ -> failwith "avgerage against types "
             | _ -> failwith "other data types not yet supported"

        let distance samples avgs =
            // calculate z-score for each feature
            let ss =  Seq.toArray samples
            let avs = Seq.toArray avgs
            let zss = Array.map2 (zScore) ss avs

            // combine score
            let d = Maths.euclideanDist zss (Array.create (Seq.length zss) 0.0)
            d
        
        
        override this.Classify (trainingData, testData) =

            let colCount = trainingData.Instances.Count
            if colCount <> testData.Instances.Count then
                failwith "Input data sets must have same number of columns"
            

            // for each vector (t) in test data (td)
                // measure distance to each vector (s) in training data (sd)
                    // do so by taking each element of t (ti) and calculating a z-score from si
                        // e.g. zScore = (ti - si.mean) / si.stdDeviation
                            // where i is a feature index
                    // then with all z-scores compute one score by combining through euclidean distance
                // with all combined z-scores, sort and return

            
            let headersMap = Map.keys trainingData.Instances
            Debug.Assert((headersMap = Map.keys testData.Instances))

            // we assume our jagged data arrays are square
            let testDataInstanceCount = (Map.getNthValue testData.Instances 0).Length
            let trainingDataInstanceCount = (Map.getNthValue trainingData.Instances 0).Length


            let distances = Array.initJagged testDataInstanceCount trainingDataInstanceCount (fun tedIdx trdIdx -> distance (getRow tedIdx testData) (getRow trdIdx trainingData) )

            // now, sort the array, row by row
            // i.e. for each test instance (a row) have in the first column, the closest matched training instance.
            let sortedDistances = Array.Parallel.init (distances.Length) (fun i -> 
                                                                    let sortedRow = distances.[i]  |> Array.sortWithIndex //By (fun (v, index) -> abs(v))
                                                                    // an attempt at disposing unecessary data to increace mem overhead
                                                                    distances.[i] <- null
                                                                    sortedRow)

            sortedDistances