function split_2hr_audio_file
% Splits two hour audio file into 1 minute recordings
% 
% Recordings saved in 
% G:\Birgit\Sensor\image_proc\Jason 2hr recording
%
% Sensor Networks Project
% Birgit Planitz
% 20090415


% Parameters
bigfs = 60*22050; % length of a recording is 60sec; this equals 60*22050 samples (22050Hz is sampling frequency)
numrec = 120; % total number of 1min recordings

txt = '20090318-050102';


for nn = 1:numrec
    
    % read audio data
    N1 = (nn-1)*bigfs + 1;
    N2 = N1 + bigfs - 1;

    cd 'G:\Birgit\Sensor\image_proc\Jason 2hr recording'
    [y, fs, nbits, opts] = wavread(strcat(txt,'.wav'),[N1 N2]);
    
    this_file = strcat('recording_minute_',num2str(nn),'.wav');
    wavwrite(y,fs,this_file)
    cd 'G:\Birgit\Sensor\image_proc\Acoustic Analysis - Brandes - 20090415'


%     figure(1), plot(y)
%     pause
    
    
end
