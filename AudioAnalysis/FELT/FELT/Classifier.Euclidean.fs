namespace FELT.Classifiers
    open FELT.Classifiers
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared
    open System.Diagnostics
    

    type EuclideanClassifier() = class
        inherit ClassifierBase()

        let deMap (m: Map<ColumnHeader, Value[]>) = 
            let colsWithNames = Map.toArray m
            let cols = Array.map snd colsWithNames
            // reversing array (i.e. row will be first index)
            let xDim = cols.[0].Length
            let yDim = m.Count
            Array.Parallel.initJagged xDim yDim  (fun rowIndex colIndex -> cols.[colIndex].[rowIndex]), xDim, yDim


        let distance (a: Value[]) (b: Value[]) = 
            // collapse non-numeric representations down to floats

            match (a, b) with
                | (IsAnyNumbers nsa, IsAnyNumbers nsb) -> Maths.euclideanDist nsa nsb
                | _ -> failwith "Not implemented for types other than number" 
            

        override this.Classify (trainingData, testData) =

            let colCount = trainingData.Instances.Count
            if colCount <> testData.Instances.Count then
                failwith "Input data sets must have same number of columns"

            // for each vector (t) in test data (td)
                // measure distance to each vector (s) in training data (sd)

            // we dont need column names so much for this
            let trd, trdx, trdy = deMap trainingData.Instances
            let ted, tedx, tedy = deMap testData.Instances

            // WARNING: BIG OP, we need to use jagged arrays here because not enough memory for 2d array
            // distances is an cross-joined array of test and training intances, e.g.:
            // [ [ d(t1-s1); d(t1-s2) ]
            //   [ d(t2-s1); d(t2-s2) ] ]
            let distances = Array.Parallel.initJagged tedx trdx (fun tedIdx trdIdx -> distance ted.[tedIdx] trd.[trdIdx])

            Info "Euclidean Classifier - Distances complete"

            // now, sort the array, row by row
            // i.e. for each test instance (a row) have in the first column, the closest matched training instance.
            let sortedDistances = Array.Parallel.init tedx (fun i -> 
                                                                    let sortedRow = distances.[i]  |> Array.sortWithIndex
                                                                    // an attempt at disposing unecessary data to increace mem performance
                                                                    distances.[i] <- null
                                                                    sortedRow)
            
            Info "Euclidean Classifier - Sorting complete"

            Warn "Starting Euclidean Classifier garbage collection"
            //http://blogs.msdn.com/b/ricom/archive/2004/11/29/271829.aspx
            System.GC.Collect()
            Info "Finished Euclidean Classifier garbage collection"

            sortedDistances

        end