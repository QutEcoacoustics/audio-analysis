namespace FELT.Runner
    module SummationReport =
        
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
        open FELT.Results
        open FELT.Results.EpPlusHelpers

        type info =
            {
                features : seq<string>
                sensitivities : seq<obj>
                accuracies: seq<obj>
                roc : obj
                time : obj
                memory : obj
            }
        

        /// warning this class by default involves a lot of mutation and intrinsically causes side-affects
        let Write (dest: FileInfo) (template: FileInfo) (configs: seq<ReportConfig>) =
            
            // set up results
            let numRows = Seq.length configs
            let fileNames = Seq.map (fun c -> c.ReportDestination) configs
            let analyses =  Seq.map (fun c -> c.AnalysisType) configs

            let gatherInfo (file:FileInfo) : info =
                use file = new ExcelPackage(file)
                let wb = file.Workbook
                let logws = wb.Worksheets.["Log"]
                let namesStart = wb.Names.["FeatureNames"]                
                let placesStart = wb.Names.["PlacementSummary"] 

                let fn  = 
                    let initial = namesStart.Start.Column, []
                    let test (x,_) = 
                        let v = logws.Cells.[namesStart.Start.Row, x].Value
                        if isNull v then
                            false
                        else
                            v.ToString() |> String.IsNullOrWhiteSpace |> not
                    
                    let action (col, list) = col + 1 , (logws.Cells.[namesStart.Start.Row, col].Value.ToString()) :: list
                    whilerec initial test action |> snd
                let positives, negatives =
                    wb.Names.["RocPositives"].Value |> nullToFloat, wb.Names.["RocNegatives"].Value.ToString() |> nullToFloat |> float
                let ss, ass =
                    let initial = placesStart.Start.Row, Seq.empty<Count>
                    let test (x,_) = 
                        let v = logws.Cells.[x, placesStart.Start.Column].Value
                        if isNull v then
                            false
                        else
                            v.ToString() |> String.IsNullOrWhiteSpace |> not
                    
                    let action (row, list) = 
                        let v = int << nullToFloat <| (logws.Cells.[row, placesStart.Start.Column + 1]).Value  
                        row + 1 , list +. v
                    let r = whilerec initial test action |> snd
                    Seq.map (fun  c -> box <| float c / (positives)) r, Seq.map (fun  c -> box <| float c / (positives + negatives)) r
                let roc, timeTaken, memory =
                    wb.Names.["ModifiedAUC"].Value |> nullToFloat, wb.Names.["TimeTaken"].Value |> nullToString, wb.Names.["MemUsage"].Value |> nullToFloat
                {features = fn; sensitivities = ss; accuracies = ass; roc = roc; time = timeTaken; memory = memory}

            let allKnownInformation = Seq.map gatherInfo fileNames
            let allKnownFeatures =  
                    allKnownInformation  
                    |> Seq.map (fun x -> x.features) 
                    |> Seq.concat 
                    |> Set.ofSeq
                
                

            // open file
            Log "Report creation"

            let report = new ExcelPackage(dest, template)
            let workbook = report.Workbook
            let summaryws =  workbook.Worksheets.["Summary"]
            let performancews = workbook.Worksheets.["Performance"]

            // write data
            Log "summary sheet"
            setCellVert summaryws "Filenames" fileNames (fun cell fn -> cell.Hyperlink <- new Uri(fn.FullName); cell.Value <- fn.Name)

            setVert summaryws "AnalysisNames" analyses id
            setVert summaryws "Filenames" fileNames (fun fi -> fi.Name)

            setHorz summaryws "Features" allKnownFeatures id

//            // fill feature matrix formulas down
//            let featureCell = workbook.Names.["SummaryGrid"]
//            let featureRow = summaryws.Cells.[featureCell.Start.Row, featureCell.Start.Column, featureCell.Start.Row, featureCell.Start.Column + (Seq.length allKnownFeatureNames)]
//            fillDown summaryws featureRow (numRows - 1)


            let matchFeatures =  (=) >> Seq.tryFind >< allKnownFeatures >> Option.isSome >> ifelse 1 0
            setSquare summaryws "SummaryGrid" <| Seq.map (fun info -> Seq.map matchFeatures info.features) allKnownInformation 

            Log "end summary sheet"
            Log "performances sheet"

            // This sheet partly automatic, so fill down formulas in those columns
////            let width = 22 // performancews.Dimension.End.Column
////            let startRow = workbook.Names.["DataStartRow"]
////            
////            let templateRange = performancews.Cells.[startRow.Start.Row, startRow.Start.Column, startRow.Start.Row, width]
////            let templateRangeArray = performancews.Cells.[startRow.Start.Row, startRow.Start.Column + width - 1, startRow.Start.Row + numRows, width + 1]
////            fillDown performancews templateRange (numRows - 1)
////            

            // now set values
            let vs =
                Seq.map (fun info -> info.roc .+ info.accuracies ++ info.sensitivities +. info.time +. info.memory) allKnownInformation
            setSquare performancews "ResultsTable" vs

            Log "end performances sheet"
            // save file
            Log "write file"

            report.Save()
            report.Dispose()

            Log "done"

            dest


