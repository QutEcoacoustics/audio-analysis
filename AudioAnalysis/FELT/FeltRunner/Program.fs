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
open System
open System.Configuration
open System.Diagnostics
open System.IO
open System.Reflection
open FELT.FindEventsLikeThis
open FELT.Results

let fail() =
    eprintfn "Exiting because of error!"
    #if DEBUG
    printfn "Debug hook...  press any key to continue..."
    Console.ReadKey(false) |> ignore
    #endif
    Environment.Exit(1);

let version = Assembly.GetAssembly(typeof<ResultsComputation>).GetName() |> (fun x -> sprintf "%s, %s, %s" x.Name (x.Version.ToString()) x.CodeBase)
printfn "Welcome to felt version:"
printfn "%s" version

#if DEBUG
printfn "Debug hook...  press any key to continue..."
Console.ReadKey(false) |> ignore
#endif

printfn "Start: read configuration settings..."

let config = ConfigurationManager.AppSettings

// settings
let ResultsDirectory = config.["ResultsDirectory"]
let WorkingDirectory = config.["WorkingDirectory"]
let TestData = WorkingDirectory + config.["TestData"]
let TrainingData = WorkingDirectory + config.["TrainingData"]

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
let reportDest = new  FileInfo(ResultsDirectory.ToString() + runDate.ToString("yyyy-MM-dd HH_mm_ss") + ".xlsx")

// load in features
let features = new ResizeArray<string>(config.["Features"].Split(','))

printfn "end: configuration settings..."
printfn "Start: data import..."
// load data

let loadAndConvert features filename = 
    let lines = IO.readFileAsString filename
    if lines.IsNone then
        eprintfn "There are no lines to read in %s" filename
        Option.None
    else
        lines.Value |> CSV.csvToData features |> Option.Some


// create data
let trFile = loadAndConvert features TrainingData 
let teFile = loadAndConvert features TestData

printfn "end: data import..."

if trFile.IsNone || teFile.IsNone then
    eprintfn "An error occurred loading one of the data files, exiting..."
    fail()
else 
    printfn "start: main analysis..."

    let trData = { trFile.Value with DataSet = DataSet.Training}
    let teData = { trFile.Value with DataSet = DataSet.Test}

    // run the analysis
    let config =            {
                RunDate = runDate;
                TestDataBytes = (new FileInfo(TestData)).Length;
                TrainingDataBytes = (new FileInfo(TrainingData)).Length;
                ReportDestination = reportDest;
                ReportTemplate = new FileInfo("ExcelResultsComputationTemplate.xlsx");
            }

    RunAnalysis trData teData FELT.FindEventsLikeThis.BasicGrouped config |> ignore

    printfn "end: main analysis..."
    printfn "Analysis complete!"
    printf "Open report (y/n)"
    let openFile = Console.ReadKey(true);

    if Char.ToLower openFile.KeyChar = 'y' then
        Process.Start(reportDest.FullName) |> ignore

    printfn "Exiting"
    Console.ReadKey(false) |> ignore





