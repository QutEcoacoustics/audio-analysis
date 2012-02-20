namespace MQUTeR.FSharp.Shared.Stats

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



    module Array =
        let sum xs = Array.fold (+) 0.0<_> xs
        let mean xs = sum xs / double (Array.length xs)

        let add_by f (a:float<'u>) n = a + (f n) 

        let sum_by f xs = Array.fold (add_by f) 0.0<_> xs
        let sum_by2 f xs ys = Array.fold2 (fun state a b -> (f a b) + state) 0.0<_> xs ys

        let mean_by f xs = sum_by f xs / double (Array.length xs)

        let variance xs = let m = mean xs in mean_by (fun x -> let e = (x - m) in e * e) xs

        let stdDeviation xs = sqrt (variance xs)

        let skewness xs =
            let n = double(Array.length xs)
            let m = mean xs
            let s = stdDeviation xs
            mean_by (fun x -> (x-m) ** 3.0) xs / (s ** 3.0)

        let covariance xs ys =
            let m = mean xs
            let n = mean ys
            sum_by2 (fun x y -> (x-m) * (y-n)) xs ys / float(n)

        let correlation xs ys =
            covariance xs ys / (mean xs * mean ys)

        let zscore (sample:float<_>) mean stdev =
            (sample - mean) / stdev