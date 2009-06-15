#light

module AcousticEventDetection

open Math.Matrix

let toBlackAndWhite t = map (fun e -> if e > t then 1.0 else 0.0)