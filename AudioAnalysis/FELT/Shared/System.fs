namespace System

    module Doowacker =
        let stub = ()

    [<AutoOpen>]
    module Extensions =
        type TimeSpan with
            static member FromMilliseconds ms =
                ms |> float |> System.TimeSpan.FromMilliseconds
            static member FromSeconds (ms:int<_>) =
                ms |> float |> System.TimeSpan.FromSeconds

        // tuples
        let map (x,y) f = (f x), (f y)
        let apply (x,y) v  = (x v), (y v)
