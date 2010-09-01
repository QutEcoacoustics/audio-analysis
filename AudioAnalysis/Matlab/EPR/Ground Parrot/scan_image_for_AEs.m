function scan_image_for_AEs(name, F, int_thresh, small_events_thresh, xlsfile, template_name)
% Get all AEs where the min freq is within a specific freq band (fband_min,fband_max) 
%
% Match image (size of template with AE situated in bottom left hand
% corner) against template
%
% bmp 20091102



% parameters
fmax = max(F);

%paths
addpath('G:\Birgit\Subversion\AudioAnalysis\Matlab\Common')
addpath('G:\Birgit\Acoustic Data\Ground parrots - acoustic events2')

% get acoustic events
[this_results] = xlsread(strcat(name,'_Intensity_Thresh_',num2str(int_thresh),'dB_Small_area_thresh_max_',num2str(small_events_thresh),'.xls'));
allAE = this_results(:,1:4); % all acoustic events


% load template
load(strcat(template_name,'.mat'))
templateT = template.T;
template_start = templateT(1);
template_end = templateT(end);
templateF = template.F;
template_bottom = templateF(1);
template_top = templateF(end);


% search for similar image segments in specific frequency range of input image
freq_bottom_thresh1 = template_bottom - 500; % HARDCODED 
freq_bottom_thresh1(freq_bottom_thresh1<0) = 0;
freq_bottom_thresh2 = template_bottom + 500; % HARDCODED 
freq_bottom_thresh2(freq_bottom_thresh2>fmax) = fmax;

time_start = allAE(:,1);
freq_bottom = allAE(:,3);
indAE = find( (freq_bottom > freq_bottom_thresh1) & (freq_bottom < freq_bottom_thresh2) );
lenAE = length(indAE);
AE = allAE(indAE,:);

scan_results = struct('AE',{},'numBoundary',{},'indBoundary',{});
% retrieve image (size of template) with AE at bottom left hand corner
for na = 1:lenAE

    this_time_start = AE(na,1);
    this_time_end = AE(na,1) + (template_end-template_start);
    this_freq_bottom = AE(na,3);
    this_freq_top = AE(na,3) + (template_top-template_bottom);

    % find all acoustic events that fit within these boundaries
    ind_boundary_AE = find( (time_start>=this_time_start) & (time_start<this_time_end) & (freq_bottom>=this_freq_bottom) & (freq_bottom<this_freq_top) );
    num_boundary_AE = length(ind_boundary_AE);

    scan_results(na).AE = [this_time_start, this_time_end-this_time_start, this_freq_bottom, this_freq_top];
    scan_results(na).numBoundary = num_boundary_AE;
    scan_results(na).indBoundary = num2str(ind_boundary_AE');
    
end

% store results in excel file
xlswrite(xlsfile,{'Start Time (s)','Duration (s)','Lowest Freq','Heighest Freq','# of AEs','Acoustic Events','%Overlap'}, 'A1:G1');
if size(scan_results)>0
    xlswrite(xlsfile, cat(1,scan_results.AE), strcat('A',num2str(2),':D',num2str(lenAE+1)))
    xlswrite(xlsfile, cat(1,scan_results.numBoundary), strcat('E',num2str(2),':E',num2str(lenAE+1)))
    tmp = strvcat(scan_results.indBoundary);
end
for na = 1:lenAE
    xlswrite(xlsfile, {tmp(na,:)}, strcat('F',num2str(na+1),':F',num2str(na+1)))
end
