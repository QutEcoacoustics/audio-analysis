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
        

        let fail() =
            eprintfn "Exiting because of error!"
            #if DEBUG
            printfn "Debug hook...  press any key to continue..."
            Console.ReadKey(false) |> ignore
            #endif
            Environment.Exit(1);

        let version = Assembly.GetAssembly(typeof<ResultsComputation>).GetName() |> (fun x -> sprintf "%s, %s, %s" x.Name (x.Version.ToString()) x.CodeBase)



        Log "Welcome to felt version:"
        Logf "%s" version

        #if DEBUG
        Warn "Debug hook...  press any key to continue..."
        Console.ReadKey(false) |> ignore
        #endif

        Info "Start: read configuration settings..."

        let config = ConfigurationManager.AppSettings

        // settings
        let ResultsDirectory = config.["ResultsDirectory"]
        let WorkingDirectory = config.["WorkingDirectory"]
        let TestData = WorkingDirectory + config.["TestData"]
        let TrainingData = WorkingDirectory + config.["TrainingData"]
        let exportFrn = bool.Parse(config.["ExportFrn"])
        let exportFrd = bool.Parse(config.["ExportFrd"])

        // ANALYSIS RUN SETTINGS
        let allKnownAnalyses = FELT.Workflows.Analyses
        let analysesConfig = ConfigurationManager.GetSection("analyses") :?> FELT.Runner.AnalysesConfig
        let analyses = analysesConfig.Analyses |> Seq.cast |> Seq.map (fun (ae:FELT.Runner.Analysis) -> ae.Name, allKnownAnalyses.TryFind(ae.Name) ) |> Seq.toList

        if (analyses.Length = 0) then
            Error "No analysis set in configuration"
            fail()

        Infof "Analyses sheduled to run: %s" (Seq.fold (fun s (a, _) -> s + a + ", "  ) "" analyses)

        if not (Seq.forall (snd >> Option.isSome) analyses) then
            Error "Invalid analysis found, or name count not be found"
            fail()

        // Transforms settings
        let transform = ConfigurationManager.GetSection("transformations") :?> TransformsConfig
        let transforms = transform.Transformations |> Seq.cast |> Seq.map (fun (tx:TransformElement) -> tx.Feature, tx.NewName, tx.Using) |> Seq.toList


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

        let reportDateName (dt: DateTime) analysis = 
            dt, sprintf "%s\\%s %s.xlsx" resultsDirectory.FullName (dt.ToString "yyyy-MM-dd HH_mm_ss") analysis

        let logger = Logger.create ((String.Empty |> reportDateName batchRunDate |> snd |> fun x -> new FileInfo(x)).FullName + ".log")

        let reportName analysis = reportDateName DateTime.Now analysis

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

            // run the analyses

            let run (ano: string * (WorkflowItem list) Option) =
                let analysis, ops = ano
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

                Infof "Starting analysis: %s" analysis
                RunAnalysis trData teData (ops.Value) transforms config |> ignore
                Infof "Analysis %s completed" analysis
                Warn "Starting post analysis garbage collection"
                //http://blogs.msdn.com/b/ricom/archive/2004/11/29/271829.aspx
                System.GC.Collect()
                Info "Finished post analysis  garbage collection"
                config

            // actually run them
            let configs = List.map run analyses

            Info "end: main analysis..."

            let sumReport = 
                if configs.Length > 1 then
                    Log "Creating summary report"
                    let dest = SummationReport.Write (new FileInfo(reportDateName batchRunDate "Summary" |> snd)) (new FileInfo("ExcelResultsComputationTemplate.xlsx")) configs
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
                let f = if sumReport.IsSome then sumReport.Value else configs.Head.ReportDestination
                Infof "Opening file: %s" f.FullName
                Process.Start(f.FullName) |> ignore

            Info "Exiting"
            Console.ReadKey(false) |> ignore





