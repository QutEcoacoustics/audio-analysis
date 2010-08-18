@ECHO OFF
echo Installing QutSensors.CacheProcessor...
sc create QutCacheProcessor binpath= "D:\SensorsCacheProcessor\QutSensors.CacheProcessor.exe" start= auto obj= ".\sensorcacheprocessor" password= qscp84^#ihascb DisplayName= QutCacheProcessor
sc start QutCacheProcessor
echo Done
pause