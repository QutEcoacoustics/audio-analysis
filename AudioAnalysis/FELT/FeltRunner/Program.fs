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
open System.IO
open FELT.FindEventsLikeThis


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
         Directory.CreateDirectory (ResultsDirectory + runDate.ToString("yyyyMMdd_HHmmss\\"))
    with
        | ex -> 
            eprintfn "%s" ex.Message
            null

// load in features
let features = new ResizeArray<string>(config.["TrainingData"].Split(','))

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

if trFile.IsNone || teFile.IsNone
    prinfn "An error occurred loading one of the data files, exiting..."
else 
    // run the analysis
    let config =
            {
                RunDate runDate
                TestDataBytes: int64
                TrainingDataBytes: int64
                ReportDestination: FileInfo
                ReportTemplate: FileInfo
            }
    RunAnalysis Basic 

    Console.ReadKey(false) |> ignore





