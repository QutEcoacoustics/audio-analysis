namespace MQUTeR.FSharp.Shared

    module Maths =
        let square x = pown x 2
        
        let euclideanDist vectorP vectorQ =
            let op a b =  square (a - b) 
            Seq.map2 op vectorP vectorQ
            |> Seq.sum
            |> sqrt

