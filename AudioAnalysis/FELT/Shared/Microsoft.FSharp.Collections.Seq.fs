namespace Microsoft.FSharp.Collections
    open System
    open Microsoft.FSharp.Collections
    open System.Linq

    [<AutoOpen>]
    module Seq =
            

            let inline (++) x y = Seq.append x y
            let inline (+.) x y = Seq.append x [|y|]
            let inline (.+) x y = Seq.append [|x|] y
            let inline (.+.) x y = [| x; y |] :> seq<_>

            let tupleWith y xs =
                Seq.map (fun x -> (x, y)) xs

            type Count = int
            let ZeroCount:Count = 0

            let first xs = Seq.nth 1 xs
            let second xs = Seq.nth 2 xs
            let third xs = Seq.nth 3 xs
            let fourth xs = Seq.nth 4 xs
            let fifth xs = Seq.nth 5 xs

            let mapJagged f = Seq.map (Seq.map f)

            

            let histogramBy f bins collection =
                let m = bins |> tupleWith 0 |>  Map.ofSeq
                let g (map:Map<_, Count>) item = 
                    let x = f item
                    // this is meant to throw an exception if element does not exist (i.e. there is no bin)
                    let count = map.[x]
                    Map.add x (count + 1) map
 
                PSeq.fold (g) m collection