function koala_template_example
% bmp 20091007


small_events_thresh = 200;
file_path_audio = pwd;
file_path_acoustic_events = pwd;
EPR_path = pwd;
working_path = pwd;
results_path = pwd;




% int_thresh = 6; % use a lower threshold to create template - has better spacing of events
% tname = 'Top Knoll - St Bees_20080912-213000.wav'; % test image for generating template
% % create template
% template_name = make_image_template_user(tname, file_path_audio, file_path_acoustic_events, results_path, int_thresh, small_events_thresh)
% % use [78 80.5 0 6000]; don't include first (left hand most), or last
% % (right hand most) events in template
% return



template_name = 'Top Knoll - St Bees_20080912-213000.wav_Intensity_Thresh_6dB_Small_area_thresh_max_200_template157';


int_thresh = 9;
name = 'Honeymoon Bay - St Bees_20080905-001000.wav'; % test image
xlsfile = strcat('Matching_',template_name,'.xls');
% scan image for AEs in appropriate frequency band; store results in xlsfile
scan_image_for_AEs_koala(name, file_path_audio, file_path_acoustic_events, results_path, int_thresh, small_events_thresh, xlsfile, template_name)



% do matching
template_matching_using_distance_koala(name, file_path_audio, file_path_acoustic_events, results_path, int_thresh, small_events_thresh, xlsfile, template_name)


% check out results
score = 3; % rate every score higher than 3 (out of 14 if above template is used) as a match
examine_all_AE_results(name, file_path_audio, file_path_acoustic_events, results_path, int_thresh, small_events_thresh, xlsfile, score)
