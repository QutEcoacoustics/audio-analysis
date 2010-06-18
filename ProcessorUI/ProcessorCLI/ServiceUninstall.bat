@ECHO OFF
echo Removing QutSensors Analysis Processor...
net stop QUTSensorsAnalysisJobProcesor
C:\Windows\Microsoft.NET\Framework\v2.0.50727\installutil.exe /uninstall ProcessorCLI.exe
echo Done
pause