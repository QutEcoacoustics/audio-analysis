module QutSensors.AudioAnalysis.AED.LargeEvents
    
open GetAcousticEvents
open Matlab
open Util
    
let lastMin cs h = 
    let s = Seq.pairwise h |> Seq.map (fun (x,y) -> y-x) |> Seq.zip ( Seq.skip 1 cs)
    Seq.take (Seq.length s - 1) s |> Seq.fold (fun z (c,x) -> if x < 0 then c else z) (Seq.hd cs)
    
let threshold rs =
    let cs = seq {for i in 0..10 -> i * 1000}
    let t = hist (areas rs) cs |> lastMin cs
    let d = 3000
    if t < d then d else t
    
let aeToMatrix ae =
    let r = ae.Bounds
    Math.Matrix.init r.Height r.Width (fun i j -> if ae.Elements.Contains (r.Top + i, r.Left + j) then 1.0 else 0.0)

let separateLargeEvents aes =
    let areat = bounds aes |> threshold
    let f ae =
        let freqt = 20.0
        let m = aeToMatrix ae
        let s = sumRows m |> Seq.map (fun x -> x / (float) m.NumCols * 100.0 <= freqt) 
        let m1 = Math.Matrix.mapi (fun i _ x -> if Seq.nth i s then 0.0 else x) m
        let rs = getAcousticEvents m1
                 |> List.map (fun x -> let b1, b2 = ae.Bounds, x.Bounds in {Left=b1.Left+b2.Left; Top=b1.Top+b2.Top; Width=b2.Width; Height=b2.Height})
                    
        let timet = 100.0 / 3.0
        let m2 = m - m1
        rs @ (getAcousticEvents m2
              |> List.filter (fun x -> (float) x.Bounds.Height * 100.0 / (float) m2.NumRows >= timet)
              |> List.map (fun x -> let b1, b2 = ae.Bounds, x.Bounds in {Left=b1.Left+b2.Left; Top=b1.Top; Width=b2.Width; Height=b1.Height}))
    Seq.collect (fun ae -> if area ae.Bounds < areat then [ae.Bounds] else f ae) aes