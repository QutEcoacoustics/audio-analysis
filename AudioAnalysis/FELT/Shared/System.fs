namespace System

    open Microsoft.FSharp
    open Microsoft.FSharp.Math
    open Microsoft.FSharp.Math.SI 

    [<AutoOpen>]
    module Extensions =
        type TimeSpan with
            static member FromMilliseconds ms =
                ms |> float |> System.TimeSpan.FromMilliseconds
            static member FromSeconds (ms:int<_>) =
                ms |> float |> System.TimeSpan.FromSeconds
            member this.ToSeconds : float<s> =
                LanguagePrimitives.FloatWithMeasure (this.TotalSeconds) 
            member this.DivideBy x =
                TimeSpan.FromTicks <| this.Ticks / x

        // tuples
        let map (x,y) f = (f x), (f y)
        let apply (x,y) v  = (x v), (y v)


        type System.Double with
            member this.Minutes =
                TimeSpan.FromMinutes this

            member this.To y =
                Interval.create  this y

        type System.Int32 with
            member this.Minutes =
                this |> float |> TimeSpan.FromMinutes

            member this.To y =
                Interval.create this y

        
        type 'a ``[]`` with
            member this.zeroLength =
                this.Length - 1
            


