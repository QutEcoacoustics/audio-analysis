namespace FELT.Results
    open System.Reflection
    open OfficeOpenXml
    open System
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Collections
    open System.IO
    open FELT.Classifiers

    type ReportConfig = 
        {
            RunDate : DateTime
            TestDataBytes: int64
            TrainingDataBytes: int64
            ReportDestination: FileInfo
            ReportTemplate: FileInfo
        }

    type ResultsComputation(config:ReportConfig) = class

        let countUpFormula = "RC[-1] + 1"
        let version = Assembly.GetAssembly(typeof<ResultsComputation>).GetName() |> (fun x -> sprintf "%s, %s, %s" x.Name (x.Version.ToString()) x.CodeBase)

        // warning this class by default involves a lot of mutation and intrinsically causes side-affects
        member this.Calculate (trainingData:Data) (testData:Data) (classificationResults: Result[]) opList =
            

            (* have to do lots of stuff in this class.
                - ✔ ensure output directory exists
                
                - output excel file
                    - ✔ results summary
                        - 
                    - ✔ major results (CSV files) as work sheets
                    - Graphs
                        - positional summary pie chart
                        - distributions graph
                        - species composition
            *)

            // prepare
            

            let features = 
                if testData.Headers = trainingData.Headers then
                    testData.Headers
                else
                    failwith "The headers in the test and trainng data are the same. This report format does not support different headers for each data set"

            let uniqueTrainingClasses =
               Array.fold (flip Set.add) Set.empty<Class> trainingData.Classes  
            let uniqueTestClasses =
               Array.fold (flip Set.add) Set.empty<Class> trainingData.Classes  
            let uniqueAll = Set.union uniqueTrainingClasses uniqueTestClasses
            let tagSummary = 
                [
                    [trainingData.Classes.Length ; testData.Classes.Length ;  trainingData.Classes.Length + testData.Classes.Length ];
                    [uniqueTrainingClasses.Count; uniqueTestClasses.Count; uniqueAll.Count];
                    [   (Set.difference uniqueTrainingClasses uniqueTestClasses).Count;  
                        (Set.difference uniqueTestClasses uniqueTrainingClasses).Count; 
                        ((Set.difference uniqueTestClasses uniqueAll).Count + (Set.difference uniqueTrainingClasses uniqueAll).Count) ]
                ]

            let fullResultsTags = Array.map (Array.map (fun y -> trainingData.Classes.[snd y])) classificationResults

            let fullResultsDistances = Array.map (Array.map (fun y -> fst y)) classificationResults 

            let placeFunc rowNum class' row=
                match Array.tryFindIndex ((=) class') row with
                    | Some index -> (rowNum, index + 1 )
                    | None -> (rowNum, 0)
            let placing = Array.mapi2 placeFunc testData.Classes fullResultsTags

            let placeHistogram =  Seq.countBy (fun x -> snd x) placing
            let placeSummary =
                let places = [1 ; 5; 10; 25; 50 ]
                List.map (fun place -> [place ; Seq.fold (fun total x -> if place < snd x then total + (snd x) else total) 0 placeHistogram ]) places
            let percentileSummary =
                let percentiles = [0.01; 0.1; 0.2; 0.25; 0.33; 0.5; 0.66; 0.75; 0.9; 1.0]
                let percentilesAsPlaces = List.map (fun x -> x , int( Math.Round(x * double trainingData.Classes.Length))) percentiles
                List.map (fun place -> [fst place ; Seq.fold (fun total x -> if snd place < fst x then total + (snd x |> float) else total) 0.0 placeHistogram ]) percentilesAsPlaces
                

            
            // create excel package
            let report = new ExcelPackage(config.ReportDestination, config.ReportTemplate)
            let workbook = report.Workbook
            let logws =  workbook.Worksheets.["Log"]
            let sumResults = workbook.Worksheets.["Summary Results"]
            let fullResults = workbook.Worksheets.["Full Results"]
            let fullResultsDist = workbook.Worksheets.["Full Results Distances"]

            let names = workbook.Names
            let setv (ws:ExcelWorksheet) name value =
                ws.SetValue(names.[name].Address, value)
            let setHorz (ws:ExcelWorksheet) name values f =
                let fdc = names.[name].Start
                Seq.iteri (fun index op -> ws.SetValue(fdc.Row, fdc.Column + index, f op)) values
            let setVert (ws:ExcelWorksheet) name values f =
                let fdc = names.[name].Start
                Seq.iteri (fun index op -> ws.SetValue(fdc.Row + index, fdc.Column, f op)) values
            let setSquare (ws:ExcelWorksheet) name values =
                let sc = names.[name].Start
                Seq.iteri (fun indexi -> Seq.iteri (fun  indexj value -> ws.SetValue(sc.Row + indexi, sc.Column + indexj, value))) values

            
            

            // set
            
            setv logws "RunDate" (config.RunDate.ToString("R"))

            setv logws "Version" (version) 

            setv logws "TrBytes" config.TestDataBytes
            setv logws "TeBytes" config.TrainingDataBytes

            // set op list
            setHorz logws "AlgorithmType" opList fst
            setHorz logws "AlgorithmName" opList snd
            
            // set feature list
            setHorz logws "FeatureDataTypes" features Map.getValue
            setHorz logws "FeatureNames" features Map.getKey

            // set TagSummary
            setSquare logws "TagSummary" tagSummary

            // results summary
            setSquare logws "PlacementSummary" placeSummary
            names.["PlacementSummary"].Offset(0,2, placeSummary.Length, 1).FormulaR1C1 <- "RC[-1]/Log!C15"
            setSquare logws "PercentileSummary" percentileSummary

            // summary results worksheet
            setSquare sumResults "srPlaces" (placeHistogram |> Seq.map (fun x -> [fst x ; snd x]) |> Seq.sort)
            names.["srPlaces"].Offset(0,2, Seq.length placeHistogram, 1).FormulaR1C1 <- "RC[-1]/Log!C15"
            names.["srPlaces"].Offset(0,3, Seq.length placeHistogram, 1).FormulaR1C1 <- "SUM(R2C3:RC[-1])"

            // full results
            setVert fullResults "frClasses" testData.Classes id
            setv fullResults "frNumbers" 1
            names.["frNumbers"].Offset(0, 1, 1, trainingData.Classes.Length - 1).FormulaR1C1 <- "RC[-1] - 1"
            setSquare fullResults "frData" fullResultsTags

            let plc = names.["frPlaces"].Offset(0,0, testData.Classes.Length, 1)
            plc.FormulaR1C1 <- "MATCH(RC[-1],RC[1]:RC[" + trainingData.Classes.Length.ToString() + "],0)"

            // full results (distances)
            setVert fullResultsDist "frdClasses" testData.Classes id
            setv fullResultsDist "frdNumbers" 1
            names.["frdNumbers"].Offset(0, 1, 1, trainingData.Classes.Length - 1).FormulaR1C1 <- "RC[-1] + 1"
            setSquare fullResultsDist "frdData" fullResultsDistances

            setv logws "TimeTaken" ((DateTime.Now - config.RunDate).ToString())

            

            report.Save()
            report.Dispose()

            ()
        end
