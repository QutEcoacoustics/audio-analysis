@ECHO OFF
echo Installing QutSensors.CacheProcessor for spectrograms only...
sc create QutCacheSpectrogram binpath= "D:\SensorsCacheProcessor\QutSensors.CacheProcessor.exe" start= auto obj= ".\sensorcacheprocessor" password= qscp84^#ihascb DisplayName= QutCacheSpectrogram
sc start QutCacheSpectrogram
echo Done
pause