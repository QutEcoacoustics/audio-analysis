function do_acoustic_event_detection
% This code starts with a raw audio file; applies image processing
% and returns areas (four points of a rectangle) that are designated as
% acoustic events
%
% Sensor Networks Project
% Birgit Planitz
% 20090415


% PARAMETERS
w = 5; %window length of wxw window used in wiener filtering
int_thresh = 9; % intensity threshold
big_area_thresh = 3000;
ctmp = colormap(gray); c = flipud(ctmp); %colormap for plotting in grayscale



% STEP 1: COMPUTE SPECTROGRAM
addpath('../Common')
[y,fs,I1,F,T,fmax,tmax] = wavToSpectrogram('../../AED/Test/matlab/BAC2_20071015-045040.wav');
[M,N] = size(I1);
show_image(c,I1,T,F,1);

% STEP 2: WIENER FILTERING
I2 = wiener2(I1, [w w]);

% STEP 3: GET MODAL INTENSITIES OF SUBBBANDS and REMOVE FROM IMAGE
I3 = subband_mode_intensities(I2);

% STEP 4: CONVERTS IMAGE TO BLACK AND WHITE USING INTENSITY THRESHOLD
I4 = image_thresh_bw(I3,int_thresh);

% STEP 5: JOIN VERTICAL LINES IN IMAGE
I5 = join_vertical_lines(I4);

% STEP 6: JOIN HORIZONTAL LINES IN IMAGE
I6 = join_horizontal_lines(I5);

% STEP 7: GET ACOUSTIC EVENTS
[AE,L] = get_acoustic_events(I6,I1,I2,I3);

% STEP 8: SEPARATE EVENTS THAT ARE TOO LARGE INTO SMALLER EVENTS
AE2 = [];
if (~isempty(AE)) % do this next step only if acoustic events have been detected
    AE2 = separate_large_AEs_areas(AE,L,big_area_thresh,I6,I1,I2,I3); % separate large area events into smaller events
end

% STEP 9: KEEP LARGE EVENTS ONLY
AE3 = [];
if (~isempty(AE2))
    [AE3,small_area_thresh] = mode_small_area_threshold(AE2); % compute small area threshold for culling acoustic events & cull small events
end

% STEP 10: CONVERT ACOUSTIC EVENTS TO TIME AND FREQUENCY
AE4 = [];
if (~isempty(AE3))
    [rAE,cAE] = size(AE3');
    AE4 = zeros(rAE,4);
    AE4(:,1) = T([AE3(1,:)]);
    AE4(:,2) = max(T) / N * AE3(3,:);
    AE4(:,3) = F([AE3(2,:)]);
    AE4(:,4) = F([AE3(2,:) + AE3(4,:) - 1]);
end
show_image(c,I3,T,F,2,AE4);
