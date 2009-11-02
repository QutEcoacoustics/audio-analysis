module Common

open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open QutSensors.AudioAnalysis.AED.Util
                       
type TestMetadata = {Dir:string; BWthresh:double; smallThreshIn:int; smallThreshOut:int}
let BAC2_20071015_045040 =
    {Dir="BAC2_20071015-045040"; BWthresh=9.0; smallThreshIn=200; smallThreshOut=130}
let GParrots_JB2_20090607_173000_wav_minute_3 =
    {Dir="GParrots_JB2_20090607-173000.wav_minute_3"; BWthresh=3.0; smallThreshIn=100; smallThreshOut=55}
                          
//let testAll f = Seq.iter f [BAC2_20071015_045040; GParrots_JB2_20090607_173000_wav_minute_3]
let testAll f = Seq.iter f [BAC2_20071015_045040]
                
// Expects the current directory to be trunk\AudioAnalysis\AED\Test
let loadTestFile2 d f = csvToMatrix (@"matlab\" + d + @"\" + f) 

let loadTestFile f md = loadTestFile2 md.Dir f 

let loadEventsFile f md =
    let aem = loadTestFile2 md.Dir f 
    // matlab matrix indicies are 1 based, F# is 0 based
    let dec x = (int x) - 1
    seq {for i in 0..(aem.NumCols-1) -> {Left=dec aem.[0,i]; Top=dec aem.[1,i]; Width=(int) aem.[2,i]; Height=(int) aem.[3,i]}}
    
let floatEquals f1 f2 d = abs(f1 - f2) <= d
        
// TODO would rather use Either than an exception here
let matrixFloatEquals (a:matrix) (b:matrix) d = 
    // TODO blow up if not same size (a.GetLength(0) = b.GetLength(0) etc)
     for i=0 to (a.NumRows-1) do
       for j=0 to (a.NumCols-1) do
         let fe = floatEquals a.[i,j] b.[i,j] d
         if not fe then failwith (sprintf "Floats at [%d,%d] not equal to distance %f: %f %f" i j d a.[i,j] b.[i,j])
       done
     done
     true
     