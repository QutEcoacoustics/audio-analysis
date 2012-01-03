
module Felt.Core.Rubbish
// Learn more about F# at http://fsharp.net

open System.Configuration

//let settings = ConfigurationManager.AppSettings.Set("boobs", "donkey")

type boobs = ConfigurationManager

let config = boobs.OpenExeConfiguration(ConfigurationUserLevel.None)

config.AppSettings.Settings.Add("boobs", "donkey")

config.Save()



