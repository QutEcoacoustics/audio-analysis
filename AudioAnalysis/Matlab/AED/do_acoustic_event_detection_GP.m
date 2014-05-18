function do_acoustic_event_detection_GP
% This code starts with a raw audio file; applies image processing
% and returns areas (four points of a rectangle) that are designated as
% acoustic events
%
% PARAMETERS SET FOR GROUND PARROT FILE
%
% Sensor Networks Project
% Birgit Planitz
% 20091215


% PARAMETERS
w = 5; %window length of wxw window used in wiener filtering
int_thresh = 3; % intensity threshold
big_area_thresh = 3000;
small_area_thresh = 100; % maximum cut-off point for area size
ctmp = colormap(gray); c = flipud(ctmp); %colormap for plotting in grayscale


% STEP 1: COMPUTE SPECTROGRAM
addpath('../Common')
[y,fs,I1,F,T,] = wavToSpectrogram('../EPR/Ground Parrot/GParrots_JB2_20090607-173000.wav_minute_3.wav');
[M,N] = size(I1);
showImage(c,I1,T,F,1);
% csvout('I1.csv',I1)

% STEP 2: WIENER FILTERING
I2 = wiener2(I1, [w w]);
% csvout('I2.csv',I2)

% STEP 3: GET MODAL INTENSITIES OF SUBBBANDS and REMOVE FROM IMAGE
I3 = withoutSubbandModeIntensities(I2);
% csvout('I3.csv',I3)

% STEP 4: CONVERTS IMAGE TO BLACK AND WHITE USING INTENSITY THRESHOLD
I4 = image_thresh_bw(I3,int_thresh);
% csvout('I4.csv',I4)

% STEP 5: JOIN VERTICAL LINES IN IMAGE
I5 = join_vertical_lines(I4);
% csvout('I5.csv',I5)

% STEP 6: JOIN HORIZONTAL LINES IN IMAGE
I6 = join_horizontal_lines(I5);
% csvout('I6.csv',I6)

% STEP 7: GET ACOUSTIC EVENTS
[AE,L] = get_acoustic_events(I6,I1,I2,I3);
% csvout('AE1.csv',AE)

% STEP 8: SEPARATE EVENTS THAT ARE TOO LARGE INTO SMALLER EVENTS
AE2 = [];
if (~isempty(AE)) % do this next step only if acoustic events have been detected
    AE2 = separate_large_AEs_areas(AE,L,big_area_thresh,I6,I1,I2,I3); % separate large area events into smaller events
end
% csvout('AE2.csv',AE2)

% STEP 9: KEEP LARGE EVENTS ONLY
AE3 = [];
if (~isempty(AE2))
    AE3 = mode_small_area_threshold(AE2, small_area_thresh); % compute small area threshold for culling acoustic events & cull small events
end
% csvout('AE3.csv',AE3)

% STEP 10: CONVERT ACOUSTIC EVENTS TO TIME AND FREQUENCY
AE4 = [];
if (~isempty(AE3))
    [rAE,cAE] = size(AE3');
    AE4 = zeros(rAE,4);
    AE4(:,1) = T([AE3(1,:)]);
    AE4(:,2) = T([AE3(3,:)]);;
    AE4(:,3) = F([AE3(2,:)]);
    AE4(:,4) = F([AE3(2,:) + AE3(4,:) - 1]);
end
showImage(c,I3,T,F,2,AE4);

% SAVE IN EXCEL FILE - 20091215
if (~isempty(AE3))
    [rAE,cAE] = size(AE3');
    allAE = zeros(rAE,4);
    allAE(:,1) = T([AE3(1,:)]);
    allAE(:,2) = T([AE3(3,:)]);
    allAE(:,3) = F([AE3(2,:)]);
    allAE(:,4) = F([AE3(2,:) + AE3(4,:) - 1]);
end
workingpath = pwd
cd('../EPR/Ground Parrot/')
xlsfile = strcat('GParrots_JB2_20090607-173000.wav_minute_3.wav_Intensity_Thresh_',num2str(int_thresh),'dB_Small_area_thresh_max_',num2str(small_area_thresh),'.xls');
xlswrite(xlsfile,{'Start Time (s)', 'Duration (s)', 'Lowest Freq' , 'Heighest Freq'},'A1:D1')
if (~isempty(AE3))
    xlswrite(xlsfile,allAE,strcat('A2:D',num2str(rAE)))
end
cd(workingpath)
end

function csvout(name, M)
    dlmwrite(name, M, 'precision', 8)
end
