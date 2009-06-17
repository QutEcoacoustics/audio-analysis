function do_acoustic_event_detection
% This code starts with a raw audio file; applies image processing
% and returns areas (four points of a rectangle) that are designated as
% acoustic events
%
% Sensor Networks Project
% Birgit Planitz
% 20090415


warning off % this line is included to suppress the warning that MATLAB 
            % flashes everytime it displays a sonogram that's too large to 
            % fit image parameters




% txt = 'Honeymoon Bay - St Bees_20081120-183000'; % test file
% txt = '20090317-143000[1]';
% txt = '20090319-070105[1]';
% txt = '20090320-070105[1]';
% txt = '20090317-173000[1]'; 
% txt = '20090319-023000[1]_Nothing';
% txt = '20090319-040000[1]_bird';
% txt = 'BAC2_20071011-045040';
txt = 'BAC2_20071015-045040';
% txt = 'BAC2_20071005-132040';
% txt = 'BAC8_20080612-040000';



% read audio data
[y, fs, nbits, opts] = wavread(strcat(txt,'.wav'));
% figure(1), plot(y)
tmax = length(y)/fs; %length of signal in seconds
fmax = 11025;


% STEP 1: GENERATE SPECTROGRAM
window = 512; % hamming window using 512 samples
noverlap = round(0.5*window); % 50% overlap between frames
nfft = 256*2-1; % yield 512 frequency bins
[S,F,T,P] = spectrogram(y,window,noverlap,nfft,fs);
figure(1), clf, imagesc(T,F,10*log10(abs(P)));
axis xy; axis tight; colormap(gray); view(0,90);
colorbar
set(gca,'XTick',[0:10:tmax],'FontSize',20)
set(gca,'YTick',[0:2000:fmax],'FontSize',20)
title('Original Image','FontSize',20)
ylabel('Frequency (kHz)','FontSize',20)
xlabel('Time (s)','FontSize',20)
% maximise this image on your screen for optimal viewing





% convert amplitude to dB
I1 = 10*log10(abs(P));
% figure(10), clf, imagesc(T,F,I1);
% axis xy; axis tight; colormap(gray); view(0,90);
% xlabel('Time (s)');
% ylabel('Frequency (Hz)');
% colorbar
% set(gca,'XTick',[0:10:tmax],'FontSize',20)
% set(gca,'YTick',[0:2000:fmax],'FontSize',20)
% title('Original Image with Marqueed Acoustic Events','FontSize',20)
% ylabel('Frequency (kHz)','FontSize',20)
% xlabel('Time (s)','FontSize',20)
% % maximise this image on your screen for optimal viewing


% STEP 2: WIENER FILTERING
% NOTES: wiener2.m is a MATLAB function
w = 5; %window length of wxw window used in wiener filtering
I2 = wiener2(I1, [w w]);
% figure(2), clf, imagesc(T,F,I2);
% axis xy; axis tight; colormap(gray); view(0,90);
% xlabel('Time (s)');
% ylabel('Frequency (Hz)');
% colorbar
% set(gca,'XTick',[0:10:tmax],'FontSize',20)
% set(gca,'YTick',[0:2000:fmax],'FontSize',20)
% title('Wiener Filtered Image','FontSize',20)
% ylabel('Frequency (kHz)','FontSize',20)
% xlabel('Time (s)','FontSize',20)
% maximise this image on your screen for optimal viewing



% STEP 3: GET MODAL INTENSITIES OF SUBBBANDS and REMOVE FROM IMAGE
% NOTES: subband_mode_intensities.m is my function
I3 = subband_mode_intensities(I2);
% figure(3), clf, imagesc(T,F,I3);
% axis xy; axis tight; colormap(gray); view(0,90);
% xlabel('Time (s)');
% ylabel('Frequency (Hz)');
% colorbar
% set(gca,'XTick',[0:10:tmax],'FontSize',20)
% set(gca,'YTick',[0:2000:fmax],'FontSize',20)
% title('Image with Modal Noise Removed from Each Frequency Sub-band','FontSize',20)
% ylabel('Frequency (kHz)','FontSize',20)
% xlabel('Time (s)','FontSize',20)
% maximise this image on your screen for optimal viewing



I3tmp = I3;
I3tmp(I3<0) = 0;
% figure(31), clf, imagesc(T,F,I3tmp);
% axis xy; axis tight; colormap(gray); view(0,90);
% xlabel('Time (s)');
% ylabel('Frequency (Hz)');
% colorbar

% STEP 5: CONVERTS IMAGE TO BLACK AND WHITE USING INTENSITY THRESHOLD
int_thresh = 9;
I4 = image_thresh_bw(I3,int_thresh);
% figure(4), clf, imagesc(T,F,I4);
% axis xy; axis tight; colormap(gray); view(0,90);
% xlabel('Time (s)');
% ylabel('Frequency (Hz)');
% colorbar
% set(gca,'XTick',[0:10:tmax],'FontSize',20)
% set(gca,'YTick',[0:2000:fmax],'FontSize',20)
% title('Signal (white) versus Background (black)','FontSize',20)
% ylabel('Frequency (kHz)','FontSize',20)
% xlabel('Time (s)','FontSize',20)

I5=I4;



% STEP 6a: JOIN VERTICAL LINES IN IMAGE
% NOTES: join_vertical_lines.m and join_horizontal_lines.m are my functions
I6 = join_vertical_lines(I5);


% STEP 6b: JOIN HORIZONTAL LINES IN IMAGE
I6 = join_horizontal_lines(I6);
% figure(7), clf, imagesc(T,F,I6)
% axis xy; axis tight; colormap(gray); view(0,90);
% xlabel('Time (s)');
% ylabel('Frequency (Hz)');
% colorbar
% set(gca,'XTick',[0:10:tmax],'FontSize',20)
% set(gca,'YTick',[0:2000:fmax],'FontSize',20)
% title('Joined signals in horizontal and vertical directions','FontSize',20)
% ylabel('Frequency (kHz)','FontSize',20)
% xlabel('Time (s)','FontSize',20)

% fid = fopen('I6b.txt', 'wt');
% fprintf(fid, '%f\n', I6);
% fclose(fid);

return


% STEP 7: GET ACOUSTIC EVENTS
% NOTES: get_acoustic_events.m is my function; line.m and imagesc.m are
% MATLAB functions
[AE,L] = get_acoustic_events(I6);
% alternatively uncomment code below to view image and show time and frequency axis
% if ~isempty(AE)
%     [M,N] = size(I1);
%     T = linspace(0,tmax,N);
%     F = linspace(0,fmax,M);
% 
%     figure(8), clf, imagesc(T,F,I1);
%     axis xy; axis tight; colormap(gray); view(0,90);
%     xlabel('Time (s)');
%     ylabel('Frequency (Hz)');
%     colorbar
%     figure(8), hold on, line([T(AE(1,:)); T(AE(1,:))+T(AE(3,:))], [F(AE(2,:)); F(AE(2,:))],'Color','b')
%     figure(8), hold on, line([T(AE(1,:)); T(AE(1,:))+T(AE(3,:))], [F(AE(2,:))+F(AE(4,:)); F(AE(2,:))+F(AE(4,:))],'Color','b')
%     figure(8), hold on, line([T(AE(1,:)); T(AE(1,:))], [F(AE(2,:)); F(AE(2,:))+F(AE(4,:))],'Color','b')
%     figure(8), hold on, line([T(AE(1,:))+T(AE(3,:)); T(AE(1,:))+T(AE(3,:))], [F(AE(2,:)); F(AE(2,:))+F(AE(4,:))],'Color','b')
% 
% 
%     set(gca,'XTick',[0:10:tmax],'FontSize',20)
%     set(gca,'YTick',[0:2000:fmax],'FontSize',20)
%     title('Original Image with Marqueed Acoustic Events','FontSize',20)
%     ylabel('Frequency (kHz)','FontSize',20)
%     xlabel('Time (s)','FontSize',20)
%     % maximise this image on your screen for optimal viewing
% end



% STEP 8: SEPARATE EVENTS THAT ARE TOO LARGE INTO SMALLER EVENTS
% NOTES: mode_large_area_threshold.m and separate_large_AEs_areas.m are my 
% functions; isempty.m is a MATLAB function
if (~isempty(AE)) % do this next step only if acoustic events have been detected
    big_area_thresh = mode_large_area_threshold(AE); % compute large area threshold for separating acoustic events
    if ~isempty(big_area_thresh)
%         AE2 = separate_large_AEs_areas_old(AE,L,big_area_thresh,I6); % separate large area events into smaller events
        AE2 = separate_large_AEs_areas(AE,L,big_area_thresh,I6); % separate large area events into smaller events
    else
        AE2 = [];
    end
else
    AE2 = [];
end

% % sketch rectangular boundaries around new acoustic events - some big ones
% % have been separated into smaller ones
% if ~isempty(AE2)
%     figure(9), clf, imshow(I1)
%     figure(9), clf, imagesc(I1)
%     figure(9), hold on, line([AE2(1,:); AE2(1,:)+AE2(3,:)], [AE2(2,:); AE2(2,:)],'Color','r')
%     figure(9), hold on, line([AE2(1,:); AE2(1,:)+AE2(3,:)], [AE2(2,:)+AE2(4,:); AE2(2,:)+AE2(4,:)],'Color','r')
%     figure(9), hold on, line([AE2(1,:); AE2(1,:)], [AE2(2,:); AE2(2,:)+AE2(4,:)],'Color','r')
%     figure(9), hold on, line([AE2(1,:)+AE2(3,:); AE2(1,:)+AE2(3,:)], [AE2(2,:); AE2(2,:)+AE2(4,:)],'Color','r')
% end


% STEP 9: KEEP LARGE EVENTS ONLY
% NOTES: mode_small_area_threshold.m is my function, size.m is a MATLAB 
% function
if (~isempty(AE2))
    [AE3,small_area_thresh] = mode_small_area_threshold(AE2); % compute small area threshold for culling acoustic events & cull small events
else
    AE3 = [];
    small_area_thresh = [];
end
% sketch remaining acoustic events
if (~isempty(AE3))
    
    [M,N] = size(I1);
    T = linspace(0,tmax,N);
    F = linspace(0,fmax,M);

    
    % alternatively uncomment code below to view image and show time and frequency axis
    figure(10), clf, imagesc(T,F,I1);
    axis xy; axis tight; colormap(gray); view(0,90);
    xlabel('Time (s)');
    ylabel('Frequency (Hz)');
    colorbar
    figure(10), hold on, line([T(AE3(1,:)); T(AE3(1,:))+T(AE3(3,:))], [F(AE3(2,:)); F(AE3(2,:))],'Color','b')
    figure(10), hold on, line([T(AE3(1,:)); T(AE3(1,:))+T(AE3(3,:))], [F(AE3(2,:))+F(AE3(4,:)); F(AE3(2,:))+F(AE3(4,:))],'Color','b')
    figure(10), hold on, line([T(AE3(1,:)); T(AE3(1,:))], [F(AE3(2,:)); F(AE3(2,:))+F(AE3(4,:))],'Color','b')
    figure(10), hold on, line([T(AE3(1,:))+T(AE3(3,:)); T(AE3(1,:))+T(AE3(3,:))], [F(AE3(2,:)); F(AE3(2,:))+F(AE3(4,:))],'Color','b')
    

    set(gca,'XTick',[0:10:tmax],'FontSize',20)
    set(gca,'YTick',[0:2000:fmax],'FontSize',20)
    title('Original Image with Marqueed Acoustic Events','FontSize',20)
    ylabel('Frequency (kHz)','FontSize',20)
    xlabel('Time (s)','FontSize',20)
    % maximise this image on your screen for optimal viewing


end






