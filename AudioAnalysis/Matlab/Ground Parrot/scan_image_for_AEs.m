function scan_image_for_AEs(name, file_path_audio, file_path_acoustic_events, results_path, int_thresh, small_events_thresh, xlsfile, template_name)
% Get all AEs where the min freq is within a specific freq band (fband_min,fband_max) 
%
% Match image (size of template with AE situated in bottom left hand
% corner) against template
%
% bmp 20090917


working_path = pwd;


% OTHER PARAMETERS - hardcoded for the moment
window = 512; % hamming window using 512 samples
noverlap = round(0.5*window); % 50% overlap between frames
nfft = 256*2-1; % yield 512 frequency bins



% GENERATE SPECTROGRAM
% read audio data
cd(file_path_audio)
[y, fs, nbits, opts] = wavread(name);
cd(working_path)
leny = length(y);

% get original image
[S,F,T,P] = spectrogram(y,window,noverlap,nfft,fs);
I1 = 10*log10(abs(P)); % convert amplitude to dB
% variables below are for plotting - later in code
[M,N] = size(I1);
tmax = length(y)/fs; %length of signal in seconds
fmax = 11025;
T = linspace(0,tmax,N);
F = linspace(0,fmax,M);
% wiener filtering
w = 5; %window length of wxw window used in wiener filtering
I2 = wiener2(I1, [w w]);
% remove subband noise
I3 = subband_mode_intensities(I2);



% GET ACOUSTIC EVENTS
cd(file_path_acoustic_events)
[this_results] = xlsread(strcat(name,'_Intensity_Thresh_',num2str(int_thresh),'dB_Small_area_thresh_max_',num2str(small_events_thresh),'.xls'));
cd(working_path)
allAE = this_results(:,1:4); % all acoustic events
% show_image(I2,T,F,tmax,fmax,1,allAE')



% AE SEARCH/MATCHING
% initialise an excel file for storing data
cd(results_path)
xlswrite(xlsfile,{'Filename','Start Time (s)','Duration (s)','Lowest Freq','Heighest Freq','User classification','# of AEs','Acoustic Events','%Overlap'}, 'A1:I1');
cd(working_path)
cntr = 1; % init counter for storing results in excel file


% load template
load(strcat(template_name,'.mat'))
I3_template = template.I;
[tM,tN] = size(I3_template);
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
time_end = allAE(:,1)+allAE(:,2);
freq_top = allAE(:,4);
freq_bottom = allAE(:,3);
indAE = find( (freq_bottom > freq_bottom_thresh1) & (freq_bottom < freq_bottom_thresh2) );
lenAE = length(indAE);
AE = allAE(indAE,:);
    
show_image(I2,T,F,tmax,fmax,4,allAE')
show_image(I2,T,F,tmax,fmax,4,AE',1)
    
% retrieve image (size of template) with AE at bottom left hand corner
for na = 1:lenAE

    this_time_start = AE(na,1);
    [tmp2, left] = min(abs(T-this_time_start));
    this_time_end = AE(na,1) + (template_end-template_start);
    [tmp2, right] = min(abs(T-this_time_end));
    this_freq_bottom = AE(na,3);
    [tmp2, bottom] = min(abs(F-this_freq_bottom));
    this_freq_top = AE(na,3) + (template_top-template_bottom);
    [tmp2, top] = min(abs(F-this_freq_top));
    I3_seg_all = zeros(tM,tN);
    I3_seg_all(1:top-bottom+1,1:right-left+1) = I3(bottom:top,left:right);

    % find all acoustic events that fit within these boundaries
    ind_boundary_AE = find( (time_start>=this_time_start) & (time_start<this_time_end) & (freq_bottom>=this_freq_bottom) & (freq_bottom<this_freq_top) );
    num_boundary_AE = length(ind_boundary_AE);
    boundary_AE = allAE(ind_boundary_AE,:);

    % store AEs that can be used for matching (excel file)
    cntr = cntr + 1;

    cd(results_path)
    xlswrite(xlsfile, {name}, strcat('A',num2str(cntr),':A',num2str(cntr)))
    xlswrite(xlsfile, [this_time_start, this_time_end-this_time_start, this_freq_bottom, this_freq_top], strcat('B',num2str(cntr),':E',num2str(cntr)))
    xlswrite(xlsfile, num_boundary_AE, strcat('G',num2str(cntr),':G',num2str(cntr)))
    xlswrite(xlsfile, {num2str(ind_boundary_AE')}, strcat('H',num2str(cntr),':H',num2str(cntr)))

%     % uncomment below to do user classification
%     %listen to sound
%     user_entry1 = input('Play audio (y/n)?', 's');
%     if ~issame(user_entry1,'n')
%         tleft = round(fs*time_start);
%         tleft(tleft<1)=1;
%         tright = round(fs*time_end);
%         tright(tright>leny)=leny;
%         wavplay(y([tleft:tright]),fs)
%     end
%     % user classify
%     user_entry2 = input('Sound type', 's');
%     xlswrite(xlsfile, {user_entry2}, strcat('F',num2str(cntr),':F',num2str(cntr)))

    cd(working_path)
end






function show_image(I1,T1,F1,tmax,fmax,fig_num,AE1,holdonyes)

c = colormap(gray);
c = flipud(c);

if nargin ==6
    
    figure(fig_num), clf, imagesc(T1,F1,I1);
    axis xy; axis tight; colormap(c); view(0,90);
    ylabel('Frequency (kHz)','FontSize',20)
    xlabel('Time (s)','FontSize',20)
    set(gca,'XTick',[0:10:tmax],'FontSize',20)
    set(gca,'YTick',[0:2000:fmax],'FontSize',20)
    colorbar
   
elseif nargin ==7 
    
    if isempty(AE1)

        figure(fig_num), clf, imagesc(T1,F1,I1);
        axis xy; axis tight; colormap(c); view(0,90);
        ylabel('Frequency (kHz)','FontSize',20)
        xlabel('Time (s)','FontSize',20)
        set(gca,'XTick',[0:10:tmax],'FontSize',20)
        set(gca,'YTick',[0:2000:fmax],'FontSize',20)
        colorbar
    
    else

        figure(fig_num), clf, imagesc(T1,F1,I1);
        axis xy; axis tight; colormap(c); view(0,90);
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(3,:)],'Color','b')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(4,:); AE1(4,:)],'Color','b')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)], [AE1(3,:); AE1(4,:)],'Color','b')
        figure(fig_num), hold on, line([AE1(1,:)+AE1(2,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(4,:)],'Color','b')
        set(gca,'XTick',[0:10:tmax],'FontSize',20)
        set(gca,'YTick',[0:2000:fmax],'FontSize',20)
        title('Filtered Image with Marqueed Acoustic Events AE1','FontSize',20)
        ylabel('Frequency (kHz)','FontSize',20)
        xlabel('Time (s)','FontSize',20)
        colorbar
    end    
    
    elseif nargin ==8 
    
    if ( ~isempty(AE1) & (holdonyes==1) )

        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(3,:)],'Color','r')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(4,:); AE1(4,:)],'Color','r')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)], [AE1(3,:); AE1(4,:)],'Color','r')
        figure(fig_num), hold on, line([AE1(1,:)+AE1(2,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(4,:)],'Color','r')
        set(gca,'XTick',[0:10:tmax],'FontSize',20)
        set(gca,'YTick',[0:2000:fmax],'FontSize',20)
        title('Filtered Image with Marqueed Acoustic Events AE1','FontSize',20)
        ylabel('Frequency (kHz)','FontSize',20)
        xlabel('Time (s)','FontSize',20)
        colorbar
    end    
end

