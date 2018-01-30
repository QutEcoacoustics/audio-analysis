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
                | True data -> Assert.True(true)
                | _ -> Assert.True(false, testFinishedToString name result) 
        } 
        
let config = {quick with Runner = xUnitRunner}