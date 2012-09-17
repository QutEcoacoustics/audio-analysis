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