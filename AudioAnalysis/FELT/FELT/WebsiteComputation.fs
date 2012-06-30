namespace FELT.Results

    open System.Reflection
    open OfficeOpenXml
    open System
    open MQUTeR.FSharp.Shared
    open MQUTeR.FSharp.Shared.Maths
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core
    open System.IO
    open FELT.Classifiers
    open MQUTeR.FSharp.Shared.IO


    type WebsiteComputation(config:CacheFormat) = class
        let OnePlace:Place = 1
        

        /// (opList: (string * string * string) list)
        member this.Calculate (trainingData:Data) (testData:Data) (classificationResults:ClassifierResult)  limit =
            if testData.Classes.Length > 1 then
                invalidArg "testData" "only one test element supported!"
            if limit <= 0 || limit > trainingData.Classes.Length then
                invalidArg "limit" "limit most be greater than zero and no more than the number of training classes"
            
            let f = 
                match classificationResults with
                    | ClassifierResult.Function f -> f
                    | _ -> invalidArg "classificationResults" "Only lazy evaulation supported!"

            let d = new System.Collections.Generic.SortedDictionary<int, Class>()
            
            let getClass y = trainingData.Classes.[snd y]
            
            // sorted results - note this returns a full list of places (trainingData.Length)
            // TODO: optimize - only return "limit" results
            let singleRow = f 0 // only one test item

            // stick value into dictionary - ignore distance value for now
            Array.iteri (fun index result -> d.Add(index, result |> getClass)) singleRow

            d

        end