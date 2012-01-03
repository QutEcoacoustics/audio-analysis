namespace Felt.Core
    open System

    type DataType =
        | Training = 0
        | Test = 1
    
    type ValueType =
        | Number of float
        | String of string
        | Date of DateTime

    type 'a Column =
        {
            Name: string
            Values: 'a list
        }

    type DataSet =
        {
           DataType : DataType
           Values : Column<ValueType>
           Classes : Column<string>
        }
