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


    type ResultsComputation(config:ReportConfig) = class
        let OnePlace:Place = 1
        


        

        /// warning this class by default involves a lot of mutation and intrinsically causes side-affects
        member this.Calculate (trainingData:Data) (testData:Data) (classificationResults:ClassifierResult) (opList: (string * string * string) list) =

            3.0


        end