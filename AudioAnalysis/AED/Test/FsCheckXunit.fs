module FsCheckXunit

open System 
open FsCheck.Runner 
open Xunit 
   
// TODO property tied to a single test i.e. test name (not property name) given on failure
let xUnitRunner = 
    { new IRunner with 
        member x.OnArguments(ntest,args,every) = ()
          //args |>  
          //List.iter(function 
          //  | :? IDisposable as d -> d.Dispose()  
          //  | _ -> ())    
        member x.OnShrink(args, everyShrink) = ()
        member x.OnFinished(name, result) = 
            match result with 
                | True data ->
                    Assert.True(true) 
                    data.Stamps |> Seq.iter (fun x -> printfn "%d - %A" (fst x) (snd x)) 
                | False (_,args,_,_,_) ->  Assert.True(false, sprintf "%s - Falsifiable: %A" name args) 
                //| False (_,args,None,_,_) ->  Assert.True(false, sprintf "%s - Falsifiable: %A" name args) 
                //| False (_,args,Some exc) -> Assert.True(false, sprintf "%s - Falsifiable: %A with exception %O" name args exc) 
                | Exhausted data ->  Assert.True(false, sprintf "Exhausted after %d tests" (data.NumberOfTests) ) 
        } 
let config = {quick with Runner = xUnitRunner}