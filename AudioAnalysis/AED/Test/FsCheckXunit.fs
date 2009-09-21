module FsCheckXunit

open FsCheck
open FsCheck.Runner 
open Xunit 
   
let xUnitRunner = 
    { new IRunner with 
        member x.OnArguments(_,_,_) = ()  
        member x.OnShrink(_,_) = ()
        member x.OnFinished(name, result) = 
            match result with 
                | True data ->
                    Assert.True(true) 
                    data.Stamps |> Seq.iter (fun x -> printfn "%d - %A" (fst x) (snd x)) 
                | False (data,_,args,Exception e,_) ->
                    Assert.True(false, sprintf "%s - Falsifiable after %i tests (%i shrinks): %A with exception %O"
                        name data.NumberOfTests data.NumberOfShrinks args e) 
                | False (data,_,args,Timeout i,_) ->
                    Assert.True(false, sprintf "%s - Timeout of %i milliseconds exceeded after %i tests (%i shrinks): %A"
                        name i data.NumberOfTests data.NumberOfShrinks args) 
                | False (data,_,args,_,_) ->
                    Assert.True(false, sprintf "%s - Falsifiable after %i tests (%i shrinks): %A"
                        name data.NumberOfTests data.NumberOfShrinks args) 
                | Exhausted data ->  Assert.True(false, sprintf "Exhausted after %d tests" (data.NumberOfTests) ) 
        } 
let config = {quick with Runner = xUnitRunner}