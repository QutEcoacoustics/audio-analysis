@ECHO OFF
echo Removing QutSensors.CacheProcessor...
net stop QutSensorsCacheJobProcessor
C:\Windows\Microsoft.NET\Framework\v2.0.50727\installutil.exe /uninstall QutSensors.CacheProcessor.exe
echo Done
pause