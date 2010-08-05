@ECHO OFF
echo Removing QutSensors Analysis Processor...
sc stop QutSensorsAnalysisLive
sc delete QutSensorsAnalysisLive
echo Done
pause