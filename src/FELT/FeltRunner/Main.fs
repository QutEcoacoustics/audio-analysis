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
        open System.Collections
        open System.Collections.Specialized
        open System.Diagnostics
        open System.IO
        open System.Reflection
        //open Linq.QuotationEvaluation
        open System.Reflection
        open FELT.FindEventsLikeThis
        open FELT.Results
        open FELT.Search
        open FELT.Workflows
        open Microsoft.FSharp.Collections
        open log4net

        let fail() =
            Error "Exiting because of error!"
            #if DEBUG
            printfn "Debug hook...  press any key to continue..."
            Console.ReadKey(false) |> ignore
            #endif
            Environment.Exit(1);

        let version = Assembly.GetAssembly(typeof<ResultsComputation>).GetName() |> (fun x -> sprintf "%s, %s, %s" x.Name (x.Version.ToString()) x.CodeBase)
        let sep = Path.DirectorySeparatorChar |> string
        let config filePath = 
            let filePath' = 
                match filePath with
                    | "" | null -> Option.None
                    | s when File.Exists s -> Option.Some(s)
                    | _ ->
                        // okay try automatic resolution
                        let fn = Path.GetFileName filePath
                        let ed =  Path.GetDirectoryName <| Assembly.GetExecutingAssembly().Location
                        let guess = [ ed + sep + fn; ed + sep + "ConfigFiles" + sep + fn]
                        List.tryPick (fun p ->  if File.Exists p then Some(p) else Option.None) guess
                        

            
            if Option.isNone filePath' then
                 raise  <| FileNotFoundException("Cannot find the configuration file requested", filePath)

            let full =
                filePath'
                |> Option.get
                |> fun x -> let fm = new ExeConfigurationFileMap()  in fm.ExeConfigFilename <- x; fm
                |> fun y -> System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(y, ConfigurationUserLevel.None)
            
            let section = full.GetSection("appSettings") :?> AppSettingsSection
            let nvc : NameValueCollection = Reflection.ReflectionHelpers.getPropertyInternal section "InternalSettings"
            
//            if section = null || not (section :? NameValueCollection) then
//                raise <| new ConfigurationErrorsException("App settings not valid");
//            else
            full, nvc

            //full, full.AppSettings.Settings
//            let appSettings =   (full.GetSection("appSettings")  :> ConfigurationSection)  :?> AppSettingsSection
//            full.
//            let r = appSettings.["bbobs"]
//            match appSettings with
//                | :? NameValueCollection as nvc -> full, nvc
//                | _ -> raise <| new ConfigurationErrorsException("App settings not valid")

        /// Setup a basic run.
        /// Warning, mutation occurs, the source directory for the log file is set here as a side affect
        let setupRun (datestamp:DateTime) resultsDirectory extension =
            let resultsDirectory =
                try
                     Directory.CreateDirectory (resultsDirectory + datestamp.ToString("yyyyMMdd_HHmmss") + "\\")
                with
                    | ex -> 
                        eprintfn "%s" ex.Message
                        fail()
                        null

            let reportDateName (dt: DateTime) e analysis = 
                dt, (new FileInfo( sprintf "%s\\%s %s.%s" resultsDirectory.FullName (dt.ToString "yyyy-MM-dd HH_mm_ss") analysis e))

            //! Warning: mutation
            MQUTeR.FSharp.Shared.Logger.fName <- ((String.Empty |> reportDateName datestamp extension |> snd |> fun x -> x).FullName + ".log")

            let reportName analysis = reportDateName DateTime.Now extension analysis
            resultsDirectory, reportDateName, reportName


        let suggestion() =
            Info "Start: read configuration settings..."

            let fullConfig, config = config "Truskinger.Felt.Suggestion.config"
            
            // settings
            let ResultsDirectory = config.["ResultsDirectory"]
            let WorkingDirectory = config.["WorkingDirectory"]
            let TestData = WorkingDirectory + config.["TestData"]
            let TrainingData = WorkingDirectory + config.["TrainingData"]
            let exportFrn = bool.Parse(config.["ExportFrn"])
            let exportFrd = bool.Parse(config.["ExportFrd"])
            let allAnalyses = bool.Parse(config.["CrossAnalyseAllFeatures"])
            let allAnalysesLimit = System.Int32.Parse(config.["CrossAnalyseAllFeatures_Limit"])
            let duplicates = System.Int32.Parse(config.["Duplicates"])


            // ANALYSIS RUN SETTINGS
            let allKnownAnalyses = FELT.Workflows.Analyses
            let ac = fullConfig.GetSection("analyses")
            let analysesConfig =  castAs<AnalysesConfig> ac
            let analyses = analysesConfig.Analyses |> Seq.cast |> Seq.map (fun (ae:FELT.Runner.Analysis) -> ae.Name, allKnownAnalyses.TryFind(ae.Name) ) |> Seq.toArray

            if (analyses.Length = 0) then
                Error "No analysis set in configuration"
                fail()

            Infof "Analyses sheduled to run: %s" (Seq.fold (fun s (a, _) -> s + a + ", "  ) "" analyses)

            if not (Seq.forall (snd >> Option.isSome) analyses) then
                Error "Invalid analysis found, or name count not be found"
                fail()

            // Transforms settings
            let transform = castAs<TransformsConfig> <| fullConfig.GetSection("transformations")
            let transforms = transform.Transformations |> Seq.cast |> Seq.map (fun (tx:TransformElement) -> tx.Features, tx.NewName, tx.Using) |> Seq.toList


            // set up run
            let batchRunDate = DateTime.Now
            let resultsDirectory, reportDateName, reportName = setupRun batchRunDate ResultsDirectory "xlsx"

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
                    let results = powerset |> Array.collect (fun s -> Array.map (fun (x,y) -> (x,y,s)) analyses)

                    // now set for running duplicate testing
                    if duplicates > 0 then
                        let s = seq { for i in 0..duplicates  -> results}

                        Array.concat s, powerset.Length
                    else
                        results, powerset.Length
                    

                if allAnalyses then
                    Warnf "All selected analyses will be run with EVERY combination of features! %i features, %i combinations, %i analyses, %i duplicates, %i runs." trData.Instances.Count combinations analyses.Length duplicates analyses'.Length
                else
                    Infof "All selected analyses will be run with all features! %i features, %i analyses, %i duplicates, %i runs." trData.Instances.Count combinations duplicates analyses'.Length
                
                // run the analyses
                let templateResolver name = 
                    let fi = new FileInfo(name)
                    if fi.Exists then
                        fi
                    else
                        let dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                        let fi' = new FileInfo(Path.Combine(dir, name))
                        if fi'.Exists then
                            fi'
                        else
                            raise <| new FileNotFoundException(name)

                let run (ano: string * (WorkflowItem list) Option * Set<ColumnHeader>) =
                    let analysis, ops, columnsToUse = ano
                    let dt, dest = reportName analysis 
                    let config =
                        {
                            RunDate = dt;
                            AnalysisType = analysis;
                            TestDataBytes = (new FileInfo(TestData)).Length;
                            TrainingDataBytes = (new FileInfo(TrainingData)).Length;
                            ReportDestination = dest;
                            ReportTemplate = templateResolver "ExcelResultsComputationTemplate.xlsx";
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
                        let dest = SummationReport.Write 
                                    (reportDateName batchRunDate "x" "Summary" |> snd)
                                    (templateResolver "ExcelResultsSummationTemplate.xlsx") 
                                    configs
                        Log "Finished summary report"    
                        Some(dest)
                    else
                        Option.None


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

        let search analysisConfig () =
            //let startSearchDate = DateTime.Now
            //
            //Info "Reading configurations settings"
            //let configFile, config = config analysisConfig
            //Debugf "Loaded: %s" configFile.FilePath
            //
            //// "Truskinger.Felt.Search.config"
            //let wd = config.["WorkingDirectory"]
            //let rd = Path.Combine( wd, config.["ResultsDirectory"])
            //
            //let resultsDirectory, reportDateName, reportName = setupRun startSearchDate rd "json"
            //let runDate, reportNameFull = reportName "Search"
            //
            //let config = 
            //    { 
            //        WorkingDirectory = wd; 
            //        ResultsDirectory = resultsDirectory ; 
            //        ResultsFile = reportNameFull
            //
            //        TrainingData = new FileInfo(Path.Combine(wd, config.["TrainingData"]))
            //
            //        TestAudio = new DirectoryInfo(config.["TestAudio"]);
            //
            //        // audio caches
            //        TrainingAudio = new DirectoryInfo(config.["TrainingAudio"]);
            //        AudioSnippetCache = new DirectoryInfo(config.["AudioStoreDirectory"]);
            //
            //        AedConfig = 
            //            {
            //                SmallAreaThreshold = config.["aed_smallAreaThreshold"] |> int |> LanguagePrimitives.Int32WithMeasure;
            //                IntensityThreshold = config.["aed_intensityThreshold"] |> tou2;
            //            }
            //    }
            //// execute analysis
            //FELT.Search.main config
            //
            //Infof "Search Completed, time taken: %A" (DateTime.Now - startSearchDate)
            ()

        let usage error () =
            Info "Incorrect paramters given"
            if String.IsNullOrWhiteSpace error |> not then
                Error error 
            Info "Usage: "
            Info "      xxxxxx.exe analysisOption pathToConfig"
            Info "Valid analysisOptions are: suggestion, search"
            ()

        let getLogFilePath() =
            let rootAppender = 
                    (LogManager.GetRepository() :?> log4net.Repository.Hierarchy.Hierarchy).Root.Appenders 
                    |> Seq.cast<obj>
                    |> Seq.pick (fun ap -> if ap :? Appender.FileAppender then Some(ap :?> Appender.FileAppender) else Option.None) 
            rootAppender.File;

        let copyLog source _ =
            let dest = Logger.fName
            if not <| String.IsNullOrWhiteSpace dest then
                File.Copy(source, dest)
        

        let Entry programEntry (args:string[]) =
            let ad = AppDomain.CurrentDomain
            let log4netFile = getLogFilePath()
            if not programEntry then  ad.UnhandledException.Add (fun (args:UnhandledExceptionEventArgs) ->ErrorFailf "Unhandled exception:\n%A" args.ExceptionObject)
            ad.ProcessExit.Add (copyLog log4netFile)
            ad.UnhandledException.Add (copyLog log4netFile)

            Log "Welcome to felt version:"
            Logf "%s" version

            #if DEBUG
            if not (Debugger.IsAttached || programEntry) then
                Warn "Debug hook...  press any key to continue..."
                Console.ReadKey(false) |> ignore
            #endif

            // determine analysis to run
            let first = Seq.tryHead args

            let f = 
                match first with
                    | Some "suggestion" -> 
                        Info "Running suggestion tool."
                        suggestion
                    | Some "search" -> 
                        Info "Running search tool."

                        let configPath = Array.tryGet args 1
                        if configPath.IsSome then
                            (search configPath.Value)
                        else
                            Warn "Config path not specified, will attempt to load default"
                            (search  "Truskinger.Felt.Search.config")
                    | _ -> usage "Analysis option not valid"

            f()

            Info "Exiting"
            if not programEntry then Console.ReadKey(false) |> ignore
            0

        let ProgramEntry (args : Object) =
            // raise <| new NotImplementedException()
            Warn "Compiled for running suggestion tool only"
            ignore <| Entry true [|"suggestion"|]
            

//        [<EntryPoint>]
        let CommandEntry args =
            Entry false args



