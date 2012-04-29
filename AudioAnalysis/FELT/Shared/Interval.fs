
namespace Microsoft.FSharp.Math
    
    type Interval<'a when 'a : comparison>(lower:'a, upper:'a) =
        do
            if lower > upper then
                invalidArg "lower" "interval cannot have a lower bound that is greater than the upper boud"

        member this.Lower = lower
        member this.Upper = upper

         
    
    module Interval =
        let inline lower (iv:Interval<_>) = iv.Lower
        let inline upper (iv:Interval<_>) = iv.Upper
        let inline difference (iv:Interval<_>) = iv.Upper - iv.Lower
        let inline create lower upper = new Interval<_>(lower, upper)
