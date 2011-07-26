 function Time_domain_detection
clc;
clear;
% colormap for plotting in grayscale
ctmp = colormap(gray); 
c = flipud(ctmp);
%  window=512;% hamming window using 512 samples
window=256;

% window=128;
% window=1024;
%noverlap = round(0.5*window); % 50% overlap between frames

%  pathtofile='C:\SensorNetworks\WavFiles\BAC\BAC1_20071008-084607.wav';
%  pathtofile='C:\SensorNetworks\WavFiles\3hours samford tagged dataset\single channel\2 mins\NEJB_NE465_20101013-043000\NEJB_NE465_20101013-044200.wav';
 pathtofile='C:\Documents and Settings\n7594879\My Documents\sandra\Project Codes\Matlab\dataset\training data\BAC2_20071015-045040.wav';
% pathtofile='C:\SensorNetworks\WavFiles\download\SERF 3_20100913-083000.wav';
%  pathtofile='C:\SensorNetworks\WavFiles\Koala_Female\HoneymoonBay_StBees_20081122-203000.wav';
% pathtofile='C:\SensorNetworks\WavFiles\whipbird\file0151mono.wav_segment_14.wav';

[y, fs] = wavread(pathtofile);
[yy,fs,I1,F,T] = wavToSpectrogram(pathtofile);
[M,N]=size(I1);
% showImage(c,I1,T,F,1);

L=length(y); S=y;

%figure(1),plot(y);axis xy; axis tight; view(0,90);title('Original data','Fontsize',20);
%ylabel('Amplitude','Fontsize',20);
%xlabel('Time(s)','Fontsize',20);

%Add chebyshev type1 filter
% [B,A]=cheby1(9,3,0.2721);
% h1=dfilt.df2(B,A);
% y2 = filter(h1, y);S=y2;
% [y3,fs,I10,F1,T1]=smoothSpectrogram(S,fs);
% showImage(c,I10,T1,F1,5);

%Add chebyshev type 1 filter(low pass)
% [B,A]=cheby1(9,0.1,0.4535);
% h1=dfilt.df2(B,A);
% y2 = filter(h1, y);S=y2;
% [y3,fs,I10,F1,T1]=smoothSpectrogram(S,fs);
% showImage(c,I10,T1,F1,5);

%Add chebyshev type1 band pass filter(200-3500Hz)
[h1,S1]=ChebyshevFilter(10,0.1,3000,5500,fs/2,y);

[y3,fs,I10,F1,T1]=smoothSpectrogram(S1,fs);
% showImage(c,I10,T1,F1,5);

%Add chebyshev type1 band pass filter(2000-4000Hz)
[h2,S2]=ChebyshevFilter(10,0.1,4500,7500,fs/2,y);
[y4,fs,I11,F1,T1]=smoothSpectrogram(S2,fs);
% showImage(c,I11,T1,F1,6);

%Add chebyshev type1 band pass filter(2000-4000Hz)
[h3,S3]=ChebyshevFilter(10,0.1,6000,9500,fs/2,y);

[y4,fs,I12,F1,T1]=smoothSpectrogram(S3,fs);
% showImage(c,I12,T1,F1,7);

% 
% [WhistleFrequencyStd1, WhistleFrameStd1,WhistleFrequencydB1, WhistleFramedB1,IstdH1,IstdV1,IdBH1,IdBV1]=SignalMarkedInSpectrogram(window,S1,L,fs,I1,F);
% [WhistleFrequencyStd2, WhistleFrameStd2,WhistleFrequencydB2, WhistleFramedB2,IstdH2,IstdV2,IdBH2,IdBV2]=SignalMarkedInSpectrogram(window,S2,L,fs,I1,F);
% [WhistleFrequencyStd3, WhistleFrameStd3,WhistleFrequencydB3, WhistleFramedB3,IstdH3,IstdV3,IdBH3,IdBV3]=SignalMarkedInSpectrogram(window,S3,L,fs,I1,F);
[freqBin1,FrameZero1,freqBindB1,FrameZerodB1,WhipFrequencydB1,WhipFramedB1,BlockFrequencydB1,BlockFramedB1]=SignalMarkedInSpectrogram(window,S1,L,fs,I1,F);
[freqBin2,FrameZero2,freqBindB2,FrameZerodB2,WhipFrequencydB2,WhipFramedB2,BlockFrequencydB2,BlockFramedB2]=SignalMarkedInSpectrogram(window,S2,L,fs,I1,F);
[freqBin3,FrameZero3,freqBindB3,FrameZerodB3,WhipFrequencydB3,WhipFramedB3,BlockFrequencydB3,BlockFramedB3]=SignalMarkedInSpectrogram(window,S3,L,fs,I1,F);
% [WhipFrequencyStd1, WhipFrameStd1,WhipFrequencydB1,WhipFramedB1]=SignalMarkedInSpectrogram(window,S1,L,fs,I1,F);
% [WhipFrequencyStd2, WhipFrameStd2,WhipFrequencydB2,WhipFramedB2]=SignalMarkedInSpectrogram(window,S2,L,fs,I1,F);
% [WhipFrequencyStd3, WhipFrameStd3,WhipFrequencydB3,WhipFramedB3]=SignalMarkedInSpectrogram(window,S3,L,fs,I1,F);

% [BlockFrequencyStd1,BlockFrameStd1,BlockFrequencydB1,BlockFramedB1]=SignalMarkedInSpectrogram(window,S1,L,fs,I1,F);
% [BlockFrequencyStd2,BlockFrameStd2,BlockFrequencydB2,BlockFramedB2]=SignalMarkedInSpectrogram(window,S2,L,fs,I1,F);
% [BlockFrequencyStd3,BlockFrameStd3,BlockFrequencydB3,BlockFramedB3]=SignalMarkedInSpectrogram(window,S3,L,fs,I1,F);

%combine all dots in three band pass filters and cluster them
% [StartPoint,EndPoint,AcousticFrequency,AcousticFrame,FreqBins,OutStart1,OutEnd1,OutStart2,OutEnd2,SFrq,EFrq]=WhistleClustering(WhistleFrequencyStd1, WhistleFrameStd1,WhistleFrequencyStd2, WhistleFrameStd2,WhistleFrequencyStd3, WhistleFrameStd3,fs,window);
[StartPoint,EndPoint,AcousticFrequency,AcousticFrame,FreqBins,OutStart1,OutEnd1,OutStart2,OutEnd2,SFrq,EFrq]=WhistleClustering(freqBin1,FrameZero1,freqBin2,FrameZero2,freqBin3,FrameZero3,fs,window);
[AcousSigWhip,frequencyWhip,timeframeWhip,OutStartHW,OutEndHW,OutStartLW,OutEndLW,HFrqW,LFrqW]=WhipClustering(WhipFrequencydB1,WhipFramedB1,WhipFrequencydB2,WhipFramedB2,WhipFrequencydB3,WhipFramedB3,fs,window);
% [AcousSigWhip,frequencyWhip,timeframeWhip,OutStartHW,OutEndHW,OutStartLW,OutEndLW,HFrqW,LFrqW]=WhipClustering(freqBindB1,FrameZerodB1,freqBindB2,FrameZerodB2,freqBindB3,FrameZerodB3,fs,window);
% [AcousSig,frequency,timeframe,OutStartH,OutEndH,OutStartL,OutEndL,HFrq,LFrq]=BlockClustering(BlockFrequencydB1,BlockFramedB1,BlockFrequencydB2,BlockFramedB2,BlockFrequencydB3,BlockFramedB3,fs,window);
[AcousSig,frequency,timeframe,OutStartH,OutEndH,OutStartL,OutEndL,HFrq,LFrq]=BlockClustering(freqBindB1,FrameZerodB1,freqBindB2,FrameZerodB2,freqBindB3,FrameZerodB3,fs,window);
% showImage2(c,I1,AcousSig,frequency,timeframe,OutStartH,OutEndH,OutStartL,OutEndL,HFrq,LFrq,T,F,18,1);

% showImage2(c,I1,AcousSigWhip,frequencyWhip,timeframeWhip,OutStartHW,OutEndHW,OutStartLW,OutEndLW,HFrqW,LFrqW,T,F,18,1);
% showImage1(c,I1,AcousticFrequency,AcousticFrame,StartPoint,EndPoint,FreqBins,OutStart1,OutEnd1,OutStart2,OutEnd2,SFrq,EFrq,T,F,19,1);
showComponents(c,I1,AcousticFrequency,AcousticFrame,StartPoint,EndPoint,FreqBins,OutStart1,OutEnd1,OutStart2,OutEnd2,SFrq,EFrq,AcousSig,frequency,timeframe,OutStartH,OutEndH,OutStartL,OutEndL,HFrq,LFrq,AcousSigWhip,frequencyWhip,timeframeWhip,OutStartHW,OutEndHW,OutStartLW,OutEndLW,HFrqW,LFrqW,T,F,20,1);

%



% % showImage1(c,I1,IstdH1,IstdV1,IstdH2,IstdV2,T,F,9,1);
% showImage1(c,I1,IstdH1,IstdV1,IstdH2,IstdV2,IstdH3,IstdV3,T,F,9,1);
% showImage1(c,I1,WhistleFrequencyStd1, WhistleFrameStd1,WhistleFrequencyStd2, WhistleFrameStd2,WhistleFrequencyStd3, WhistleFrameStd3,T,F,12,1);
% showImage1(c,I1,WhipFrequencyStd1,WhipFrameStd1,WhipFrequencyStd2,WhipFrameStd2,WhipFrequencyStd3,WhipFrameStd3,T,F,14,1);
% showImage1(c,I1,BlockFrequencyStd1,BlockFrameStd1,BlockFrequencyStd2,BlockFrameStd2,BlockFrequencyStd3,BlockFrameStd3,T,F,16,1);
% showImage1(c,I1,IdBH1,IdBV1,IdBH2,IdBV2,IdBH3,IdBV3,T,F,17,2);
% showImage1(c,I1,WhistleFrequencydB1, WhistleFramedB1,WhistleFrequencydB2, WhistleFramedB2,WhistleFrequencydB3, WhistleFramedB3,T,F,13,2);
% showImage1(c,I1,freqBindB1,FrameZerodB1,freqBindB2,FrameZerodB2,freqBindB3,FrameZerodB3,T,F,15,2);
% showImage1(c,I1,freqBindB1,FrameZerodB1,freqBindB2,FrameZerodB2,freqBindB3,FrameZerodB3,T,F,15,2);


fprintf('finished!\n');

end




    
    




