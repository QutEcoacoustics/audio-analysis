namespace MQUTeR.FSharp.Shared


    open Accord.Statistics
    open System
    open System.Diagnostics
    open System.Reflection
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Numerics
    open Microsoft.FSharp.Collections

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

            let inline mean_by f xs = 
                let s = sum_by f xs 
                LanguagePrimitives.DivideByInt s (Array.length xs)

            let inline variance xs = let m = mean xs in mean_by (fun x -> let e = (x - m) in e * e) xs
            let inline varianceAndMean xs = 
                let m = mean xs
                mean_by (fun x -> let e = (x - m) in e * e) xs, m

            let inline stdDeviation xs = sqrt (variance xs)

            let inline stdDeviationAndMean xs =
                let var, mean = varianceAndMean xs
                sqrt(var), mean


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


       // module AnalysticalStats =
//            let ConfusionMatrix =
//                //let cm = new Analysis.ConfusionMatrix()
//                raise (new NotImplementedException())
//                //cm
//                3

        module RocCurve =
            open Accord.Statistics.Analysis

            type xPoint = double
            type yPoint = double

            let RocScore (measurements:double[]) (predictions:double[]) (numberOfIncrements:int) =
                if Array.length measurements  <> Array.length predictions then
                    raise (new System.ArgumentOutOfRangeException())

                // do check for all same elements
                let first = measurements.[0]
                if Array.exists ((<>) first) measurements then

                    let roc = new ReceiverOperatingCharacteristic(measurements, predictions)
                    // Compute a points for every place                
                    roc.Compute(numberOfIncrements)

                    roc
                else
                    failwith "All measurements same value.. function cannot cope"

            // using trapezium method
            let AreaUnderCurve xs ys =
                let xsLength = Array.length xs
                if xsLength <> Array.length ys then
                    invalidArg "" "Sequence length must be the same for xs and ys"

                let calcArea index state x1 y1 =
                    if (index = xsLength - 1) then
                        state
                    else
                        let x2 = xs.[index + 1]
                        let y2 = ys.[index + 1]

                        let xd = x2 - x1
                        let yd = y2 + y1

                        let a = (xd * yd)
                        state + a
                let area= Array.fold2i calcArea 0.0 xs ys
                area  / 2.0
            
            let rocCurveToPlacingAUC (roc: ReceiverOperatingCharacteristic) =
                let rocPoints = roc.Points |> Seq.toArray |> Array.sortBy (fun x -> (x).Cutoff) |> Array.rev
                //let xs = Seq.scan (fun state (rocp:ReceiverOperatingCharacteristicPoint) -> state + (float rocp.TruePositives)) 0.0 roc.Points |> Seq.toArray
                let xs = Array.map (fun (rocp:ReceiverOperatingCharacteristicPoint) -> 1.0 - (if Double.IsInfinity rocp.Cutoff then 1.0 else rocp.Cutoff)) rocPoints 
                let ys = Array.map (fun (rocp:ReceiverOperatingCharacteristicPoint) -> rocp.Sensitivity) rocPoints
                let area = AreaUnderCurve xs ys
                
                1.0 - area, xs, rocPoints

            let PrintRocCurvePoint (p:Accord.Statistics.Analysis.ReceiverOperatingCharacteristicPoint) = 
                let _, props = ReflectionHelpers.iterateProperties p (fun name value -> name, value)
                Array.unzip props

