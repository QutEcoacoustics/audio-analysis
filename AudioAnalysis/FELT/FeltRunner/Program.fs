namespace FELT.Runner

    module Main =
        (*

        a executable designed to run a FELT comparison

        settings are defined in app.config

        steps:

        1) load data

        2) format data

        3) pre-processors

        4) run classifiers

        5) print results
             results = pre-processors * classifiers

             file format: 
             folder: appSettingsResultsFolder\runStartDate\

        *)


        open MQUTeR.FSharp.Shared
        open MQUTeR.FSharp.Shared.Logger
        open System
        open System.Configuration
        open System.Diagnostics
        open System.IO
        open Linq.QuotationEvaluation
        open System.Reflection
        open FELT.FindEventsLikeThis
        open FELT.Results
        open FELT.Workflows
        open Microsoft.FSharp.Collections
        

        let fail() =
            Error "Exiting because of error!"
            #if DEBUG
            printfn "Debug hook...  press any key to continue..."
            Console.ReadKey(false) |> ignore
            #endif
            Environment.Exit(1);

        let version = Assembly.GetAssembly(typeof<ResultsComputation>).GetName() |> (fun x -> sprintf "%s, %s, %s" x.Name (x.Version.ToString()) x.CodeBase)

        let suggestion() =
            Info "Start: read configuration settings..."

            let config = ConfigurationManager.AppSettings

            // settings
            let ResultsDirectory = config.["ResultsDirectory"]
            let WorkingDirectory = config.["WorkingDirectory"]
            let TestData = WorkingDirectory + config.["TestData"]
            let TrainingData = WorkingDirectory + config.["TrainingData"]
            let exportFrn = bool.Parse(config.["ExportFrn"])
            let exportFrd = bool.Parse(config.["ExportFrd"])
            let allAnalyses = bool.Parse(config.["CrossAnalyseAllFeatures"])
            let allAnalysesLimit = System.Int32.Parse(config.["CrossAnalyseAllFeatures_Limit"])


            // ANALYSIS RUN SETTINGS
            let allKnownAnalyses = FELT.Workflows.Analyses
            let analysesConfig = ConfigurationManager.GetSection("analyses") :?> FELT.Runner.AnalysesConfig
            let analyses = analysesConfig.Analyses |> Seq.cast |> Seq.map (fun (ae:FELT.Runner.Analysis) -> ae.Name, allKnownAnalyses.TryFind(ae.Name) ) |> Seq.toArray

            if (analyses.Length = 0) then
                Error "No analysis set in configuration"
                fail()

            Infof "Analyses sheduled to run: %s" (Seq.fold (fun s (a, _) -> s + a + ", "  ) "" analyses)

            if not (Seq.forall (snd >> Option.isSome) analyses) then
                Error "Invalid analysis found, or name count not be found"
                fail()

            // Transforms settings
            let transform = ConfigurationManager.GetSection("transformations") :?> TransformsConfig
            let transforms = transform.Transformations |> Seq.cast |> Seq.map (fun (tx:TransformElement) -> tx.Features, tx.NewName, tx.Using) |> Seq.toList


            // set up run
            let batchRunDate = DateTime.Now

            let resultsDirectory =
                try
                     Directory.CreateDirectory (ResultsDirectory + batchRunDate.ToString("yyyyMMdd_HHmmss") + "\\")
                with
                    | ex -> 
                        eprintfn "%s" ex.Message
                        fail()
                        null

            let reportDateName (dt: DateTime) e analysis = 
                dt, sprintf "%s\\%s %s.xls%s" resultsDirectory.FullName (dt.ToString "yyyy-MM-dd HH_mm_ss") analysis e

            let logger = Logger.create ((String.Empty |> reportDateName batchRunDate "x" |> snd |> fun x -> new FileInfo(x)).FullName + ".log")

            let reportName analysis = reportDateName DateTime.Now "x" analysis 

            // load in features
            let features = new ResizeArray<string>(config.["Features"].Split(','))

            Info "end: configuration settings..."
            Info "Start: data import..."
            // load data

            let loadAndConvert features filename = 
                let lines = IO.readFileAsString filename
                if lines.IsNone then
                    Errorf "There are no lines to read in %s" filename
                    Option.None
                else
                    lines.Value |> CSV.csvToData features |> Option.Some


            // create data
            let trFile = loadAndConvert features TrainingData 
            let teFile = loadAndConvert features TestData

            Info "end: data import..."

            if trFile.IsNone || teFile.IsNone then
                eprintfn "An error occurred loading one of the data files, exiting..."
                fail()
            else 
                Info "start: main analysis..."

                let trData = { trFile.Value with DataSet = DataSet.Training}
                let teData = { teFile.Value with DataSet = DataSet.Test}

                let analyses', combinations = 
                    // set up the feature combinations
                    let keys = Map.keys trData.Instances |> Set.ofArray
                    let powerset = 
                        if (allAnalyses) then
                            // fst element of powerset is empty... skip
                            let fSets = Set.powerset keys |> Seq.skip 1 |> Seq.toArray |> Array.rev

                            if allAnalysesLimit > 0 && fSets.Length > allAnalysesLimit then
                                Warnf "MAXIMUM NUMBER OF FEATURE COMBINATIONS PRODUCED. %i was capped to %i. This means there will be %i runs instead if %i" fSets.Length allAnalysesLimit (fSets.Length * analyses.Length) (allAnalysesLimit * analyses.Length)
                                Array.sub fSets 0 allAnalysesLimit
                            else
                                fSets
                         else
                            [| keys |]

                    // map every subset to every analysis
                    powerset |> Array.collect (fun s -> Array.map (fun (x,y) -> (x,y,s)) analyses), powerset.Length

                if allAnalyses then
                    Warnf "All selected analyses will be run with EVERY combination of features! %i features, %i combinations, %i analyses, %i runs." trData.Instances.Count combinations analyses.Length analyses'.Length
                else
                    Infof "All selected analyses will be run with all features! %i features, %i analyses, %i runs." trData.Instances.Count combinations analyses'.Length
                
                // run the analyses

                let run (ano: string * (WorkflowItem list) Option * Set<ColumnHeader>) =
                    let analysis, ops, columnsToUse = ano
                    let dt, dest = reportName analysis 
                    let config =
                        {
                            RunDate = dt;
                            AnalysisType = analysis;
                            TestDataBytes = (new FileInfo(TestData)).Length;
                            TrainingDataBytes = (new FileInfo(TrainingData)).Length;
                            ReportDestination = (new FileInfo(dest));
                            ReportTemplate = new FileInfo("ExcelResultsComputationTemplate.xlsx");
                            TestOriginalCount = teData.Classes.Length;
                            TrainingOriginalCount = trData.Classes.Length;
                            ExportFrd = exportFrd;
                            ExportFrn = exportFrn
                        }

                    // re-construct data sets 
                    let trainingData = {trData with Headers = Map.keepThese trData.Headers columnsToUse; Instances = Map.keepThese trData.Instances columnsToUse}
                    let testData     = {teData with Headers = Map.keepThese teData.Headers columnsToUse; Instances = Map.keepThese teData.Instances columnsToUse}

                    Infof "Starting analysis: %s" analysis
                    RunAnalysis trainingData testData (ops.Value) transforms config |> ignore
                    Infof "Analysis %s completed" analysis
                    Warn "Starting post analysis garbage collection"
                    //http://blogs.msdn.com/b/ricom/archive/2004/11/29/271829.aspx
                    System.GC.Collect()
                    Info "Finished post analysis  garbage collection"
                    config

                // actually run them
                let configs = Array.map run analyses'

                Info "end: main analysis..."

                let sumReport = 
                    if configs |> Seq.length > 1 then
                        Log "Creating summary report"
                        let dest = SummationReport.Write (new FileInfo(reportDateName batchRunDate "x" "Summary" |> snd)) (new FileInfo("ExcelResultsSummationTemplate.xlsx")) configs
                        Log "Finished summary report"    
                        Some(dest)
                    else
                        None


                Info "Analysis complete!"
    
                // clear any keystrokes accumulated by accident
                while Console.KeyAvailable do Console.ReadKey(false) |> ignore
    
                Logf "Open %s (y/n)" (if sumReport.IsSome then "summary report" else "report")
    
                let openFile = Console.ReadKey(true);
                Warnf "Key pressed: %c" openFile.KeyChar
 
                if Char.ToLower openFile.KeyChar = 'y' then
                    let f = if sumReport.IsSome then sumReport.Value else (Seq.first configs).ReportDestination
                    Infof "Opening file: %s" f.FullName
                    Process.Start(f.FullName) |> ignore

        let search() =
            raise ( NotImplementedException())
            ()

        let usage() =
            Info "Incorrect paramters given"
            Info "Usage: "
            Info "      xxxxxx.exe [option]"
            Info "Valid options are: suggestion, search"
            ()

        
        [<EntryPoint>]
        let Entry args =
            let ad = AppDomain.CurrentDomain in
                ad.UnhandledException.Add (fun (args:UnhandledExceptionEventArgs) -> Errorf "Unhandled exception:\n%A" args.ExceptionObject)


            Log "Welcome to felt version:"
            Logf "%s" version

            #if DEBUG
            if not Debugger.IsAttached then
                Warn "Debug hook...  press any key to continue..."
                Console.ReadKey(false) |> ignore
            #endif

            // determine analysis to run
            let first = Seq.tryHead args
            let f : unit -> unit = 
                match first with
                    | Some "suggestion" -> 
                        Info "Running suggestion tool."
                        suggestion
                    | Some "search" -> 
                        Info "Running search tool."
                        search
                    | _ -> usage

            f()

            Info "Exiting"
            Console.ReadKey(false) |> ignore
            0





