#light

module Common

open Util.Array2

let loadTestFile2 f i j = fileToMatrix (@"matlab\" + f) i j

let loadTestFile f = loadTestFile2 f 256 5188
