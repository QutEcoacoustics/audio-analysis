(*
    CSV tools
    Author: Anthony Truskinger

*)

namespace MQUTeR.FSharp.Shared
    open System
    open MQUTeR.FSharp.Shared
    open MQUTeR.FSharp.Shared.Utilities
    open Microsoft.FSharp.Math
    open System.Text.RegularExpressions

    module CSV =
        let CsvColumnLinesTypeCheck = 5
        let quotation = "\""
        type DataType =
            | Number=0// of float
            | Date=1// of DateTime
            | Text=2// of string

        type 'a Column =
            {
                Name: string
                Values: 'a array
                //DataType: DataType
            }

//        type ColType =
//            | Number of float Column
//            | Date of DateTime Column
//            | Text of string Column


        //:rowToList "Hello, this is  a,test,\"hell,lo\", to see,, what happen, soooo, \"\"\"we need a value\"\", said bree\", something else " ',';;
        let rowToList delimitter (line:string)  =
            let line' = line.ToCharArray()
            let notInQuote (str:string) =
                match str.Length with
                    | 0 -> true
                    | 1 -> not(str.StartsWith(quotation))
                    | _ -> 
                        if str.TrimStart().StartsWith(quotation) then
                            // remove all escaped quotations... this limits the number of cases to deal with
                            let str' = str.TrimStart().Replace("\"\"", String.Empty)
                            not( str'.StartsWith(quotation) && not(str'.EndsWith(quotation)) )
                        else
                            true
            let dequote string =
                let regexPattern = "(\s*\")(.*)(\"\s*)$"
                if System.String.IsNullOrWhiteSpace(string) then
                    string
                else
                     string 
                    |> fun s -> s.Replace("\"\"", quotation) 
                    |> fun s ->
                        let reg = Regex.Match(s, regexPattern)
                        if (reg.Success) then
                            // 0th group -> entire string, 1st -> white space, 2nd -> value we want
                            reg.Groups.[2].Value
                        else 
                            s

            let rec scan (chars: array<char>) wordsAcc (partial: System.String) =
                match chars with
                    | Rest (c, rest)  ->  
                        if c = delimitter && (notInQuote partial)  then                           
                            scan rest ((dequote partial) :: wordsAcc) ""
                        else
                            scan rest wordsAcc (System.String.Concat(partial, c))
                    | _ -> (dequote partial) :: wordsAcc
            scan line' [] "" |> List.toArray |> Array.rev


        /// This function attempts to guess the type of all instances given
        /// resulting in either a string, float, or DateTime
        let guessType (instances: string list) = 
            let dataType input =
                if Double.TryParse(input,  ref 0.0 ) then
                    DataType.Number
                elif DateTime.TryParse(input, ref (new DateTime())) then
                    DataType.Date
                else
                    DataType.Text
            
            let dts  = List.map dataType instances  
            let first = List.head dts;

            if List.forall (fun x -> first = x) dts then
                first
            else
                DataType.Text
        
        let csvToVectors (text: string array)  =
            match text with
                | Rest (firstRow, rest) ->

                    let headers = firstRow |> rowToList ','

                    let cells = Array.map (rowToList ',') rest  
                    
                    let typeCheckCount = min CsvColumnLinesTypeCheck cells.Length

                    // take first CsvColumnLinesTypeCheck rows of each column, and attempt to guess type
                    let firstFewRows = cells.[0..typeCheckCount - 1]     
                    let grabCol index = ((Array.fold (fun lst (x : string array) ->  x.[index] :: lst ) List<string>.Empty  firstFewRows) |> guessType, index)
                    let bounds =  Array.init firstFewRows.[0].Length   (fun x -> x)                         
                    let types = Array.map grabCol  bounds

                    // storge
                    let make dt : Map<string,'a array> =
                        Array.filter (fun x -> fst x = dt) types |> Array.fold (fun m (x,i) -> m.Add(headers.[i], (Array.zeroCreate<'a> cells.Length))) Map.empty<string,'a array>                  
                    
                    let (dates: Map<string,DateTime[]> ) = make DataType.Date
                    let (numbers: Map<string,double[]>) = make DataType.Number
                    let (strings: Map<string,string[]>) = make DataType.Text

                    // scan through each row
                    for rowIndex = 0 to cells.zeroLength do
                        let row = cells.[rowIndex]
                        // for each column
                        for colIndex = 0 to row.zeroLength do
                            let cell = row.[colIndex]
                            let name = headers.[colIndex]
                            // match index with types list
                            let t = types.[colIndex]
                            // add element to apprrpiate list
                            try
                                match t with
                                    | (DataType.Date, _) ->dates.[name].[rowIndex] <- DateTime.Parse(cell)
                                    | (DataType.Number, _) -> numbers.[name].[rowIndex] <- Double.Parse(cell)
                                    | (DataType.Text, _) -> strings.[name].[rowIndex] <- cell
                                    | _ -> failwith "Invalid data type"
                                with
                                    | :? System.FormatException -> failwithf "Could not parse a date / number; row:%i col:%i, value: %s" rowIndex colIndex cell
                    (headers, numbers, strings, dates)
                | _ -> failwith "no text input given"

            

        

