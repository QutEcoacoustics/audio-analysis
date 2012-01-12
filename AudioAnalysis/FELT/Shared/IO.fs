
namespace MQUTeR.FSharp.Shared
    
    open System
    open System.IO

    
    module IO =

        let readFileAsString filePath =
            let lines =
                try
                    File.ReadAllLines(filePath)
                with | ex -> 
                    eprintfn "Open file failed: %s" ex.Message
                    [| |]
            if lines.Length = 0 then
                Option.None
            else
                // potentially very bad performance
                Option.Some(lines)
            
                


