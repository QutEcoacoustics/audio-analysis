module FsCheckXunit

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
                | False (_,args,_,_,_) ->  Assert.True(false, sprintf "%s - Falsifiable: %A" name args) 
                //| False (_,args,None,_,_) ->  Assert.True(false, sprintf "%s - Falsifiable: %A" name args) 
                //| False (_,args,Some exc) -> Assert.True(false, sprintf "%s - Falsifiable: %A with exception %O" name args exc) 
                | Exhausted data ->  Assert.True(false, sprintf "Exhausted after %d tests" (data.NumberOfTests) ) 
        } 
let config = {quick with Runner = xUnitRunner}