
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
            
                

    [<AutoOpen>]
    module Logger =
        let mutable fName = null;
        let tempMessages = new ResizeArray<string>()

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

        let Log   = logToFile " " 
        let Info  = logToFile " INFO:  " 
        let Warn  = logToFile " WARN:  " 
        let Error = logToFile " ERROR: "
            
        let Logf   fmt = Printf.ksprintf (logToFile " ") fmt
        let Infof  fmt = Printf.ksprintf (logToFile " INFO:  ") fmt
        let Warnf  fmt = Printf.ksprintf (logToFile " WARN:  ") fmt
        let Errorf fmt = Printf.ksprintf (logToFile " ERROR: ") fmt

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