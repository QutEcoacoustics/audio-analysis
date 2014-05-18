namespace Microsoft.FSharp.Collections
    open System
    open Microsoft.FSharp.Collections
    open System.Linq

    [<AutoOpen>]
    module Seq =
            
            /// Concat two seqs
            let inline (++) x y = Seq.append x y
            
            /// Append an element to a seq
            let inline (+.) x y = Seq.append x [|y|]
            
            /// Prepend an element to a seq
            let inline (.+) x y = Seq.append [|x|] y
            
            /// Combine two elements into a seq
            let inline (.+.) x y = [| x; y |] :> seq<_>

            let tupleWith y xs =
                Seq.map (fun x -> (x, y)) xs

            type Count = int
            let ZeroCount:Count = 0

            let tryHead xs = Seq.tryPick Some xs
            
            let first xs = Seq.nth 0 xs
            let second xs = Seq.nth 1 xs
            let third xs = Seq.nth 2 xs
            let fourth xs = Seq.nth 3 xs
            let fifth xs = Seq.nth 4 xs

            let mapJagged f = Seq.map (Seq.map f)

            let histogramBy f bins collection =
                let m = bins |> tupleWith 0 |>  Map.ofSeq
                let g (map:Map<_, Count>) item = 
                    let x = f item
                    // this is meant to throw an exception if element does not exist (i.e. there is no bin)
                    let count = map.[x]
                    Map.add x (count + 1) map
 
                PSeq.fold (g) m collection

            module Parallel =
                open System.Threading.Tasks

                let iter f (seq : seq<'T>) =
                    let p = new ParallelOptions() 
                    p.MaxDegreeOfParallelism <- p.TaskScheduler.MaximumConcurrencyLevel
                    Parallel.ForEach(seq, p, fun i -> f i) |> ignore
                    
                    //Parallel.For (0, array.Length, fun i -> f array.[i]) |> ignore  
                
                let iteri f (seq : seq<'T>) =
                    let p = new ParallelOptions() 
                    p.MaxDegreeOfParallelism <- p.TaskScheduler.MaximumConcurrencyLevel
                    
                    Parallel.ForEach(seq, p, fun x pls index -> f index x)  |> ignore
                    
                    //Parallel.For (0, array.Length, fun i -> f array.[i]) |> ignore  

//                let itera f (array : 'T[]) =
//                    checkNonNull "array" array
//                    Parallel.For (0, array.Length, fun i -> f array.[i]) |> ignore  
//            
//                let iters f s = 
//                    ParallelEnumerable.ForAll(toP(s), Action<_>(f))