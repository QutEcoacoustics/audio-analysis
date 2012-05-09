
namespace Microsoft.FSharp.Math
    open System


    type Interval<'a when 'a : comparison>(lower:'a, upper:'a) =
        do
            if lower > upper then
                invalidArg "lower" (sprintf "interval cannot have a lower bound that is greater than the upper bound (lower: %A, upper: %A)" lower upper)

        member this.Lower = lower
        member this.Upper = upper

        override this.ToString() =
            "(" + lower.ToString() + ", " + upper.ToString() + ")"
    
    
    module Interval =
        let inline lower (iv:Interval<_>) = iv.Lower
        let inline upper (iv:Interval<_>) = iv.Upper
        let inline difference (iv:Interval<_>) = iv.Upper - iv.Lower
        let inline create lower upper = new Interval<_>(lower, upper)

        module IntervalTypes =
            type IntervalFunc<'a when 'a : comparison> = Interval<'a> -> ('a -> bool)

            let inline leftOpen     i x = x >  (lower i)
            let inline leftClosed   i x = x >= (lower i) 
            let inline rightOpen    i x = x <  (upper i)
            let inline rightClosed  i x = x <= (upper i)
                                      
            let inline bothOpen     i x = leftOpen   i x && rightOpen   i x
            let inline bothClosed   i x = leftClosed i x && rightClosed i x
            let inline leftOpenRightClosed i x = leftOpen i x && rightClosed i x
            let inline leftClosedRightOpen i x = leftClosed i x && rightOpen i x

            let unbounded i v = not <| Double.IsNaN(v)

        let inline isInRange i x = IntervalTypes.leftOpenRightClosed i x
        let inline isInInterval (it:IntervalTypes.IntervalFunc<_>) i x = it i x