namespace FELT.Classifiers
    open FELT.Classifiers
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared
    open MQUTeR.FSharp.Shared.DataHelpers
    open System.Diagnostics

    type ZScoreClassifier() =
        inherit ClassifierBase()

        let zScore sample avg =
            match sample with
             | IsNumber n ->
                match avg with
                    | IsAvgNumber avg -> Maths.zscore n.Value avg.DescriptiveStatistics.Mean avg.DescriptiveStatistics.StandardDeviation
                    | _ -> failwith "avgerage against types "
             | _ -> failwith "other data types not yet supported"

        let distance samples avgs =
            // calculate z-score for each feature
            let zss = Seq.map2 (zScore) samples avgs

            // combine score
            Maths.euclideanDist zss (Array.create (Seq.length zss) 0.0)
        
        
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
            let testDataInstanceCount = (Map.getNthValue trainingData.Instances 0).Length
            let trainingDataInstanceCount = (Map.getNthValue trainingData.Instances 0).Length


            let distances = Array.Parallel.initJagged testDataInstanceCount trainingDataInstanceCount (fun tedIdx trdIdx -> distance (getRow tedIdx testData) (getRow trdIdx trainingData) )

            // now, sort the array, row by row
            // i.e. for each test instance (a row) have in the first column, the closest matched training instance.
            let sortedDistances = Array.Parallel.init (distances.Length) (fun i -> 
                                                                    let sortedRow = distances.[i]  |> Array.sortWithIndex
                                                                    // an attempt at disposing unecessary data to increace mem performance
                                                                    distances.[i] <- null
                                                                    sortedRow)

            sortedDistances