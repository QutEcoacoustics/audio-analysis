@ECHO OFF
echo Installing QutSensors.CacheProcessor for audio only...
sc create QutCacheAudio binpath= "D:\SensorsCacheProcessor\QutSensors.CacheProcessor.exe" start= auto obj= ".\sensorcacheprocessor" password= qscp84^#ihascb DisplayName= QutCacheAudio
sc start QutCacheAudio
echo Done
pause