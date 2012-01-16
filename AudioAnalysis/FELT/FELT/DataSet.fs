namespace FELT.Core
    open System
    open MQUTeR.FSharp.Shared.CSV

    type DataSet =
        | Training = 0
        | Test = 1
    

    type 'a Column =
        {
            Name: string
            Values: 'a list
        }

    type Data =
        {
           DataSet : DataSet
           Headers : String array
           Types : MQUTeR.FSharp.Shared.CSV.DataType array

           Text : Map<string, string array>
           Numbers : Map<string, double array>
           Dates : Map<string, DateTime array>
           Bits : Map<string, bool array>

           ClassHeader : string
           Classes :  string array
        }
       (* with
            member Length with get() = *)
