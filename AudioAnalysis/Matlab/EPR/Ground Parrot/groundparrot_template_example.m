function groundparrot_template_example
% bmp 20090923

int_thresh = 3;
small_events_thresh = 100;

% file
name = 'GParrots_JB2_20090607-173000.wav_minute_3.wav';

addpath('../../Common')
[y,fs,I1,F,T,] = wavToSpectrogram(name);
w = 5; %window length of wxw window used in wiener filtering
I2 = wiener2(I1, [w w]);
% remove subband noise
I3 = withoutSubbandModeIntensities(I2);

% read in template
% template_name = 'ground_parrot_image_template_47';
% template_name = 'GParrots_JB2_20090607-173000.wav_minute_3.wav_Intensity_Thresh_3dB_Small_area_thresh_max_100_template50';
template_name = 'GParrots_JB2_20090607-173000.wav_minute_3.wav_Intensity_Thresh_3dB_Small_area_thresh_max_100_template72'; % new template created 20091215 with new AED

xlsfile = strcat('Matching_',template_name,'.xls');
% scan image for AEs in appropriate frequency band; store results in xlsfile
scan_image_for_AEs(name, F, int_thresh, small_events_thresh, xlsfile, template_name)



% do matching
template_matching_using_distance(name, int_thresh, small_events_thresh, xlsfile, template_name)


% check out results
score = 4; 
examine_all_AE_results(name, I3, T, F, int_thresh, small_events_thresh, xlsfile, score)
