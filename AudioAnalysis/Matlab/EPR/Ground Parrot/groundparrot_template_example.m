function groundparrot_template_example
% bmp 20100901



% Audio file for test
name = 'GParrots_JB2_20090607-173000.wav_minute_8.wav'; 
% NOTE: acoustic events have already been computed and AED excel file is
% stored in current directory

% AED computation parameter settings - used in file names for clarity
int_thresh = 3;
small_events_thresh = 100;



% Image processing - noise removal
addpath('../../Common')
[y,fs,I1,F,T,] = wavToSpectrogram(name);
w = 5; %window length of wxw window used in wiener filtering
I2 = wiener2(I1, [w w]);
% remove subband noise
I3 = withoutSubbandModeIntensities(I2);


% read in template
template_name = 'GParrots_JB2_20090607-173000.wav_minute_8.wav_Intensity_Thresh_6dB_Small_area_thresh_max_100_template52'; %20100816 tests


% excel file for storing match results
xlsfile = strcat('Matching_',template_name,'.xls');


% scan image for AEs in appropriate frequency band; store results in xlsfile
scan_image_for_AEs(name, F, int_thresh, small_events_thresh, xlsfile, template_name)


% do matching
template_matching_using_distance(name, int_thresh, small_events_thresh, xlsfile, template_name)


% check out results
score = 4; 
examine_all_AE_results(name, I3, T, F, int_thresh, small_events_thresh, xlsfile, score)


% image has been saved - see
% GParrots_JB2_20090607-173000.wav_minute_8.wav.png