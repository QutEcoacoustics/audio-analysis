#light

module Common

open Util.Array2

let loadTestFile f = fileToMatrix (@"matlab\" + f) 256 5188
