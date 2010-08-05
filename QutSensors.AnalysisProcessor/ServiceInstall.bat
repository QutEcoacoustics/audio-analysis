@ECHO OFF
echo Installing QutSensors Analysis Processor...
sc create QutSensorsAnalysisLive binpath= "D:\Projects\Sensors\AnlaysisRoot\Live\Service\QutSensors.AnalysisProcessor.exe" start= auto obj= "nt authority\LocalService" DisplayName= QutSensorsAnalysisLive
sc start QutSensorsAnalysisLive
echo Done
pause
rem use 'nt authority\localservice' with no password to install