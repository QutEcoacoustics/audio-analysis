@ECHO OFF
echo Removing QutSensors.CacheProcessor...
sc stop QutCacheProcessor
sc delete QutCacheProcessor
echo Done
pause