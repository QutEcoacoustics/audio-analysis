namespace MQUTeR.FSharp.Shared
    open System
    open System.ComponentModel.DataAnnotations

   

    type DataSet =
        | Training = 0
        | Test = 1
    
    
    type FuzzyBit = 
        {
            [<System.ComponentModel.DataAnnotations.RangeAttribute(0.0,1.0)>]
            Value : float;
        }
        



    type DataType =
        | Number=0// of float
        | Date=1// of DateTime
        | Text=2// of string
        | Bit = 3 // of double
    
    /// A non generic, empty place holder base type. Do not inherit from this
    [<AbstractClass>]
    type Value() = class

        end

    /// A generic data holding class. All types should inherit from this.
    [<AbstractClass>]
    type BaseValue<'T>(v) = class
        inherit Value()
        let value:'T = v

        abstract member Value : 'T  with get
        default this.Value
            with get() = value

        end

    type Text(s) = class
        inherit BaseValue<string>(s)
        end

    type Number(n) = class
        inherit BaseValue<float>(n)
        
        static member op_Implicit (f : float) =
            new Number(f)
        static member op_Implicit (x : Number) =
            x.Value
        static member (+) (x : Number, y : Number) =
            new Number((+)  x.Value y.Value)
        static member DivideByInt (x : Number, i : int) =
            new Number(LanguagePrimitives.DivideByInt x.Value i)
        static member Zero =
            new Number(LanguagePrimitives.GenericZero)

        end

 

    type AverageText(s, histogram) = class
        inherit Text(s)
        member this.Histogtram 
            with get() : (string * float) array = histogram            
        end
    
    module tt =
        let t =
            let n = new Number(6.0)
            let s = new Text("abc")
            
            3

//    type Values =
//        | Numbers of float array
//        | Text of string array
//        | Dates of DateTime array
//        | Bits of FuzzyBit array
//        static member inline Apply(vdu:Values) f :  Values =
//            match vdu with
//                | Numbers ns -> Values.Numbers(f ns)
//                | Text ss -> Values.Text(f ss)
//                | Dates ds ->Values.Dates(f ds)
//                | Bits bs ->Values.Bits( f bs)

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

            Instances :  Map<ColumnHeader, Value array>


            ClassHeader : ColumnHeader
            Classes :  Class array
        }
       (* with
            member Length with get() = *)

    [<AutoOpen>]
    module DataHelpers =
        open MQUTeR.FSharp.Shared

        //let inline testAndCheck<'T when 'T :> Value> (input:obj) = if input :? 'T then Option.Some(input :?> 'T) else Option.None


        let inline castTo<'T> (x:obj) = x :?> 'T
        let inline checkAndCastTo<'T> (input:obj) = if input :? 'T then Option.Some(input :?> 'T) else Option.None

        let inline testAndCastArray<'CastTo> (input: 'a array) =
            if typeof<'a> = typeof<'CastTo> then
                Option.Some(Array.map (castTo<'CastTo>) input)
            else
                Option.None



        /// Active pattern for the value type
        /// to make pattern matching easier
        let (|IsText|_|) (input) =
            checkAndCastTo<Text> input
        let (|IsNumber|_|) (input) =
            checkAndCastTo<Number> input

        let (|IsNumbers|_|) (input: #Value array) =
            testAndCastArray<Number> input
        let (|IsTexts|_|) (input: #Value array) =
            testAndCastArray<Text> input
        let (|IsStrings|_|) (input: #Value array) =
            testAndCastArray<string> input
