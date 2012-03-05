namespace MQUTeR.FSharp.Shared
    open System
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Numerics

    module Maths =
        
        let integerZOps : INumeric<IntegerZ1440> = 
            IntegerZ1440Associations.Init()
            GlobalAssociations.GetNumericAssociation()
        
        let inline square x = pown x (1G + 1G)

        let distFloat (q:float) p = (q - p)
        let distIntegerZ q p = (min (integerZOps.Subtract(q, p)) (integerZOps.Subtract(p,q))).ToFloat()
        
        /// Note: euclidean distance produce floats no matter what input is given
        /// a unique dist function is run for each data type
        /// after that the distances are mapped to a cartesian plane
        let inline euclideanDist (vectorP:seq<'a>) (vectorQ:seq<'a>) : float =
            let P = Array.ofSeq vectorP
            let Q = Array.ofSeq vectorQ
            
            let op (a:obj) (b:obj) =
                let c = match a, b with
                        | (:? float  as p), (:? float  as q) -> distFloat q p
                        | (:? int  as w), (:? int  as x) -> distFloat (float x) (float w)
                        | (:? float  as w), (:? int  as x) -> distFloat (float x) w
                        | (:? int  as w), (:? float  as x) -> distFloat x (float w)
                        | (:? IntegerZ1440  as u), (:? IntegerZ1440  as v)-> distIntegerZ v u
                        | _ -> failwith "unknown data type"
                let d = square c
                d
            Array.map2 op P Q
            |> Seq.sum
            |> sqrt

        /// Note: Z-scores produce floats no matter what input is given
        let inline zscore sample mean stdev =
            let a =(float ( sample - mean))
            let b = (float stdev)
            a / b

        module Array =
            open NumericLiteralG

            let inline sum xs = Array.fold (+) 0G xs

            let inline mean (xs: 'a array) : 'a = 
                let s = sum xs 
                //let n = (float (Array.length xs))
                let n = Array.length xs
                LanguagePrimitives.DivideByInt s  n
                

            let inline add_by f (a) n = a + (f n) 

            let inline sum_by f xs = Array.fold (add_by f) 0G xs
            let inline sum_by2 f xs ys = Array.fold2 (fun state a b -> (f a b) + state) 0G xs ys

            let inline mean_by f xs = sum_by f xs / op_Explicit (Array.length xs)

            let inline variance xs = let m = mean xs in mean_by (fun x -> let e = (x - m) in e * e) xs

            let inline stdDeviation xs = sqrt (variance xs)

            let inline skewness xs =
                let n = double(Array.length xs)
                let m = mean xs
                let s = stdDeviation xs
                mean_by (fun x -> (x-m) ** 3.0) xs / (s ** 3.0)

            let inline covariance xs ys =
                let m = mean xs
                let n = mean ys
                sum_by2 (fun x y -> (x-m) * (y-n)) xs ys / float(n)

            let inline correlation xs ys =
                covariance xs ys / (mean xs * mean ys)



        module List =
            let sum xs = List.fold (+) 0.0<_> xs
            let mean xs = sum xs / double (List.length xs)
    


            let rec sum_by f xs = match xs with [] -> 0.0<_> | (x::xs) -> f x + sum_by f xs
            let rec sum_by2 f xs ys = match xs,ys with (x::xs, y::ys) -> f x y + sum_by2 f xs ys | _ -> 0.0<_>

            let mean_by f xs = sum_by f xs / double (List.length xs)

            let variance xs = let m = mean xs in mean_by (fun x ->  (x-m) ** 2.0) xs
            let stdDeviation xs = sqrt (variance xs)

            let skewness xs =
                let n = double( List.length xs)
                let m = mean xs
                let s = stdDeviation xs
                mean_by (fun x -> (x-m) ** 3.0) xs / (s ** 3.0)

            let covariance xs ys =
                let m = mean xs
                let n = mean ys
                sum_by2 (fun x y -> (x-m) * (y-n)) xs ys / float(n)

            let correlation xs ys =
                covariance xs ys / (mean xs * mean ys)
