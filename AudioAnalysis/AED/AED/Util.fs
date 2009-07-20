module QutSensors.AudioAnalysis.AED.Util.Core

(* If the first Option is not empty return it, else return the second.
   Copy of Scala Option.orElse.
*)
let (|?) o p = if Option.isSome o then o else p 

(* If the Option is not empty return its value, otherwise return d.
   Copy of Scala Option.getOrElse
*)
let (|?|) o d = match o with | Some x -> x | _ -> d