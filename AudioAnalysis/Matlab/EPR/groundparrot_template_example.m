function groundparrot_template_example
% bmp 20090923


int_thresh = 3;
small_events_thresh = 100;
file_path_audio = pwd;
file_path_acoustic_events = pwd;
EPR_path = pwd;
working_path = pwd;
results_path = pwd;

% file
name = 'GParrots_JB2_20090607-173000.wav_minute_3.wav';


% read in template
template_name = 'ground_parrot_image_template_47';


xlsfile = strcat('Matching_',template_name,'.xls')
% scan image for AEs in appropriate frequency band; store results in xlsfile
scan_image_for_AEs(name, file_path_audio, file_path_acoustic_events, results_path, int_thresh, small_events_thresh, xlsfile, template_name)

% do matching
template_matching_using_distance(name, file_path_audio, file_path_acoustic_events, results_path, int_thresh, small_events_thresh, xlsfile, template_name)


% check out results
score = 3.5; % rate every score higher than 1.5 (out of 4 if above template is used) as a match
examine_all_AE_results(name, file_path_audio, file_path_acoustic_events, results_path, int_thresh, small_events_thresh, xlsfile, score)
