@ECHO OFF
echo Installing QutSensors.CacheProcessor...
C:\Windows\Microsoft.NET\Framework\v2.0.50727\installutil.exe QutSensors.CacheProcessor.exe
net start QutSensorsCacheJobProcessor
echo Done
pause
rem use 'nt authority\localservice' with no password to install