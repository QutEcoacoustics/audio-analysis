(*
    Operator tools
    Author: Anthony Truskinger

*)

namespace Felt.Shared

    open System

    module Utilities =

        let inline (@@) (a: 'a) (b: 'a array) =
            Array.append [|a|] b

        let (|Rest|_|) (input: seq<'a>) =
                match Seq.length input with
                    | 0 -> Option.None
                    | 1 -> Option.Some(Seq.nth 0 input, Seq.empty)
                    | _ -> Option.Some(Seq.nth 0 input, Seq.skip 1 input) 
        
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