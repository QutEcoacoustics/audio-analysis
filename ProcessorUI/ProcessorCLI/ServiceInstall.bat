@ECHO OFF
echo Installing QutSensors Analysis Processor...
C:\Windows\Microsoft.NET\Framework\v2.0.50727\installutil.exe ProcessorCLI.exe
net start QUTSensorsAnalysisJobProcesor
echo Done
pause
rem use 'nt authority\localservice' with no password to install