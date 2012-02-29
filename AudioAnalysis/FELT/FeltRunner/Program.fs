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
open FELT.Runner
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

(* ANALYSIS RUN SETTING *)
let analysis = <@ FELT.Workflows.BasicGrouped @>


let transform = ConfigurationManager.GetSection("transformations") :?> TransformsConfig

let transforms = transform.Transformations |> Seq.cast |> Seq.map (fun (tx:TransformElement) -> tx.Feature, tx.NewName, tx.Using) |> Seq.toList

let DefaultClassString = "Tag"

// set up run
let runDate = DateTime.Now

let resultsDirectory =
    try
         Directory.CreateDirectory (ResultsDirectory + runDate.ToString("yyyyMMdd_HHmmss") + "\\")
    with
        | ex -> 
            eprintfn "%s" ex.Message
            fail()
            null
let reportName() = sprintf "%s\\%s %s.xlsx" resultsDirectory.FullName (runDate.ToString "yyyy-MM-dd HH_mm_ss") (Utilities.getNameOfModuleBinding analysis)
let reportDest = new  FileInfo(reportName())

let logger = Logger.create (reportDest.FullName + ".log")
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

    // run the analysis
    let config =            {
                RunDate = runDate;
                TestDataBytes = (new FileInfo(TestData)).Length;
                TrainingDataBytes = (new FileInfo(TrainingData)).Length;
                ReportDestination = reportDest;
                ReportTemplate = new FileInfo("ExcelResultsComputationTemplate.xlsx");
                TestOriginalCount = teData.Classes.Length;
                TrainingOriginalCount = trData.Classes.Length;
                ExportFrd = exportFrd;
                ExportFrn = exportFrn
            }


    RunAnalysis trData teData (analysis.Eval()) transforms config |> ignore

    Info "end: main analysis..."
    Info "Analysis complete!"
    
    // clear any keystrokes accumulated by accident
    while Console.KeyAvailable do Console.ReadKey(false) |> ignore
    
    Log "Open report (y/n)"
    
    let openFile = Console.ReadKey(true);
    Warnf "Key pressed: %c" openFile.KeyChar

    if Char.ToLower openFile.KeyChar = 'y' then
        Infof "Opening file: %s" reportDest.FullName
        Process.Start(reportDest.FullName) |> ignore

    Info "Exiting"
    Console.ReadKey(false) |> ignore





