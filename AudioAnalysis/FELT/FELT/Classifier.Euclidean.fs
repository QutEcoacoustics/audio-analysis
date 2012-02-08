namespace FELT.Classifiers
    open FELT.Classifiers
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Collections
    open MQUTeR.FSharp.Shared

    type EuclideanClassifier() = class
        inherit ClassifierBase()
        
        let deMap (m: Map<ColumnHeader, Value[]>) = 
            let colsWithNames = Map.toArray m
            let cols = Array.map snd colsWithNames
            // reversing array (i.e. row will be first index)
            let xDim = cols.[0].Length
            let yDim = m.Count
            Array.initJagged xDim yDim  (fun rowIndex colIndex -> cols.[colIndex].[rowIndex]), xDim, yDim

        let distance (a: Value[]) (b: Value[]) = 
            match (a, b) with
                | (IsNumbers nsa, IsNumbers nsb) -> Maths.euclideanDist (unwrap nsa) (unwrap nsb)
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
            let distances = Array.initJagged tedx trdx (fun tedIdx trdIdx -> distance ted.[tedIdx] trd.[trdIdx])

            // now, sort the array, row by row
            // i.e. for each test instance (a row) have in the first column, the closest matched training instance.
            let sortedDistances = Array.init tedx (fun i -> distances.[i]  |> Array.sortWithIndex)

            sortedDistances

        end