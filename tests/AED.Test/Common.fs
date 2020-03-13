module Common

open System.IO

open Xunit
open System
open System.IO
open System.IO.Compression
open System.Reflection
open Acoustics.AED.GetAcousticEvents
open Acoustics.AED.Util
open Microsoft.FSharp
open Microsoft.FSharp.Math
                       
type TestMetadata = {Dir:string; BWthresh:double; smallThreshIn:int; smallThreshOut:int}
let BAC2_20071015_045040 =
    {Dir="BAC2_20071015-045040"; BWthresh=9.0; smallThreshIn=200; smallThreshOut=130}
let GParrots_JB2_20090607_173000_wav_minute_3 =
    {Dir="GParrots_JB2_20090607-173000.wav_minute_3"; BWthresh=3.0; smallThreshIn=100; smallThreshOut=35}
                          
let testAll f = Seq.iter f [BAC2_20071015_045040; GParrots_JB2_20090607_173000_wav_minute_3]

/// Sets the current directory to be the fictures folders where test resources are kept
let matlabPath = @"..\..\..\Fixtures\FSharp\"

// when module opens, unzip asssets
do
    let unzip file =
        let path = Path.Combine(matlabPath, file)
        if Directory.Exists(path) then
            Directory.Delete(path, true)
        ZipFile.ExtractToDirectory(path + ".zip", matlabPath)
    unzip BAC2_20071015_045040.Dir
    unzip GParrots_JB2_20090607_173000_wav_minute_3.Dir

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
let rectToStringI r = sprintf "%i, %i, %i, %i" (left r) (right r) (bottom r) (top r)

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
                    sprintf "\t Expected[%i]:\t%s\tFound[%i]:\t%s" i (Seq.item i xs |> toS) i (Seq.item i ys |> toS) +
                    (if i' < Seq.length ys then sprintf "\tFound[%i]:\t%s" i' (Seq.item i' ys |> toS) else "" )
            [(Seq.map f is) |> String.concat "\r\n" |> Some ]       
    catOptions (l::c)

let assertSeqEqual eq toS xs ys =
    let m = seqEqual eq toS xs ys
    if Seq.isEmpty m then 
        Assert.True(true) 
    else
        Assert.True(false, "\r\n\r\n" + (String.concat "\r\n\r\n" m) + "\r\n" )

let selectBounds (aes:seq<AcousticEvent>) = Seq.map (fun ae -> ae.Bounds) aes
let createEvents (rs:seq<Rectangle<int, int>>) = Seq.map (fun r -> { Bounds = r; Elements = Set.empty }) rs



let parseStringAsMatrix (input:string) =
    let delimitters = [|for  c in Environment.NewLine -> c.ToString()|]
    let split = input.Split(delimitters, StringSplitOptions.RemoveEmptyEntries)

    Matrix.init (split.Length) (split.[0].Length) (fun x y -> split.[x].[y] |> string |> Double.Parse)


[<Fact>]
let ``matrix parsing test`` () = 
    let pattern = @"
1010101010101010
1010101010101010
1010101010101010
"
    
    let expected = 
        MatrixTopLevelOperators.matrix [|
            [|1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0|];
            [|1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0|];
            [|1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0; 1.0; 0.0|];
        |]

    let actual = parseStringAsMatrix pattern
    Assert.Equal<matrix>(expected, actual)

let matrixToCoordinates predicate matrix =
    let f i j s x = if predicate x then Set.add (i,j) s else s
    Matrix.foldi f Set.empty matrix

let hitsToCoordinates =
    matrixToCoordinates (fun x -> x = 1.0)

[<Fact>]
let ``matrix parsing hit to coordinates test`` () = 
    let pattern = @"
1000000010000001
1000000101000001
1000000010000001
"
    let expected = [(0,0); (0,8); (0,15); (1,0); (1,7); (1,9); (1,15); (2,0); (2,8); (2,15);] |> Set.ofList

    let actual = parseStringAsMatrix pattern |> hitsToCoordinates
    Assert.Equal<Set<_>>(expected, actual)


[<Fact>]
let ``matrix order tests for sanities`` () = 
    let pattern = @"
1470
2581
3692
"
    let mString = pattern |> parseStringAsMatrix
    let m = MatrixTopLevelOperators.matrix [ [1.0;4.0;7.0;0.0]; [2.0;5.0;8.0;1.0]; [3.0;6.0;9.0;2.0]]
    
    Assert.Equal<matrix>(mString, m)
    Assert.Equal(5.0, m.[1,1])
    Assert.Equal(9.0, m.[2,2])
    Assert.Equal(1.0, m.[1, 3]) // row, then column - y, then x
    Assert.Equal(3.0, m.[2, 0]) // row, then column - y, then x