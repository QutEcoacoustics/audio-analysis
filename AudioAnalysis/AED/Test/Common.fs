module Common

open FsCheck
open FsCheckXunit
open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open QutSensors.AudioAnalysis.AED.Util

// TODO uncurry3                
let test f = Seq.iter (fun (d, i, j) -> f d i j) [("BAC2_20071015-045040", 256, 5188);
                                                  ("GParrots_JB2_20090607-173000.wav_minute_3", 256, 5166)
                                                  ]
                
let loadTestFile2 f i j = fileToMatrix (@"C:\Documents and Settings\Brad\svn\Sensors\trunk\AudioAnalysis\AED\Test\matlab\" + f) i j |> Math.Matrix.of_array2

let loadTestFile3 d f i j = loadTestFile2 (d + @"\" + f) i j

let loadTestFile f = loadTestFile2 f 256 5188

let loadEventsFile d f j =
    // matlab matrix indicies are 1 based, F# is 0 based
    let aem = loadTestFile3 d f 10 j
    let dec x = (int x) - 1
    seq {for i in 0..(j-1) -> {Left=dec aem.[0,i]; Top=dec aem.[1,i]; Width=(int) aem.[2,i]; Height=(int) aem.[3,i]}}
    
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
     
// FsCheck

let nonNeg = arbitrary |> fmapGen abs

let pos = nonNeg |> fmapGen ((+) 1)

let pairGen = liftGen2 (fun x y -> (x,y))
                          
let sequenceGen ms =
    let k m' m = gen { let! x = m
                       let! xs = m'
                       return (x::xs)}
    List.fold k (gen {return []}) ms
    
let replicateGenM n g = List.replicate n g |> sequenceGen

type ArbitraryModifiers = 
    static member Rectangle () =
         { new Arbitrary<Rectangle>() with
            override x.Arbitrary = liftGen4 (fun l t w h -> {Left=l;Top=t;Width=w;Height=h}) nonNeg nonNeg pos pos }
    static member AcousticEvent () =
        { new Arbitrary<AcousticEvent>()with
            override x.Arbitrary = gen { let! rect = arbitrary
                                         let r, b = right rect, bottom rect
                                         let! c = choose (1, rect.Height * rect.Width)                                    
                                         let! elms = pairGen (choose (rect.Top,b)) (choose (rect.Left,r)) |> replicateGenM c 
                                         return {Bounds=rect;Elements=Set.of_list elms}}} 
     
let chk f = 
    overwriteGenerators<ArbitraryModifiers>()
    check config f