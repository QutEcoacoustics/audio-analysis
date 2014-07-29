﻿module Common

open System.IO

open QutSensors.AudioAnalysis.AED.Util
open Xunit
open System
open System.Reflection
                       
type TestMetadata = {Dir:string; BWthresh:double; smallThreshIn:int; smallThreshOut:int}
let BAC2_20071015_045040 =
    {Dir="BAC2_20071015-045040"; BWthresh=9.0; smallThreshIn=200; smallThreshOut=130}
let GParrots_JB2_20090607_173000_wav_minute_3 =
    {Dir="GParrots_JB2_20090607-173000.wav_minute_3"; BWthresh=3.0; smallThreshIn=100; smallThreshOut=35}
                          
let testAll f = Seq.iter f [BAC2_20071015_045040; GParrots_JB2_20090607_173000_wav_minute_3]
                


/// Sets the current directory to be trunk\AudioAnalysis\AED\Test
let matlabPath = @"..\..\..\..\AED\Test\matlab\"

let basePath relativePath = 
    let codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
    let codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
    let dirPath = Path.GetDirectoryName(codeBasePath);
    Path.Combine(dirPath, relativePath);

let loadTestFile2 d f = 
    let p = Path.Combine( (basePath matlabPath), d, f)
    csvToMatrix p 

let loadTestFile f md = loadTestFile2 md.Dir f 

let loadIntEventsFile f md =
    let aem = loadTestFile2 md.Dir f 
    // matlab matrix indicies are 1 based, F# is 0 based
    let dec x = (int x) - 1
    seq {for i in 0..(aem.NumCols-1) -> lengthsToRect (dec aem.[0,i]) (dec aem.[1,i]) ((int) aem.[2,i]) ((int) aem.[3,i])}
    
let loadFloatEventsFile f md =
    let aem = loadTestFile2 md.Dir f 
    seq {for i in 0..(aem.NumCols-1) -> cornersToRect aem.[0,i] (aem.[0,i]+aem.[1,i]) aem.[3,i] aem.[2,i]}
        
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
     
let defToString x = sprintf "%A" x
let rectToString r = sprintf "%f, %f, %f, %f" (left r) (right r) (bottom r) (top r)

let seqEqual eq toS xs' ys' =
    let xs, ys = Seq.sort xs', Seq.sort ys'
    let l = if Seq.length xs = Seq.length ys then None else sprintf "Lengths differ %i vs %i" (Seq.length xs) (Seq.length ys)|> Some
    let bs = Seq.map2 eq xs ys
    let c = 
        if Seq.forall id bs 
        then 
            [None]
        else 
            let is =  Seq.choose  (fun i -> if not (Seq.nth i bs) then Some(i) else None) {0..(Seq.length(bs)-1)}
            let f (i:int) = 
                let i' = i + 1
                sprintf "%i th difference at position %i :\r\n" i i +
                    sprintf "\t Expected[%i]:\t%s\tFound[%i]:\t%s" i (Seq.nth i xs |> toS) i (Seq.nth i ys |> toS) +
                    (if i' < Seq.length ys then sprintf "\tFound[%i]:\t%s" i' (Seq.nth i' ys |> toS) else "" )
            [(Seq.map f is) |> String.concat "\r\n" |> Some ]       
    catOptions (l::c)

let assertSeqEqual eq toS xs ys =
    let m = seqEqual eq toS xs ys
    if Seq.isEmpty m then 
        Assert.True(true) 
    else
        Assert.True(false, "\r\n\r\n" + (String.concat "\r\n\r\n" m) + "\r\n" )


