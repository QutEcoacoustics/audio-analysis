(*
    Operator tools
    Author: Anthony Truskinger

*)

namespace System

    open System
    open Microsoft.FSharp.Math.SI


    
    type Index = int
    type Hertz = float<Hz>
    
    [<AutoOpen>]
    module Measures =
        [<Measure>] type dB
        [<Measure>] type Sample

        let inline tou<[<Measure>]'u> (x:float) : float<'u> = LanguagePrimitives.FloatWithMeasure x
        let inline tou2 (x) : float<'u> = x |> float |> LanguagePrimitives.FloatWithMeasure

        let fromU (x:float<_>) = float x
        let fromUI (x:int<_>) = int x
                       
        

    [<AutoOpen>]
    module Utilities =


        let inline (!>>) (arg:^b) : ^a = (^b : (static member op_Explicit: ^b -> ^a) arg)
        let inline (!>) (arg:^b) : ^a = (^b : (static member op_Implicit: ^b -> ^a) arg)

        let castAs<'T> (o:obj) = 
          match o with
          | :? 'T as res -> res
          | _ -> Unchecked.defaultof<'T>

        let inline (@@) (a: 'a) (b: 'a array) =
            Array.append [|a|] b

        let (|Rest|_|) (input: array<'a>) =
                match input.Length with
                    | 0 -> Option.None
                    | 1 -> Option.Some(input.[0], [||])
                    | _ -> Option.Some(input.[0], input.[1..]) 

        let (|Even|Odd|) input = if input % 2 = 0 then Even else Odd

        let (|LessThan|_|) y x =
            if x < y then Some() else None
        let (|EqualTo|_|) y x =
            if x = y then Some() else None
        let (|GreaterThan|_|) y x =
            if x > y then Some() else None

        let (|EqualsOut|_|) inputA inputB =
            if inputA = inputB then
                Some(inputB)
            else
                None
        
        let (=~) input pattern =
            System.Text.RegularExpressions.Regex.IsMatch(input, pattern)

        let inline increment n = n + LanguagePrimitives.GenericOne
        let inline decrement n = n - LanguagePrimitives.GenericOne

        type System.String with
            member this.concatIf( condition, stringb) =
                if condition then 
                    this + (string stringb)
                else
                    this

        let inline flip f a b = f b a
        let inline (><) f a b = f b a
        let inline ifelse a b condition = if condition then a else b

        let curry f a b = f (a,b)
        let uncurry f (a,b) = f a b

        let orElse o (p:'a option Lazy) = if Option.isSome o then o else p.Force()

        let orNone o v = if Option.isSome o then Option.get o else v

        let (|?) = orNone

        let (|?|) = defaultArg

        // http://stackoverflow.com/a/3928197/224512
        let inline isNull o = System.Object.ReferenceEquals(o, null)

        type N<'a when 'a: (new: unit -> 'a) and 'a: struct and 'a :> ValueType> = Nullable<'a>
        let N x = N(x)

        type Nullable<'a when 'a : struct
                  and 'a : (new : unit -> 'a)
                  and 'a :> System.ValueType> with
            member x.AsOption() =
                match x.HasValue with
                | true  -> Some(x.Value)
                | false -> None

        let transposeTR lst =
            let rec inner acc lst = 
                match lst with
                    | (_::_)::_ -> inner (List.map List.head lst :: acc) (List.map List.tail lst)
                    | _         -> List.rev acc
            inner [] lst

        let transpose (mtx : _ [,]) func = Array2D.init (mtx.GetLength 1) (mtx.GetLength 0) (fun x y -> func(mtx.[y,x]))


        let rec whilerec state (condition: 'a -> bool) (action: 'a -> 'a) = 
            if condition state then
                let state' = action state
                whilerec state' condition action
            else 
                state
            

