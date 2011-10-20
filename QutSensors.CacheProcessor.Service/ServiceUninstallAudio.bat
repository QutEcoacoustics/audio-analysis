@ECHO OFF
echo Removing QutSensors.CacheProcessor for audio only...
sc stop QutCacheAudio
sc delete QutCacheAudio
echo Done
pause