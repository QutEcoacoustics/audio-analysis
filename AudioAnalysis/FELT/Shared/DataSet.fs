namespace MQUTeR.FSharp.Shared
    open System
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Quotations
    open MQUTeR.FSharp.Shared.Equality
    open Microsoft.FSharp.Numerics

    type DataSet =
        | Training = 0
        | Test = 1


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

        override x.Equals(yobj) =
            Equality.equalsCast x yobj [(fun z -> z.Value)]
 
        override x.GetHashCode() = Equality.GetHashCode([x.Value])
 
        interface System.IComparable with
            member x.CompareTo yobj =
                Equality.CompareTo x yobj [fun z -> z.Value]
            end
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

    type AveragedNumber(mean, count:int,  ?standardDeviation:float, ?fakeStandardDeviation:float) = class
        inherit Number(mean)

        member this.Mean 
            with get() = base.Value 
        
        member this.Count 
            with get() = count 
        
        member this.StandardDeviation 
            with get() = standardDeviation 

        member this.FakeStdDev 
            with get() = 
                if fakeStandardDeviation.IsNone then 
                    if standardDeviation.IsSome then 
                        standardDeviation.Value
                    else
                       failwith "No standard deviation has been set and no default return value has been provided"
                else 
                    fakeStandardDeviation.Value
        
        end

    type ModuloMinute(z) = class
        inherit BaseValue<IntegerZ1440>(z)
        end
    
    type AveragedModuloMinute(mean, count:int, ?standardDeviation:float, ?fakeStandardDeviation:float ) = class
        inherit ModuloMinute(mean)

        member this.Mean 
            with get() = base.Value 
        
        member this.Count 
            with get() = count 

        member this.StandardDeviation 
            with get() = standardDeviation 

        member this.FakeStdDev 
            with get() = 
                if fakeStandardDeviation.IsNone then 
                    if standardDeviation.IsSome then 
                        standardDeviation.Value
                    else
                        failwith "No standard deviation has been set and no default return value has been provided"
                else 
                    fakeStandardDeviation.Value
        
        end

    type Date(d) = class
        inherit BaseValue<DateTime>(d)
        end

    type FuzzyBit(b) = class
        inherit Number(b)
        do
            if (b > 1.0) || (b < 0.0) then
                raise (ArgumentOutOfRangeException(sprintf "A bit can only contain values between 1 and 0 (inclusive). %f is invalid." b))
    end
 

    type AverageText(s, histogram) = class
        inherit Text(s)
        member this.Histogram 
            with get() : (string * float) array = histogram        
            
        override x.Equals(yobj) =
            Equality.equalsCast x yobj [(fun z -> box z.Value); (fun z -> box z.Histogram)]
 
        override x.GetHashCode() = Equality.GetHashCode([ box x.Value ; box x.Histogram ])
 
        interface System.IComparable with
            member x.CompareTo yobj =
                Equality.CompareTo x yobj [(fun z -> box z.Value) ; (fun a -> box a.Histogram)]
            end
        end    
    

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
    type Class = string
    type RowNumber = int
    

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


        let castTo<'T> (x:obj) = x :?> 'T
        let canCastTo<'T> (x:obj) = x :? 'T
        let checkAndCastTo<'T> (input:obj) = if input :? 'T then Option.Some(input :?> 'T) else Option.None

        let inline unwrap (input: #BaseValue<'c> array) =
                    Array.map (fun (x: #BaseValue<'c>) -> x.Value) input
        let value (v:BaseValue<'a>) = v.Value

        let testAndCastArray<'CastTo> (input: Value array) : Option<'CastTo array> =
            if input.Length = 0 then
                Some(Array.empty<'CastTo>)
            else
                let h = Array.head input
                if h.GetType() = typeof<'CastTo> then
                   Option.Some(Array.map (castTo<'CastTo>) input)
                else
                    Option.None

        let testCastAndUnwrapArray<'unwrapTo, 'CastTo when 'CastTo :> BaseValue<'unwrapTo>> (input: Value array) : Option<'unwrapTo array> =
            if input.Length = 0 then
                Some(Array.empty<'unwrapTo>)
            else
                let h = Array.head input
                if h.GetType() = typeof<'CastTo> then
                   Option.Some(Array.map (fun x -> 
                                                let c = castTo<'CastTo> x
                                                value c
                                                
                                                ) input)
                else
                    Option.None
        
        let testCastAndUnwrapNumericArray(input: Value array) : Option<float array> =
            if input.Length = 0 then
                Some(Array.empty<float>)
            else
                let h (x:Value) =
                    match x with 
                    | :? BaseValue<float> as b -> b.Value
                    | :? BaseValue<IntegerZ1440> as z -> float z.Value
                    | _ -> failwith "Invalid numeric type found"
                 
                Option.Some(Array.map (h) input)                               
//                if h.GetType() = typeof<'CastTo> then
//                   
//                else
//                    Option.None
        
        let getRow rowId (d:Data) = d.Instances |> Seq.cast |> Seq.map (fun (kvp:System.Collections.Generic.KeyValuePair<ColumnHeader, Value array>) -> kvp.Value.[rowId])
            
        /// Active pattern for the value type
        /// to make pattern matching easier
        let (|IsText|_|) (input) =
            checkAndCastTo<Text> input
        let (|IsNumber|_|) (input) =
            checkAndCastTo<Number> input
        let (|IsAvgNumber|_|) (input) =
            checkAndCastTo<AveragedNumber> input
        let (|IsModuloMinute|_|) (input) =
            checkAndCastTo<ModuloMinute> input
        let (|IsAvgModuloMinute|_|) (input) =
            checkAndCastTo<AveragedModuloMinute> input
        let (|IsDate|_|) input =
            checkAndCastTo<Date> input

        let (|IsNumbersU|_|) (input: Value array) =
            testCastAndUnwrapArray<float, Number> input
        let (|IsModuloMinutesU|_|) (input: Value array) =
            testCastAndUnwrapArray<IntegerZ1440, ModuloMinute> input
            
        let (|IsAnyNumbers|_|) (input: Value array) =
            testCastAndUnwrapNumericArray input

        let (|IsNumbers|_|) (input: Value array) =
            testAndCastArray<Number> input
        let (|IsDates|_|) (input: Value array) =
            testAndCastArray<Date> input
        let (|IsModuloMinutes|_|) (input: Value array) =
                    testAndCastArray<ModuloMinute> input
        let (|IsTexts|_|) (input: Value array) =
            testAndCastArray<Text> input
        let (|IsStrings|_|) (input: Value array) =
            testAndCastArray<string> input

        