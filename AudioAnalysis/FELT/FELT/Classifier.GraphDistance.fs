namespace FELT.Classifiers
    open FELT.Classifiers
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Collections
    open QuickGraph
    open System
    open System.Diagnostics
    open System.Collections.Generic
    open System.Linq
    open QuickGraph.Algorithms


    type GraphDistance(selector : (ColumnHeader * IGraph<'v,IEdge<'v>>) array) =
        inherit ClassifierBase()
        
        // a lazy classifier for returning the distance between object on a known graph
        override this.Classify trainingData testData =
      
            // first check that training and test data contain same columns
            FELT.Helpers.headersMatch trainingData testData

            // then use selector from constructor to return columns that are relevant
            let featuresToUse, graphs = selector |> Array.unzip

            // for simplicities sake, only work with one thing atm
            let singleFeature = featuresToUse.Single()
            let singleGraph = graphs.Single()

            // of the columns that are relevant,
            let measureRow testItem trainedItems = 
                // use the graph supplied for each one (from constructor)
                // hACK: for use only with UndirectedGraphs - generalise later
                let specficGraphType = singleGraph :?> IUndirectedGraph<'v, IUndirectedEdge<'v>>

                // and measure the distance from training node to test node
                // this library creates a set of delegates for measuring to a node, so most efficient way is to create each search
                // delegate for each 'test' row
                // note: the lambda there returns evenly weighted edges between verticies
                let distanceDelegate = specficGraphType.ShortestPathsDijkstra((fun _ -> 1.0), testItem)

                let distances =
                    Array.map (fun trainedItem -> 
                            let success, path = distanceDelegate.Invoke(trainedItem)
                            Debug.Assert(success)
                            float <| path.Count()) trainedItems

                // sort results
                let sortedDistances = Array.sortWithIndex distances

                sortedDistances
            

            
            // pick values
            let pick i = 
                let trainingValues = 
                    trainingData.Instances.[singleFeature]
                    |> function | IsTexts ts -> ts | _ -> failwith "unexpected data type"
                let testValue = 
                    testData.Instances.[singleFeature].[i] 
                    |> function | IsText t -> t | _ -> failwith "unexpected data type"
            
                testValue, trainingValues
            
            // return results
            let f i = i |> pick ||> measureRow

            ClassifierResult.Function f