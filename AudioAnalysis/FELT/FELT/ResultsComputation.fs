namespace FELT.Results
    open System.Reflection
    open OfficeOpenXml
    open System
    open MQUTeR.FSharp.Shared
    open Microsoft.FSharp.Collections
    open System.IO
    open FELT.Classifiers
    open MQUTeR.FSharp.Shared.IO

    type ReportConfig = 
        {
            RunDate : DateTime
            TestDataBytes: int64
            TrainingDataBytes: int64
            TestOriginalCount: int
            TrainingOriginalCount: int
            ReportDestination: FileInfo
            ReportTemplate: FileInfo
            ExportFrn : bool
            ExportFrd : bool
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
            
            Log "Result computation start"

            let opList = List.rev opList

            let features = 
                if testData.Headers = trainingData.Headers then
                    testData.Headers
                else
                    failwith "The headers in the test and trainng data are the same. This report format does not support different headers for each data set"

            let uniqueTrainingClasses =
               Array.fold (flip Set.add) Set.empty<Class> trainingData.Classes  
            let uniqueTestClasses =
               Array.fold (flip Set.add) Set.empty<Class> testData.Classes  
            
            Log "uniquness a & b"
            
            let uniqueAll = Set.union uniqueTrainingClasses uniqueTestClasses
            
            Log "uniqueness all"

            let tagSummary = 
                [
                    [ config.TrainingOriginalCount ; config.TestOriginalCount; config.TrainingOriginalCount + config.TestOriginalCount ];
                    [trainingData.Classes.Length ; testData.Classes.Length ;  trainingData.Classes.Length + testData.Classes.Length ];
                    [uniqueTrainingClasses.Count; uniqueTestClasses.Count; uniqueAll.Count];
                    [   (Set.difference uniqueTrainingClasses uniqueTestClasses).Count;  
                        (Set.difference uniqueTestClasses uniqueTrainingClasses).Count; 
                        ((Set.difference uniqueTestClasses uniqueAll).Count + (Set.difference uniqueTrainingClasses uniqueAll).Count) ]
                ]

            Log "tag summary"

            let fullResultsTags = Array.Parallel.mapJagged (fun y -> trainingData.Classes.[snd y]) classificationResults

            Log "full results tags"

            let fullResultsDistances = Array.Parallel.mapJagged fst classificationResults

            Log "full results distances"

            let placeFunc rowNum class' row=
                match Array.tryFindIndex ((=) class') row with
                    | Some index -> (rowNum, index + 1 )
                    | None -> (rowNum, 0)
            let placing = Array.Parallel.mapi2 placeFunc testData.Classes fullResultsTags
            
            Log "Placing"

            let placeHistogram =  Seq.countBy (fun x -> snd x) placing
            //System.Diagnostics.Debug.Assert(not (Seq.exists (fst >> ((=) 0)) placeHistogram))

            Log "Histogram"

            let placeSummary =
                let places = [|1 ; 5; 10; 25; 50 |]
                let withinPlace p = Seq.fold (fun total (key, count) -> if key <= p &&  key > 0 then total + count else total) 0 placeHistogram 
                Array.map (fun place -> [place ; withinPlace place]) places

            Log "pl summary"

            let percentileSummary =
                let percentiles = [|0.01; 0.1; 0.2; 0.25; 0.33; 0.5; 0.66; 0.75; 0.9; 1.0|]
                let percentilesAsPlaces = Array.map (fun x -> x , int( Math.Round(x * double trainingData.Classes.Length))) percentiles
                let numCoveredByPlace p = Seq.fold (fun total  (key, count) -> if key <= p && key > 0 then total + count else total) 0 placeHistogram 
                Array.map (fun (percentile:float,place) -> [percentile; float (numCoveredByPlace place)]) percentilesAsPlaces
                
            Log "pe summary"
            
            // create excel package
            let report = new ExcelPackage(config.ReportDestination, config.ReportTemplate)
            let workbook = report.Workbook
            let logws =  workbook.Worksheets.["Log"]
            let sumResults = workbook.Worksheets.["Summary Results"]
            let fullResults = workbook.Worksheets.["Full Results"]
            let fullResultsDist = workbook.Worksheets.["Full Results Distances"]
            
            Log "report opened"
            
            let names = workbook.Names
            let setv (ws:ExcelWorksheet) name value =
                ws.SetValue(names.[name].Address, value)
            let setHorz (ws:ExcelWorksheet) name values f =
                let fdc = names.[name].Start
                Seq.iteri (fun index op -> ws.SetValue(fdc.Row, fdc.Column + index, f op)) values
            let setVert (ws:ExcelWorksheet) name values f =
                let fdc = names.[name].Start
                Seq.iteri (fun index op -> ws.SetValue(fdc.Row + index, fdc.Column, f op)) values
            let setSquare (ws:ExcelWorksheet) name (values: seq<#seq<'d>>) =
                let sc = names.[name].Start
                // race condition when parallelizing, set out bounds first...
                //ws.Cells.[Seq.length values, Seq.length (Seq.nth 0 values)].Value <- "A value"
//                let vs = PSeq.map (fun row -> row |> Seq.map (fun z -> box z) |> Array.ofSeq) values
//                ws.Cells.[sc.Address].LoadFromArrays vs
                //PSeq.iteri (fun indexi row ->  ws.Cells.[sc.Offset(indexi, 0).Address].LoadFromCollection(row) |> ignore) values
                Seq.iteri (fun indexi -> Seq.iteri (fun  indexj value -> ws.SetValue(sc.Row + indexi, sc.Column + indexj, value))) values
                

            Log "log sheet"


            // set
            setv logws "RunDate" (config.RunDate.ToString("dddd, dd MMMM yyyy HH:mm:ss"))

            setv logws "Version" (version) 

            setv logws "TrBytes" config.TrainingDataBytes
            setv logws "TeBytes" config.TestDataBytes

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
            names.["PlacementSummary"].Offset(0,2, placeSummary.Length, 1).FormulaR1C1 <- "RC[-1]/Log!$C$15"
            setSquare logws "PercentileSummary" percentileSummary


            Log "summary results"

            // summary results worksheet
            setSquare sumResults "srPlaces" (placeHistogram |> Seq.map (fun x -> [fst x ; snd x]) |> Seq.sort)
            names.["srPlaces"].Offset(0,2, Seq.length placeHistogram, 1).FormulaR1C1 <- "RC[-1]/Log!$C$15"
            names.["srPlaces"].Offset(1,3, (Seq.length placeHistogram) - 1, 1).FormulaR1C1 <- "SUM(R3C3:RC[-1])"

            if config.ExportFrn then
                Log "Full results"

                // full results
                setVert fullResults "frClasses" testData.Classes id
                setv fullResults "frNumbers" 1

                Log "frn a"

                names.["frNumbers"].Offset(0, 1, 1, trainingData.Classes.Length - 1).FormulaR1C1 <- countUpFormula
            
                Log "frn b"

                setSquare fullResults "frData" fullResultsTags
                Log "frn c"
                let plc = names.["frPlaces"].Offset(0,0, testData.Classes.Length, 1)
                Log "frn d"
                plc.FormulaR1C1 <- "MATCH(RC[-1],RC[1]:RC[" + trainingData.Classes.Length.ToString() + "],0)"
            else
                Info "Not exporting full results"

            if config.ExportFrd then

                Log "full results distances"

                // full results (distances)
                setVert fullResultsDist "frdClasses" testData.Classes id
                setv fullResultsDist "frdNumbers" 1
            
                Log "frd a"

                names.["frdNumbers"].Offset(0, 1, 1, trainingData.Classes.Length - 1).FormulaR1C1 <- countUpFormula
            
                Log "frd b"

                setSquare fullResultsDist "frdData" fullResultsDistances

                Log "write time taken"
            else
                Info "Not exporting full results distances"


            setv logws "TimeTaken" ((DateTime.Now - config.RunDate).ToString())
            setv logws "MemUsage" Environment.WorkingSet

            Log "write file"

            report.Save()
            report.Dispose()

            Log "done"

            ()
        end
