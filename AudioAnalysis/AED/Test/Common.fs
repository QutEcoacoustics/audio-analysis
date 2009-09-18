module Common

open FsCheck
open FsCheckXunit
open QutSensors.AudioAnalysis.AED.GetAcousticEvents
open QutSensors.AudioAnalysis.AED.Util
    
let loadTestFile2 f i j = fileToMatrix (@"C:\Documents and Settings\Brad\svn\Sensors\trunk\AudioAnalysis\AED\Test\matlab\" + f) i j |> Math.Matrix.of_array2

let loadTestFile f = loadTestFile2 f 256 5188

let loadEventsFile f j =
    // matlab matrix indicies are 1 based, F# is 0 based
    let aem = loadTestFile2 f 10 j
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
     
(*
type NonNegativeInt = NonNegative of int

type ArbitraryModifiers =
    static member NonNegativeInt () =
        { new Arbitrary<NonNegativeInt>() with
            override x.Arbitrary = arbitrary |> fmapGen (NonNegative << abs)
            override x.CoArbitrary (NonNegative i) = coarbitrary i
            override x.Shrink (NonNegative i) = shrink i |> Seq.filter ((<) 0) |> Seq.map NonNegative }
*)

// TODO how to type annotate this version: let nonNeg = arbitrary |> fmapGen abs
let nonNeg = gen { let! (x:int) = arbitrary
                   return (abs x) }

let pos = nonNeg |> fmapGen ((+) 1)

let pairGen g1 g2 = gen { let! x = g1
                          let! y = g2
                          return (x,y) }
                          
let sequenceGen ms =
    let k m' m = gen { let! x = m
                       let! xs = m'
                       return (x::xs)}
    List.fold k (gen {return []}) ms
    
let replicateGenM n g = List.replicate n g |> sequenceGen

// TODO liftGen4
type ArbitraryModifiers = 
    static member Rectangle () =
         { new Arbitrary<Rectangle>() with
            override x.Arbitrary = gen { let! top = nonNeg 
                                         let! left = nonNeg
                                         let! height = pos
                                         let! width = pos
                                         return {Left=left;Top=top;Width=width;Height=height} }}
    static member AcousticEvent () =
        { new Arbitrary<AcousticEvent>()with
            override x.Arbitrary = gen { let! rect = arbitrary
                                         let r = right rect
                                         let b = bottom rect
                                         let! c = choose (1, rect.Height * rect.Width)
                                         // elms <- replicateM c $ pairM (choose (t,b)) (choose (l,r))                                         
                                         let! elms = pairGen (choose (rect.Top,b)) (choose (rect.Left,r)) |> replicateGenM c 
                                         // return $ AE rect $ Set.fromList elms
                                         return {Bounds=rect;Elements=Set.of_list elms}}} 
     
let chk f = 
    overwriteGenerators<ArbitraryModifiers>()
    check config f