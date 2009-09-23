function template_matching_using_distance(name, file_path_audio, file_path_acoustic_events, results_path, int_thresh, small_events_thresh, xlsfile, template_name)
% do template matching on AEs in specific frequency band
% bmp 20090917



working_path = pwd;


% OTHER PARAMETERS - hardcoded for the moment
window = 512; % hamming window using 512 samples
noverlap = round(0.5*window); % 50% overlap between frames
nfft = 256*2-1; % yield 512 frequency bins



% get AEs in a specific freq band
cd(results_path)
[all_results,all_txt] = xlsread(xlsfile);
cd(working_path)
num_g = size(all_results,1);



% LOAD AND MANIPULATE TEMPLATE
% load template
load(strcat(template_name,'.mat'))
I3_template = template.I;
templateAE = template.AE;
templateT = template.T;
templateF = template.F;
lenT = length(templateT);
lenF = length(templateF);

% template AEs - centroids shifted
AE1 = templateAE;
num_t = size(AE1,1);
AE1(:,1) = templateAE(:,1)-min(templateAE(:,1));
AE1(:,3) = templateAE(:,3)-min(templateAE(:,3));
AE1(:,4) = templateAE(:,4)-min(templateAE(:,3));
plotAE1 = AE1';

% centroids
tcAE1 = AE1(:,1) + AE1(:,2)/2; %time domain centroids 
fcAE1 = AE1(:,3) + (AE1(:,4)-AE1(:,3))/2; %freq domain centroids 
% centroids expressed as pixel values
tcAE1_pixels = round(tcAE1/(templateT(end)-templateT(1))*lenT);
tcAE1_pixels(tcAE1_pixels<1) = 1;
tcAE1_pixels(tcAE1_pixels>lenT) = lenT;
fcAE1_pixels = round(fcAE1/(templateF(end)-templateF(1))*lenF);
fcAE1_pixels(fcAE1_pixels<1) = 1;
fcAE1_pixels(fcAE1_pixels>lenF) = lenF;

% start points expressed as pixels
tsAE1 = AE1(:,1); %time domain start 
fsAE1 = AE1(:,3); %freq domain start 
% centroids expressed as pixel values
tsAE1_pixels = round(tsAE1/(templateT(end)-templateT(1))*lenT);
tsAE1_pixels(tsAE1_pixels<1) = 1;
tsAE1_pixels(tsAE1_pixels>lenT) = lenT;
fsAE1_pixels = round(fsAE1/(templateF(end)-templateF(1))*lenF);
fsAE1_pixels(fsAE1_pixels<1) = 1;
fsAE1_pixels(fsAE1_pixels>lenF) = lenF;


last_name = [];
for ng = 1:num_g
    
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
    
    % get AEs in specific freq band - computed in scan_image_for_AEs.m
    num_d = (all_results(ng,6));
    if num_d==1
        AE_inds = all_results(ng,7);
    else
        dstring = '';
        for nd = 1:num_d;
            dstring = strcat(dstring,'%d');
        end
        tmp1 = str2mat(all_txt(ng+1,8));
        tmp2 = textscan(tmp1, dstring, 1);
        AE_inds = cell2mat(tmp2);
    end
    num_a = length(AE_inds);
    thisAE = allAE(AE_inds,:);
    
%     show_image(I2,T,F,tmax,fmax,4,allAE')
%     show_image(I2,T,F,tmax,fmax,4,thisAE',1)
 
    % find closest centroids when laying image and template on top of each
    % other
    
    
    
    
    
    % UNCOMMENT figure(5), clf and 'hold on' lines to see template
    %   also
    % UNCOMMENT figure(5), and 'hold on' lines to see image segments being tested
    %   also
    % UNCOMMENT pause after disp(total_overlap), to pause between each new matching test
    
    
    
    
    
    
    % template AEs - centroid shifted
%     figure(5), clf
%     hold on, line([plotAE1(1,:); plotAE1(1,:)+plotAE1(2,:)], [plotAE1(3,:); plotAE1(3,:)],'Color','k')
%     hold on, line([plotAE1(1,:); plotAE1(1,:)+plotAE1(2,:)], [plotAE1(4,:); plotAE1(4,:)],'Color','k')
%     hold on, line([plotAE1(1,:); plotAE1(1,:)], [plotAE1(3,:); plotAE1(4,:)],'Color','k')
%     hold on, line([plotAE1(1,:)+plotAE1(2,:); plotAE1(1,:)+plotAE1(2,:)], [plotAE1(3,:); plotAE1(4,:)],'Color','k')

    % test AEs - centroid shifted
    AE2 = thisAE;
    AE2(:,1) = thisAE(:,1)-min(thisAE(:,1));
    AE2(:,3) = thisAE(:,3)-min(thisAE(:,3));
    AE2(:,4) = thisAE(:,4)-min(thisAE(:,3));
    plotAE2 = AE2';
%     figure(5), 
%     hold on, line([plotAE2(1,:); plotAE2(1,:)+plotAE2(2,:)], [plotAE2(3,:); plotAE2(3,:)],'Color','r')
%     hold on, line([plotAE2(1,:); plotAE2(1,:)+plotAE2(2,:)], [plotAE2(4,:); plotAE2(4,:)],'Color','r')
%     hold on, line([plotAE2(1,:); plotAE2(1,:)], [plotAE2(3,:); plotAE2(4,:)],'Color','r')
%     hold on, line([plotAE2(1,:)+plotAE2(2,:); plotAE2(1,:)+plotAE2(2,:)], [plotAE2(3,:); plotAE2(4,:)],'Color','r')

    tcAE2 = AE2(:,1) + AE2(:,2)/2; %time domain centroids 
    fcAE2 = AE2(:,3) + (AE2(:,4)-AE2(:,3))/2; %freq domain centroids 

    % centroids
    tcAE2 = AE2(:,1) + AE2(:,2)/2; %time domain centroids 
    fcAE2 = AE2(:,3) + (AE2(:,4)-AE2(:,3))/2; %freq domain centroids 
    % centroids expressed as pixel values
    tcAE2_pixels = round(tcAE2/(templateT(end)-templateT(1))*lenT);
    tcAE2_pixels(tcAE2_pixels<1) = 1;
    tcAE2_pixels(tcAE2_pixels>lenT) = lenT;
    fcAE2_pixels = round(fcAE2/(templateF(end)-templateF(1))*lenF);
    fcAE2_pixels(fcAE2_pixels<1) = 1;
    fcAE2_pixels(fcAE2_pixels>lenF) = lenF;

    % start points expressed as pixels
    tsAE2 = AE2(:,1); %time domain start 
    fsAE2 = AE2(:,3); %freq domain start 
    % centroids expressed as pixel values
    tsAE2_pixels = round(tsAE2/(templateT(end)-templateT(1))*lenT);
    tsAE2_pixels(tsAE2_pixels<1) = 1;
    tsAE2_pixels(tsAE2_pixels>lenT) = lenT;
    fsAE2_pixels = round(fsAE2/(templateF(end)-templateF(1))*lenF);
    fsAE2_pixels(fsAE2_pixels<1) = 1;
    fsAE2_pixels(fsAE2_pixels>lenF) = lenF;

    
    % Euclidean distance measure
    tcAE1_mat = repmat(tcAE1_pixels,1,num_a);
    fcAE1_mat = repmat(fcAE1_pixels,1,num_a);
    
    tcAE2_mat = repmat(tcAE2_pixels,1,num_t)';
    fcAE2_mat = repmat(fcAE2_pixels,1,num_t)';
    
    euc_dist = sqrt( (tcAE1_mat-tcAE2_mat).^2 + (fcAE1_mat-fcAE2_mat).^2 );
    [tmp, loc_dist] = min(euc_dist,[],2); % location of nearest neighbouring test events
    
    
    % find precentage overlap between (rectangular) AEs and their closest template AE
    total_overlap = 0;
    for ll = 1:num_t
        
        % find overlapping points
        start_t1 = tsAE1_pixels(ll);
        start_t2 = tsAE2_pixels(loc_dist(ll));
        end_t1 = tsAE1_pixels(ll) + (tcAE1_pixels(ll)-tsAE1_pixels(ll))*2;
        end_t2 = tsAE2_pixels(loc_dist(ll)) + (tcAE2_pixels(loc_dist(ll))-tsAE2_pixels(loc_dist(ll)))*2;
        start_f1 = fsAE1_pixels(ll);
        start_f2 = fsAE2_pixels(loc_dist(ll));
        end_f1 = fsAE1_pixels(ll) + (fcAE1_pixels(ll)-fsAE1_pixels(ll))*2;
        end_f2 = fsAE2_pixels(loc_dist(ll)) + (fcAE2_pixels(loc_dist(ll))-fsAE2_pixels(loc_dist(ll)))*2;
        
        % find overlap
        start_to = max(start_t1,start_t2);
        end_to = min(end_t1,end_t2);
        start_fo = max(start_f1,start_f2);
        end_fo = min(end_f1,end_f2);
        
        % conditions for NO overlap - initialise at zero
        cond_time = 0;
        cond_freq = 0;
        
        cond_time( end_to<start_to )  = 1;
        cond_freq( end_fo<start_fo ) = 1;
        
        if ( (cond_time==1) | (cond_freq==1) )
            overlap = 0;
        else
            area_1 = (end_t1-start_t1)*(end_f1-start_f1);
            area_2 = (end_t2-start_t2)*(end_f2-start_f2);
            area_o = (end_to-start_to)*(end_fo-start_fo);
            overlap = 0.5 * ( (area_o/area_1) + (area_o/area_2) );
        end
        total_overlap = total_overlap + overlap;
        
%         pause
        
    end
        
    % store total_overlap
    cntr = ng+1
    cd(results_path)
    xlswrite(xlsfile, total_overlap, strcat('I',num2str(cntr),':I',num2str(cntr)))
    cd(working_path)
    
    disp(total_overlap)
%     pause
end


function show_image(I1,T1,F1,tmax,fmax,fig_num,AE1,holdonyes)

c = colormap(gray);
% c = flipud(c);

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


