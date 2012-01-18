namespace MQUTeR.FSharp.Shared
    open System

    type DataSet =
        | Training = 0
        | Test = 1
    
    type Bit = double


    type DataType =
        | Number=0// of float
        | Date=1// of DateTime
        | Text=2// of string
        | Bit = 3 // of double


    type Value = class
        
        end
        

    type 'a Column =
        {
            Name: string
            Values: 'a array
            DataType: DataType
        }

    type ColumnHeader = string
    type Class = string //<'a when 'a : equality> = 'a -> 'a

    type Data =
        {
            DataSet : DataSet
            Headers : Map<ColumnHeader, DataType>

//            Text : Map<ColumnHeader, string array>
//            Numbers : Map<ColumnHeader, double array>
//            Dates : Map<ColumnHeader, DateTime array>
//            Bits : Map<ColumnHeader, Bit array>

            Instances : Map<ColumnHeader, Value array>


            ClassHeader : ColumnHeader
            Classes :  Class array
        }
       (* with
            member Length with get() = *)
