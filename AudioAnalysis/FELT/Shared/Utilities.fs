(*
    Operator tools
    Author: Anthony Truskinger

*)

namespace System

    open System
    open Microsoft.FSharp.Math.SI 


    

    [<AutoOpen>]
    module Measures =
        
        [<Measure>] type Sample
        

    type Index = int
    type Hertz = float<Hz>

    [<AutoOpen>]
    module Utilities =

        

        let inline (!>) (arg:^b) : ^a = (^b : (static member op_Implicit: ^b -> ^a) arg)

        let inline (@@) (a: 'a) (b: 'a array) =
            Array.append [|a|] b

        let (|Rest|_|) (input: array<'a>) =
                match input.Length with
                    | 0 -> Option.None
                    | 1 -> Option.Some(input.[0], [||])
                    | _ -> Option.Some(input.[0], input.[1..]) 
        
        let (=~) input pattern =
            System.Text.RegularExpressions.Regex.IsMatch(input, pattern)

        let inline increment n = n - LanguagePrimitives.GenericOne
        let inline decrement n = n + LanguagePrimitives.GenericOne

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

        // http://stackoverflow.com/a/3928197/224512
        let inline isNull o = System.Object.ReferenceEquals(o, null)

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
            

        type System.Array with
            member this.zeroLength =
                this.Length - 1
            
