function show_AE_2hr_recording
% Show acoustic events detected on minute segments of 2hr recordings
% 
% Test applied to 2hr recording in 
% G:\Birgit\Sensor\image_proc\Jason 2hr recording
%
% Sensor Networks Project
% Birgit Planitz
% 20090415


warning off 


numrec = 120; % total number of 1min recordings


for nn = 1:numrec
    
    cd 'G:\Birgit\Sensor\image_proc\Jason 2hr recording'
    txt = strcat('recording_minute_',num2str(nn));
    
    % read audio data
    [y, fs, nbits, opts] = wavread(strcat(txt,'.wav'));
    
    % load acoustic event locations
%     load(strcat('int_thresh_5dB_',txt,'.mat'),'AE3')
    load(strcat('int_thresh_7dB_',txt,'.mat'),'AE3')
    cd 'G:\Birgit\Sensor\image_proc\Acoustic Analysis - Brandes - 20090415'
    
    if ~isempty(AE3)

        disp(strcat('Acoustic events found at minute ',num2str(nn)))
        
%         % show audio file
%         figure(1), clf, plot(y), title('Audio file')

        % show original image
        tmax = length(y)/fs; %length of signal in seconds
        fmax = 11025;
        window = 512; % hamming window using 512 samples
        noverlap = round(0.5*window); % 50% overlap between frames
        nfft = 256*2-1; % yield 512 frequency bins
        [S,F,T,P] = spectrogram(y,window,noverlap,nfft,fs);
        figure(2), clf, imagesc(T,F,10*log10(abs(P)));
        axis xy; axis tight; colormap(gray); view(0,90);
        set(gca,'XTick',[0:10:tmax],'FontSize',20)
        set(gca,'YTick',[0:2000:fmax],'FontSize',20)
        title('Original Image','FontSize',20)
        ylabel('Frequency (kHz)','FontSize',20)
        xlabel('Time (s)','FontSize',20)
        colorbar
        % maximise this image on your screen for optimal viewing

        length(y)/512

        % convert amplitude to dB
        I1 = 10*log10(abs(P));

        % show acoustic events
        [M,N] = size(I1)
        T = linspace(0,tmax,N);
        F = linspace(0,fmax,M);


        % alternatively uncomment code below to view image and show time and frequency axis
        figure(3), clf, imagesc(T,F,I1);
        axis xy; axis tight; colormap(gray); view(0,90);
        xlabel('Time (s)');
        ylabel('Frequency (Hz)');
        colorbar
        figure(3), hold on, line([T(AE3(1,:)); T(AE3(1,:))+T(AE3(3,:))], [F(AE3(2,:)); F(AE3(2,:))],'Color','b')
        figure(3), hold on, line([T(AE3(1,:)); T(AE3(1,:))+T(AE3(3,:))], [F(AE3(2,:))+F(AE3(4,:)); F(AE3(2,:))+F(AE3(4,:))],'Color','b')
        figure(3), hold on, line([T(AE3(1,:)); T(AE3(1,:))], [F(AE3(2,:)); F(AE3(2,:))+F(AE3(4,:))],'Color','b')
        figure(3), hold on, line([T(AE3(1,:))+T(AE3(3,:)); T(AE3(1,:))+T(AE3(3,:))], [F(AE3(2,:)); F(AE3(2,:))+F(AE3(4,:))],'Color','b')
        set(gca,'XTick',[0:10:tmax],'FontSize',20)
        set(gca,'YTick',[0:2000:fmax],'FontSize',20)
        title('Original Image with Marqueed Acoustic Events','FontSize',20)
        ylabel('Frequency (kHz)','FontSize',20)
        xlabel('Time (s)','FontSize',20)
        % maximise this image on your screen for optimal viewing
        pause%(0.1)

    end
end
    