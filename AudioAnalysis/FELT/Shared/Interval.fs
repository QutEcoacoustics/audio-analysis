
namespace Microsoft.FSharp.Math
    open System
    open Option


    [<StructuralEqualityAttribute; CustomComparison>]
    type Limit<'a  when 'a : comparison> =
        | Finite of 'a
        | PositivelyInfinite
        | NegativelyInfinite   
                 
        interface IComparable<Limit<'a>> with
            member this.CompareTo y =
                match this, y with
                    | Finite a          , Finite b              -> compare a b
                    | Finite _          , PositivelyInfinite    -> -1
                    | Finite _          , NegativelyInfinite    ->  1
                    | PositivelyInfinite, Finite _              ->  1
                    | NegativelyInfinite, Finite _              -> -1
                    | PositivelyInfinite, NegativelyInfinite    ->  1
                    | NegativelyInfinite, PositivelyInfinite    -> -1
                    | PositivelyInfinite, PositivelyInfinite  
                    | NegativelyInfinite, NegativelyInfinite    ->  0
        interface System.IComparable with
            member this.CompareTo yobj =
                match yobj with
                    | :? Limit<'a> as y -> (this :> IComparable<Limit<'a>>).CompareTo(y)
                    | _ -> invalidArg "yobj" "cannot compare values of different types"
                           
        member this.isFinite  = match this with Finite _ -> true | _ -> false
        member this.isInfinite  = match this with PositivelyInfinite | NegativelyInfinite _ -> true | _ -> false
        member this.Value =
            match this with
                | Finite x -> x
                | _ -> raise <| new NotFiniteNumberException("This Limit value has an infinite value, getting a real value is not supported")

    module Limit =
        let inline getFiniteValue (l:Limit<_>) = l.Value
        let inline isFinite (l:Limit<_>) = l.isFinite
        let inline isInfinite (l:Limit<_>) = l.isInfinite
        let (|IsFinite|_|) x =
            if isFinite x then
                x |> getFiniteValue |> Some
            else
                None 
    open Limit

    type Interval<'a when 'a : comparison>(intervalType: IntervalFuncFull<_>, ?lower:'a Limit, ?upper:'a Limit) =
        static let pinf = "+∞"
        static let ninf = "-∞"
        static let emptySet = "Ø"
        static let print o = 
            match o with
            | Finite x -> x.ToString()
            | PositivelyInfinite -> pinf
            | NegativelyInfinite -> ninf                

        let l, u, empty =
            match lower, upper with
                | None  , None   -> Limit.NegativelyInfinite,  Limit.PositivelyInfinite,  true
                | None  , Some u -> u, u, false
                | Some l, None   -> l, l, false
                | Some l, Some u -> 
                    if isFinite l && isFinite u  && (getFiniteValue l) >  (getFiniteValue u) then
                            invalidArg "lower" (sprintf "interval cannot have a lower bound that is greater than the upper bound (lower: %A, upper: %A)" lower upper)
                    l, u, false

        let tie() = if empty then raise <| new InvalidOperationException("Can't call this member because this Interval is empty")
        let infinate = not empty && (l |> isInfinite) && (u |> isInfinite)
        do
            // lower and upper can have only certain values
            match l, u with
                | PositivelyInfinite, _ -> raise <| new ArithmeticException("The lower bound can not be PostivelyInfinity")
                | _, NegativelyInfinite -> raise <| new ArithmeticException("The upper bound can not be NegativeInfinity")
                | _ -> ()

        member this.Lower = tie(); l |> getFiniteValue
        member this.Upper = tie(); u |> getFiniteValue
        member this.LowerSafe = tie(); l
        member this.UpperSafe = tie(); u
        member this.IsEmpty = empty
        member this.IsInfinite = infinate
        member this.IsBounded = isFinite l && isFinite u
        member this.Type = intervalType
        member this.IsIn x = intervalType this x

        override this.ToString() =
            this.ToString(intervalType)
        member this.ToString (intervalType: IntervalFuncFull<'a>) =
            match this with
                | this when this.IsEmpty ->
                    emptySet                    
                | _ ->
                    let openChar = if intervalType this (this.LowerSafe) then  "[" else "("
                    let closeChar =  if intervalType this (this.UpperSafe) then  "]" else ")"
                    openChar +  (print l) + ", " + (print u) + closeChar
    and
        IntervalFunc<'a when 'a : comparison> = Interval<'a> -> ('a -> bool)
    and
        IntervalFuncFull<'a when 'a : comparison> = Interval<'a> -> ('a Limit -> bool)
    
    module Interval =              
       
        let inline isEmpty (iv:Interval<_>) = iv.IsEmpty
        let inline isInfinite (iv:Interval<_>) = iv.IsInfinite

        let inline isUnboundedLeft  (iv:Interval<_>) = iv.LowerSafe.isInfinite
        let inline isUnboundedRight (iv:Interval<_>) = iv.UpperSafe.isInfinite
        let inline isUnbounded (iv:Interval<_>) = not iv.IsBounded

        let inline lower (iv:Interval<_>) = iv.Lower
        let inline upper (iv:Interval<_>) = iv.Upper
        let inline left (iv:Interval<_>) = iv.LowerSafe
        let inline right (iv:Interval<_>) = iv.UpperSafe

        /// http://en.wikipedia.org/wiki/Interval_(mathematics)#Classification_of_intervals
        module IntervalTypes =
            open Limit
            
            /// (
            let inline leftOpen     i x =  x >  left i
            /// [
            let inline leftClosed   i x =  x >= left i
            /// )
            let inline rightOpen    i x =  x <  right i
            /// ]
            let inline rightClosed  i x =  x <= right i
                                                                                                                  
            let inline bothOpen            i x = leftOpen   i x && rightOpen   i x
            let inline bothClosed          i x = leftClosed i x && rightClosed i x
            let inline leftClosedRightOpen i x = leftClosed i x && rightOpen   i x
            let inline leftOpenRightClosed i x = leftOpen   i x && rightClosed i x

            let standard = leftClosedRightOpen

            let inline degenerate i x = not (isUnbounded i) && (lower i) = x && x = (upper i)

            let inline empty i x = false

        let inline empty<'a when 'a : comparison> : Interval<'a>    = new Interval<'a>(IntervalTypes.empty)
        let inline infinite<'a when 'a : comparison> : Interval<'a> = new Interval<'a>(IntervalTypes.bothOpen, Limit.NegativelyInfinite , Limit.PositivelyInfinite)
        let inline create lower upper                               = new Interval<_> (IntervalTypes.standard, Finite(lower)            , Finite(upper))
        let inline createLeftUnbounded upper                        = new Interval<_> (IntervalTypes.standard, Limit.NegativelyInfinite , Finite(upper)) 
        let inline createRightUnbounded lower                       = new Interval<_> (IntervalTypes.standard, Finite(lower)            , Limit.PositivelyInfinite)

        let inline difference iv = 
            match iv with 
                | iv when isEmpty iv-> 0G
                | iv when isInfinite iv -> raise <| NotFiniteNumberException("The difference of this interval is +∞, this is not representable in a generic form")
                | _  ->
                    iv.Upper - iv.Lower

        let difference2 iv = 
            match iv with 
                | iv when isEmpty iv-> 0.0
                | iv when isUnbounded iv -> infinity
                | _  ->
                    iv.Upper - iv.Lower

        let inline midpoint iv =
            match iv with
                | iv when isEmpty iv -> None
                | iv when isUnbounded iv -> None
                |_ ->
                    (iv.Lower + iv.Upper) / 2G

        let inline radius iv =
            match iv with
                | iv when isEmpty iv -> None
                | iv when isUnbounded iv -> None
                |_ ->
                    abs (iv.Lower - iv.Upper) / 2G

        let inline isInRange i x = IntervalTypes.leftOpenRightClosed i (Finite x)
        let inline isInLimit (i:Interval<_>) x = i.IsIn x
        let inline isIn (i:Interval<_>) x = i.IsIn (Finite x)
        let inline isInThisBound (it:IntervalFunc<_>) i x = it i x

        let inline rescale (oldRange:Interval<'a>) (newRange:Interval<'b>) (v:'a) : 'b =
            if not (oldRange.IsBounded && newRange.IsBounded) then
                raise <| ArithmeticException("Cannot rescale on an unbounded interval")
            let fraction : 'c = (v - oldRange.Lower) / (difference oldRange)
            let newValue : 'b = (fraction  * (difference newRange)) + newRange.Lower
            newValue

            

    module test =
        open Interval.IntervalTypes

        let i = Interval.create 1 010



        let ie = Interval.empty<float>

        let ifin = Interval.infinite<float>

        let ifin2 = Interval.createLeftUnbounded 1 
        let ifin3 = Interval.createRightUnbounded 1 

        let check() =
            
            printfn "%s" <| i.ToString(bothOpen)                 //= "[1, 10]"
            printfn "%s" <| i.ToString(bothClosed)               //= "(1, 10)"
            printfn "%s" <| i.ToString(leftOpenRightClosed)      //= "[1, 10)"
            printfn "%s" <| i.ToString(leftClosedRightOpen)      //= "(1, 10]"
            printfn "%s" <| ie.ToString(bothOpen)                //= "Ø"
            printfn "%s" <| ie.ToString(bothClosed)              //= "Ø"
            printfn "%s" <| ie.ToString(leftOpenRightClosed)     //= "Ø"
            printfn "%s" <| ie.ToString(leftClosedRightOpen)     //= "Ø"
            printfn "%s" <| ifin.ToString(bothOpen)              //= "[-∞, +∞]"
            printfn "%s" <| ifin.ToString(bothClosed)            //= "(-∞, +∞)"
            printfn "%s" <| ifin.ToString(leftOpenRightClosed)   //= "[-∞, +∞)"
            printfn "%s" <| ifin.ToString(leftClosedRightOpen)   //= "(-∞, +∞]"
            printfn "%s" <| ifin2.ToString(bothOpen)             //= "[-∞, 1]"
            printfn "%s" <| ifin2.ToString(bothClosed)           //= "(-∞, 1)"
            printfn "%s" <| ifin2.ToString(leftOpenRightClosed)  //= "[-∞, 1)"
            printfn "%s" <| ifin2.ToString(leftClosedRightOpen)  //= "(-∞, 1]"
            printfn "%s" <| ifin3.ToString(bothOpen)             //= "[1, +∞]"
            printfn "%s" <| ifin3.ToString(bothClosed)           //= "(1, +∞)"
            printfn "%s" <| ifin3.ToString(leftOpenRightClosed)  //= "[1, +∞)"
            printfn "%s" <| ifin3.ToString(leftClosedRightOpen)  //= "(1, +∞]"

            ()
   
