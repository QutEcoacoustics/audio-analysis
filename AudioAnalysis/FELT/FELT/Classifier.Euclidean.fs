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
            Array2D.init cols.[0].Length m.Count (fun rowIndex colIndex -> cols.[colIndex].[rowIndex])

        let distance (a: Value[]) (b: Value[]) = 
            match (a, b) with
                | (IsNumbers nsa, IsNumbers nsb) -> Maths.euclideanDist (unwrap nsa) (unwrap nsb)
                | _ -> failwith "Not implemented for types other than number" 
            

        override this.Classify (trainingData, testData) =

            let colCount = trainingData.Instances.Count
            if colCount = testData.Instances.Count then
                failwith "Input data sets must have same number of columns"

            // for each vector (t) in test data (td)
                // measure distance to each vector (s) in training data (sd)

            // we dont need column names so much for this
            let trd = deMap trainingData.Instances
            let ted = deMap testData.Instances
              
            let distances = Array2D.init (Array2D.length1 trd) (Array2D.length1 ted) (fun tedIdx trdIdx -> distance (Array2D.getRow tedIdx ted) (Array2D.getRow trdIdx trd))



            new obj()

        end