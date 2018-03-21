# Log files

Log files record all the messages that you normally see in your terminal into a
text file. Because these messages are saved to a text file we can:

# Important details

- Every time _AP.exe_ runs a log file is saved in the `Logs` folder
- The `Logs` folder is in the same directory as the `AnalysisPrograms.exe`
  program
- The latest log will always be named `log.txt`
- The latest 20 logs will be kept, after which they are deleted
- We recommend you save every log from every analysis you run
  - This will soon be easier once [#157](https://github.com/QutEcoacoustics/audio-analysis/issues/157)
    is complete.

# Detailed explanation

Because _AP.exe_ saves all the messages to a text file we can:

- look at the messages anytime to sort out problems
- send the log files to other people so they can sort out problems
- refer to these logs to determine the provenance of results

The last point is really important. To support reproducible science we need to
know what program, which configuration, and what data was used to generate the
results.

Our log files record this information. The start of every log file has the
following information:

- The date the program was started
- The version of _AP.exe_
- The type of build (either `DEBUG` or `RELEASE`)
- The date the program was compiled
- The unique id of the source code provided by Git (commonly known as the hash)
- The branch that was used to get the source code (usually this is `master`)
- The arguments the program received (which detail which input file and what 
  config file were used)

Here is an example extract that shows all of the above information:

```
2018-03-21T13:44:45.8675459+10:00 [1] INFO  CleanLogger - QUT Ecoacoustics Analysis Programs - version 18.03.3.5 (DEBUG build, 2018-03-19 12:23) 
Git branch-version: master-517b65bca92f1ed6ce3ea207a5660ff473222424-CI:123
Copyright QUT 2018
2018-03-21T13:44:45.9035316+10:00 [1] INFO  LogFileOnly - Executable called with these arguments: 
"C:\Work\GitHub\audio-analysis\src\AnalysisPrograms\bin\Debug\AnalysisPrograms.exe" -l 7
```