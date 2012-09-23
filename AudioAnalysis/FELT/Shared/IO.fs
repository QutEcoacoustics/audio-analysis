
namespace MQUTeR.FSharp.Shared
    
    open System
    open System.IO
    open System.Diagnostics
    
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
            
        let resolve path dir = let test = Path.Combine(dir, path) in if File.Exists test then true,test else false,test
                
    
    [<AutoOpen>]
    module Logger =
        open log4net
        [<assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)>]
        do
            ()

        let mutable fName:string = null;
        //let tempMessages = new ResizeArray<string>()
        let log4Net = LogManager.GetLogger("FSharpLog");
        (*
        let create file =
            let lfi = new FileInfo(file)
            fName <- lfi.FullName
            File.AppendAllLines(fName, tempMessages)
            tempMessages.Clear

        let logToFile level message =
            let msg = sprintf "[%s]%s%s" (DateTime.Now.ToString("o")) level message

            if isNull fName then
                tempMessages.Add(msg)
            else
                try 
                    File.AppendAllLines(fName, [msg])
                with
                    | ex -> Console.WriteLine("ERROR BIG THREADING BOO BOO WRITING: " + msg)
            
            Console.WriteLine(msg)
            #if DEBUG
            System.Diagnostics.Debug.WriteLine(msg)
            #endif
            msg

        let Log   str = logToFile " " str|> ignore
        let Info  str = logToFile " INFO:  " str|> ignore
        let Warn  str = logToFile " WARN:  " str|> ignore
        let Error str = logToFile " ERROR: " str|> ignore
        let ErrorFail str = 
                logToFile " FATAL ERROR: " str 
                *)
        let Debug = log4Net.Debug
        let Log   = log4Net.Info
        let Info  = log4Net.Info
        let Warn  = log4Net.Warn
        let Error = log4Net.Error
        let ErrorFail = 
                log4Net.Fatal

        let Debugf   fmt = Printf.ksprintf Log fmt 
        let Logf   fmt = Printf.ksprintf Log fmt 
        let Infof  fmt = Printf.ksprintf Info fmt 
        let Warnf  fmt = Printf.ksprintf Warn fmt
        let Errorf fmt = Printf.ksprintf Error fmt 
        let ErrorFailf fmt = 
            //let fail str = logToFile " FATAL ERROR: " str |> failwith |> ignore 
            Printf.ksprintf ErrorFail  fmt 

         /// <summary>
        /// TimeSpan pretty printer
        /// </summary>
        /// <param name="ts">The TimeSpan to format</param>
        /// <returns>A formatted string</returns>
        let FormattedTime(ts:TimeSpan) =
             String.Format
              ( "{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds )

        /// Format and print elapsed time returned by Stopwatch
        let PrintTime(ts:TimeSpan) = 
            Warn(FormattedTime(ts))

        /// Executes a function and prints timing results
        let TimedAction (label:string) test =
            Warnf "Starting\t\"%s\"\ttimed run" label
            let stopWatch = Stopwatch.StartNew()

            let result = test()

            stopWatch.Stop()
            let seqT = stopWatch.Elapsed
            Warnf "Timed run\t\"%s\"\t: %s" label (FormattedTime(seqT))
            result