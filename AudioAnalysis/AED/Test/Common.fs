module Common

open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open Util.Array2

let loadTestFile2 f i j = fileToMatrix (@"matlab\" + f) i j

let loadTestFile f = loadTestFile2 f 256 5188

let loadEventsFile f j =
    // matlab matrix indicies are 1 based, F# is 0 based
    let aem = loadTestFile2 f 6 j
    let dec x = (int x) - 1
    seq {for i in 0..(j-1) -> {Left=dec aem.[0,i]; Top=dec aem.[1,i]; Width=(int) aem.[2,i]; Height=(int) aem.[3,i]}}