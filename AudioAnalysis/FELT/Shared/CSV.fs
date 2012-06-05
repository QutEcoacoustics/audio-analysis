(*
    CSV tools
    Author: Anthony Truskinger

*)

namespace MQUTeR.FSharp.Shared
    open System
    open MQUTeR.FSharp.Shared
    
    open Microsoft.FSharp.Collections

    open Microsoft.FSharp.Math
    open System.Text.RegularExpressions
    open System.Diagnostics

    module CSV =
        let CsvColumnLinesTypeCheck = 20
        let quotation = "\""

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
                let num = ref 0.0
                if Double.TryParse(input,  num ) then
                    if (!num = 0.0 || !num = 1.0) then
                        DataType.Bit
                    else
                        DataType.Number
                elif DateTimeOffset.TryParse(input, ref (new DateTimeOffset())) then
                    DataType.Date
                else
                    DataType.Text
            
            let dts  = List.map dataType instances  
            let first = List.head dts;

            if List.forall (fun x -> first = x) dts then
                first
            else
                if List.forall (fun x -> x = DataType.Number || x = DataType.Bit) dts then
                    DataType.Number
                else
                    DataType.Text


        let csvToData (features: ResizeArray<string> ) (text: string array)  =
            match text with
                | Rest (firstRow, rest) ->

                    let headers = firstRow |> rowToList ','

                    let cells = Array.Parallel.map (rowToList ',') rest  

                    let typeCheckCount = min CsvColumnLinesTypeCheck cells.Length

                    // take first CsvColumnLinesTypeCheck rows of each column, and attempt to guess type
                    let firstFewRows = cells.[0..typeCheckCount - 1]     
                    let grabCol index = ((Array.fold (fun lst (x : string array) ->  x.[index] :: lst ) List<string>.Empty  firstFewRows) |> guessType, index)
                    let bounds =  Array.init firstFewRows.[0].Length   (fun x -> x)                         
                    let types = Array.map grabCol bounds

                    // filter out unwanted columns
                    let filteredTypes = Array.filter (fun (dt, index) -> features.Contains(headers.[index])) types
                    
                    let finalHeaders =  Array.fold (fun state (typ, index) -> Map.add headers.[index] typ state) Map.empty<ColumnHeader, DataType> filteredTypes

                    // storage
                    let populateColumn (map:Map<ColumnHeader, Value array>) (datatype, columnIndex) =
                        let convert rowIndex = 
                            let row = cells.[rowIndex]
                            let cell = row.[columnIndex] 
                            let (v:Value) = 
                                match datatype with
                                    | DataType.Date -> upcast new Date(DateTimeOffset.Parse(cell))
                                    | DataType.Number ->upcast new Number(Double.Parse(cell))
                                    | DataType.Text -> upcast new Text(cell)
                                    | DataType.Bit -> upcast new FuzzyBit(Double.Parse(cell))
                                    | _ -> failwith "Invalid data type"
                            v

                        let columnValues  = Array.init cells.Length convert
                        map.Add(headers.[columnIndex], columnValues)
                   
                    let instances = PSeq.fold populateColumn Map.empty<ColumnHeader, Value array> filteredTypes
                    let className = features.[0]
                    let classes =  Array.map (fun (x:Value) -> (x :?> BaseValue<string>).Value) instances.[className] 
                     
                    {
                        DataSet = DataSet.Training; // : DataSet
                        Headers = finalHeaders.Remove(className);//: Map<ColumnHeader, DataType>
                        Instances  = instances.Remove(className);//:  Map<ColumnHeader, Value array>
                        ClassHeader = className; //: ColumnHeader
                        Classes = classes //:  Class array
                    }
                | _ -> failwith "no text input given"
        
////        let csvToVectors (text: string array)  =
////            match text with
////                | Rest (firstRow, rest) ->
////
////                    let headers = firstRow |> rowToList ','
////
////                    let cells = Array.map (rowToList ',') rest  
////                    
////                    let typeCheckCount = min CsvColumnLinesTypeCheck cells.Length
////
////                    // take first CsvColumnLinesTypeCheck rows of each column, and attempt to guess type
////                    let firstFewRows = cells.[0..typeCheckCount - 1]     
////                    let grabCol index = ((Array.fold (fun lst (x : string array) ->  x.[index] :: lst ) List<string>.Empty  firstFewRows) |> guessType, index)
////                    let bounds =  Array.init firstFewRows.[0].Length   (fun x -> x)                         
////                    let types = Array.map grabCol  bounds
////
////                    // storge
////                    let make dt : Map<string,'a array> =
////                        Array.filter (fun x -> fst x = dt) types |> Array.fold (fun m (x,i) -> m.Add(headers.[i], (Array.zeroCreate<'a> cells.Length))) Map.empty<string,'a array>                  
////                    
////                    let (dates: Map<string,DateTime[]> ) = make DataType.Date
////                    let (numbers: Map<string,double[]>) = make DataType.Number
////                    let (strings: Map<string,string[]>) = make DataType.Text
////
////                    // scan through each row
////                    for rowIndex = 0 to cells.zeroLength do
////                        let row = cells.[rowIndex]
////                        // for each column
////                        for colIndex = 0 to row.zeroLength do
////                            let cell = row.[colIndex]
////                            let name = headers.[colIndex]
////                            // match index with types list
////                            let t = types.[colIndex]
////                            // add element to apprrpiate list
////                            try
////                                match t with
////                                    | (DataType.Date, _) ->dates.[name].[rowIndex] <- DateTime.Parse(cell)
////                                    | (DataType.Number, _) -> numbers.[name].[rowIndex] <- Double.Parse(cell)
////                                    | (DataType.Text, _) -> strings.[name].[rowIndex] <- cell
////                                    | _ -> failwith "Invalid data type"
////                                with
////                                    | :? System.FormatException -> failwithf "Could not parse a date / number; row:%i col:%i, value: %s" rowIndex colIndex cell
////                    (headers, numbers, strings, dates)
////                | _ -> failwith "no text input given"

            

        

