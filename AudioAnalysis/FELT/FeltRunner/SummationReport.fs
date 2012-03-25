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

        

        /// warning this class by default involves a lot of mutation and intrinsically causes side-affects
        let Write (dest: FileInfo) (template: FileInfo) (configs: seq<ReportConfig>) =
            
            // set up results
            let fileNames = Seq.map (fun c -> c.ReportDestination) configs
            let analyses =  Seq.map (fun c -> c.AnalysisType) configs

            let allKnownFeatureNames = [""]


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

            setHorz summaryws "Features" allKnownFeatureNames id


            Log "end summary sheet"

            // save file
            Log "write file"

            report.Save()
            report.Dispose()

            Log "done"

            dest


