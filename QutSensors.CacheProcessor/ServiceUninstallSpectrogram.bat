@ECHO OFF
echo Removing QutSensors.CacheProcessor for spectrograms only...
sc stop QutCacheSpectrogram
sc delete QutCacheSpectrogram
echo Done
pause