namespace Microsoft.FSharp.Collections
    
    open System
    
    module Set = 

        let countSubsets s =
            Set.count s |> pown 2 |> decrement

        let countSubsetsIncludeEmpty s =
            pown 2 (Set.count s)

        let powerset s = 
            let rec loop n l =
                seq {
                      match n, l with
                      | 0, _  -> yield []
                      | _, [] -> ()
                      | n, x::xs -> yield! Seq.map (fun l -> x::l) (loop (n-1) xs)
                                    yield! loop n xs
                }   
            let xs = s |> Set.toList     
            seq {
                for i = 0 to List.length xs do
                    for x in loop i xs -> set x
            }
            
