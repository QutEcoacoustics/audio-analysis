(*
    CSV tools
    Author: Anthony Truskinger

*)

namespace Felt.Shared
    open System
    open Felt.Shared
    open Felt.Shared.Utilities
    open Microsoft.FSharp.Math

    module CSV =
        let CsvColumnLinesTypeCheck = 5
        let quotation = "\""
        type DataType =
            | Number = 0 // of float
            | Date = 1 // of DateTime
            | Text = 2 // of string

        //:rowToList "Hello, this is  a,test,\"hell,lo\", to see,, what happen, soooo, \"\"\"we need a value\"\", said bree\", something else " ',';;
        let rowToList delimitter (line:string) =

            let notInQuote (string:string) =
                match string.Length with
                    | 0 -> true
                    | 1 -> not(string.StartsWith(quotation))
                    | _ -> 
                        if string.StartsWith(quotation) then
                            false
                        else
                            (string.StartsWith(quotation) && string.EndsWith(quotation) && not (string.EndsWith("\"\"")))
            
            let dequote string =
                if System.String.IsNullOrWhiteSpace(string) then
                    string
                else
                    //:Debug.writefn "-----> %s" string
                    string 
                    |> fun s -> s.Replace("\"\"", quotation) 
                    |> fun s -> if s.StartsWith(quotation) then s.Substring(1) else s 
                    |> fun s -> if s.EndsWith(quotation) then s.Substring(0, s.Length - 1) else s

            let rec scan (chars: seq<char>) wordsAcc (partial: System.String) =
                match chars with
                    | Rest (c, rest)  ->  
                        if c = delimitter && (notInQuote partial)  then                           
                            scan rest ((dequote partial) :: wordsAcc) ""
                        else
                            scan rest wordsAcc (System.String.Concat(partial, c))
                    | _ -> (dequote partial) :: wordsAcc

            scan line [] ""

        /// This function attempts to guess the type of all instances given
        /// resulting in either a string, float, or DateTime
        let guessType (instances: string list) = 
            // check head
            let first = List.head instances


            let dataType =
                if Double.TryParse(first ) then
                    DataType.Number
                elif true then
                    DataType.Date
                else
                    DataType.Text



            33
        
        let csvToVectors (text: string list)  =
            match text with
                | firstRow :: rest ->

                    let headers = firstRow |> rowToList ','

                    let cells = List.map (rowToList ',') rest 
      
                    
                    cells
                | _ -> failwith "no text input given"

            

        

