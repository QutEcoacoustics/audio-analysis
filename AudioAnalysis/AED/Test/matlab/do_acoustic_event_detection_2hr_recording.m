function do_acoustic_event_detection_2hr_recording
% This code starts with a raw audio file; applies image processing
% and returns areas (four points of a rectangle) that are designated as
% acoustic events
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
    
    disp(nn)
    
    cd 'G:\Birgit\Sensor\image_proc\Jason 2hr recording'
    txt = strcat('recording_minute_',num2str(nn));

    % read audio data
    [y, fs, nbits, opts] = wavread(strcat(txt,'.wav'));
    cd 'G:\Birgit\Sensor\image_proc\Acoustic Analysis - Brandes - 20090415'

    
    
    % STEP 1: GENERATE SPECTROGRAM
    window = 512; % hamming window using 512 samples
    noverlap = round(0.5*window); % 50% overlap between frames
    nfft = 256*2-1; % yield 512 frequency bins
    [S,F,T,P] = spectrogram(y,window,noverlap,nfft,fs);
    
    % convert amplitude to dB
    I1 = 10*log10(abs(P));
    


    % STEP 2: WIENER FILTERING
    % NOTES: wiener2.m is a MATLAB function
    w = 5; %window length of wxw window used in wiener filtering
    I2 = wiener2(I1, [w w]);
    

    % STEP 3: GET MODAL INTENSITIES OF SUBBBANDS and REMOVE FROM IMAGE
    % NOTES: subband_mode_intensities.m is my function
    I3 = subband_mode_intensities(I2);
    

    % STEP 5: CONVERTS IMAGE TO BLACK AND WHITE USING INTENSITY THRESHOLD
    int_thresh = 7;
    I4 = image_thresh_bw(I3,int_thresh);
    

    % STEP 6a: JOIN VERTICAL LINES IN IMAGE
    % NOTES: join_vertical_lines.m and join_horizontal_lines.m are my functions
    I5 = join_vertical_lines(I4);
    
    % STEP 6b: JOIN HORIZONTAL LINES IN IMAGE
    I6 = join_horizontal_lines(I5);
    


    % STEP 7: GET ACOUSTIC EVENTS
    % NOTES: get_acoustic_events.m is my function; line.m and imagesc.m are
    % MATLAB functions
    [AE,L] = get_acoustic_events(I6);
    

    % STEP 8: SEPARATE EVENTS THAT ARE TOO LARGE INTO SMALLER EVENTS
    % NOTES: mode_large_area_threshold.m and separate_large_AEs_areas.m are my 
    % functions; isempty.m is a MATLAB function
    if (~isempty(AE)) % do this next step only if acoustic events have been detected
        big_area_thresh = mode_large_area_threshold(AE); % compute large area threshold for separating acoustic events
        if ~isempty(big_area_thresh)
            AE2 = separate_large_AEs_areas(AE,L,big_area_thresh,I6); % separate large area events into smaller events
        else
            AE2 = [];
        end
    else
        AE2 = [];
    end
    


    % STEP 9: KEEP LARGE EVENTS ONLY
    % NOTES: mode_small_area_threshold.m is my function, size.m is a MATLAB 
    % function
    if (~isempty(AE2))
        [AE3,small_area_thresh] = mode_small_area_threshold(AE2); % compute small area threshold for culling acoustic events & cull small events
    else
        AE3 = [];
        small_area_thresh = [];
    end
    
    % save list of final acoustic events
    cd 'G:\Birgit\Sensor\image_proc\Jason 2hr recording'
    save(strcat('int_thresh_',num2str(int_thresh),'dB_',txt,'.mat'),'AE3')
    cd 'G:\Birgit\Sensor\image_proc\Acoustic Analysis - Brandes - 20090415'
    
%     pause
    
end




