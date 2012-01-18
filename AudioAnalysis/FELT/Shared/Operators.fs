(*
    Operator tools
    Author: Anthony Truskinger

*)

namespace MQUTeR.FSharp.Shared

    open System

    type index = int

    module Utilities =

        let inline (@@) (a: 'a) (b: 'a array) =
            Array.append [|a|] b

        let (|Rest|_|) (input: array<'a>) =
                match input.Length with
                    | 0 -> Option.None
                    | 1 -> Option.Some(input.[0], [||])
                    | _ -> Option.Some(input.[0], input.[1..]) 
        
        let (=~) input pattern =
            System.Text.RegularExpressions.Regex.IsMatch(input, pattern)

        type System.String with
            member this.concatIf( condition, stringb) =
                if condition then 
                    this + (string stringb)
                else
                    this

        // http://stackoverflow.com/a/3928197/224512
        let inline isNull o = System.Object.ReferenceEquals(o, null)

        let transposeTR lst =
            let rec inner acc lst = 
                match lst with
                    | (_::_)::_ -> inner (List.map List.head lst :: acc) (List.map List.tail lst)
                    | _         -> List.rev acc
            inner [] lst

        let transpose (mtx : _ [,]) func = Array2D.init (mtx.GetLength 1) (mtx.GetLength 0) (fun x y -> func(mtx.[y,x]))


        type System.Array with
            member this.zeroLength =
                this.Length - 1
            
        
            
        