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


open Felt.Shared
open System
open System.Configuration
open System.IO


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
         Directory.CreateDirectory (ResultsDirectory + runDate.ToString("yyyymmdd_HHmmss\\"))
    with
        | ex -> 
            eprintfn "%s" ex.Message
            null

// load data

let trFile = IO.readFileAsString TrainingData
let teFile = IO.readFileAsString TestData





