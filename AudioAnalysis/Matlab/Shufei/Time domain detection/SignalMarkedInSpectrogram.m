%  function [freqBin,FrameZero,freqBindB,FrameZerodB]=SignalMarkedInSpectrogram(window,S,L,fs,I1,F)
% function [WhistleFrequencyStd, WhistleFrameStd,WhistleFrequencydB, WhistleFramedB,IstdH,IstdV,IdBH,IdBV]=SignalMarkedInSpectrogram(window,S,L,fs,I1,F)
function [freqBin,FrameZero,freqBindB,FrameZerodB,WhipFrequencydB, WhipFramedB]=SignalMarkedInSpectrogram(window,S,L,fs,I1,F)
% function [BlockFrequencyStd,BlockFrameStd,BlockFrequencydB,BlockFramedB]=SignalMarkedInSpectrogram(window,S,L,fs,I1,F)
[dB,ZeroCrossing,Std,fz,X]=ExtractfeaturesInTimeDomain(window,S,L,fs);

%  figure (14);
%  plot(fz,dB,'.');
%  title('dB vs frequency','Fontsize',20);
%  ylabel('dB','Fontsize',20);
%  xlabel('frequency (Hz)','Fontsize',20);

%  figure (14);
%  plot(fz,Std,'.');
%  title('Std vs frequency','Fontsize',20);
%  ylabel('Std','Fontsize',20);
%  xlabel('frequency (Hz)','Fontsize',20);
%calculate the residuals of Std
 [resi,cur]=residuals(fz,Std);

% figure (14);
% plot(fz,resi,'.');
% title('Residuals vs frequency','Fontsize',20);
% ylabel('Residuals','Fontsize',20);
% xlabel('frequency (Hz)','Fontsize',20);
%hold on;
%  csvwrite('resi.csv',resi');
% 
%calculate the baseline of residuals
 curve1=fit(fz',resi','Poly1');

% plot(fz,resi,'.',fz,curve1(fz));  
% figure(4);plot(fz,resi, '.');

%calculate the New std after noise removal upon the algorithm proposed in
%paper of Lamel et al. 1981
[Nstd,oneStd,Nosle1]=noiseremoval(resi,X,1);

% figure(4);
%  plot(fz,Nstd,'.');
%  title('Residuals values after noise removal','fontsize',15);
% ylabel('Residuals','fontsize',15);
% xlabel('frequency (Hz)','fontsize',15);
%calculate the New dB af ter noise removal 
[NdB,NoneStd,Nosle2]=noiseremoval(dB,X,2);

% figure(13);
% plot(fz,NdB,'.');
% title('dB values after noise removal','fontsize',15);
% ylabel('dB','fontsize',15);
% xlabel('frequency (Hz)','fontsize',15);
%figure(6);plot(fz,noise,'.');
% csvwrite('Nstd.csv',Nstd');
% csvwrite('NdB.csv',NdB');

%set thresholds to extract the signals according to normal distribution 
sig=signalExtract(Nstd,oneStd,X,Nosle1,1);

% csvwrite('sig.csv',sig');
% figure(7);
% plot(fz,sig,'.');
% title('Residuals values based on 2.58*oneStd','fontsize',15);
% ylabel('Residuals','fontsize',15);
% xlabel('frequency (Hz)','fontsize',15);
%set thresholds to extract the signals according to normal distribution 
sigdB=signalExtract(NdB,NoneStd,X,Nosle2,2);

% csvwrite('sigdB.csv',sigdB');
% figure(15);
%  plot(fz,sigdB,'.');
%  title('dB values based on 2.58*oneStd','fontsize',15);
% ylabel('dB','fontsize',15);
% xlabel('frequency (Hz)','fontsize',15);
%calculate the frame number & time duration of the signal
%calculate the fz corresponding to sig.

[FrameN,SigFz,freqBin,FrameZero]=framenumber(sig,fz,F,X);

% csvwrite('FrameN.csv',FrameN');
% csvwrite('SigFz.csv',SigFz');
%  figure(8);
% plot(FrameN,fz,'.');
% hold on;
% plot(FrameN,SigFz,'x');

%Add the detection algorithm of whistle.
%  [WhistleFrequencyStd, WhistleFrameStd]=WhistleLocation(freqBin,FrameZero, fs,window);
%Add the detection algorithm of whip.
%  [WhipFrequencyStd,WhipFrameStd]=WhipDetection(freqBin,FrameZero, fs,window);
%Add the detection algorithm of whip.
% [BlockFrequencyStd,BlockFrameStd]=Block_detection(freqBin,FrameZero,fs,window);

[FrameNdB,SigDB,freqBindB,FrameZerodB]=framenumber(sigdB,fz,F,X);

% figure(16);
% plot(FrameNdB,SigDB,'.');
%Add the detection algorithm of whistle.
%  [WhistleFrequencydB, WhistleFramedB]=WhistleLocation(freqBindB,FrameZerodB, fs,window);
 %Add the detection algorithm of whip.
[WhipFrequencydB, WhipFramedB]=WhipDetection(freqBindB,FrameZerodB, fs,window);
%Add the detection algorithm of whip.
%  [BlockFrequencydB,BlockFramedB]=Block_detection(freqBindB,FrameZerodB,fs,window);
 
%mark all points in the spectrogram
%change I1[M*N] according to the [FrameN, SigFz]
% [IstdH,IstdV]=MakerInSpectrogram(I1,F,FrameN,SigFz,sig,oneStd,X);

%Add the detection algorithm of whistle.
% [WhistleFrequencyStd, WhistleFrameStd]=WhistleLocation(IstdH, IstdV,fs,window);
%Add the detection algorithm of whip
% [WhipFrequencyStd,WhipFrameStd]=WhipDetection(IstdH,IstdV,fs,window);
%Add the detection algorithm of Block
% [BlockFrequencyStd,BlockFrameStd]=Block_detection(IstdH,IstdV,fs,window);

% [IdBH,IdBV]=MakerInSpectrogram(I1,F,FrameNdB,SigDB,sigdB,NoneStd,X);
%Add the detection algorithm of whistle.
% [WhistleFrequencydB, WhistleFramedB]=WhistleLocation(IdBH,IdBV,fs,window);
%Add the detection algorithm of whip
% [WhipFrequencydB,WhipFramedB]=WhipDetection(IdBH,IdBV,fs,window);
%Add the detection algorithm of Block
% [BlockFrequencydB,BlockFramedB]=Block_detection(IdBH,IdBV,fs,window);


end