module Common

open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open QutSensors.AudioAnalysis.AED.Util
    
let loadTestFile2 f i j = fileToMatrix (@"C:\Documents and Settings\Brad\svn\Sensors\trunk\AudioAnalysis\AED\Test\matlab\" + f) i j

let loadTestFile f = loadTestFile2 f 256 5188

let loadEventsFile f j =
    // matlab matrix indicies are 1 based, F# is 0 based
    let aem = loadTestFile2 f 10 j
    let dec x = (int x) - 1
    seq {for i in 0..(j-1) -> {Left=dec aem.[0,i]; Top=dec aem.[1,i]; Width=(int) aem.[2,i]; Height=(int) aem.[3,i]}}
    
let floatEquals f1 f2 d = abs(f1 - f2) <= d
        
// TODO would rather use Either than an exception here
let a2FloatEquals (a:float[,]) (b:float[,]) d = 
    // TODO blow up if not same size (a.GetLength(0) = b.GetLength(0) etc)
     for i=0 to (a.GetLength(0)-1) do
       for j=0 to (a.GetLength(1)-1) do
         let fe = floatEquals a.[i,j] b.[i,j] d
         if not fe then failwith (sprintf "Floats at [%d,%d] not equal to distance %f: %f %f" i j d a.[i,j] b.[i,j])
       done
     done
     true