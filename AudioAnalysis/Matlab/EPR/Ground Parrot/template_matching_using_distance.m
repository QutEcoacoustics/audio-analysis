function template_matching_using_distance(name, int_thresh, small_events_thresh, xlsfile, template_name)
% do template matching on AEs in specific frequency band
% bmp 20091102



%paths
addpath('G:\Birgit\Subversion\AudioAnalysis\Matlab\Common')
addpath('G:\Birgit\Acoustic Data\Ground parrots - acoustic events2')

% get acoustic events
[this_results] = xlsread(strcat(name,'_Intensity_Thresh_',num2str(int_thresh),'dB_Small_area_thresh_max_',num2str(small_events_thresh),'.xls'));
allAE = this_results(:,1:4); % all acoustic events


% get AEs in a specific freq band
[all_results,all_txt] = xlsread(xlsfile);
num_g = size(all_results,1);



% LOAD AND MANIPULATE TEMPLATE
% load template
load(strcat(template_name,'.mat'))
templateT = template.T;
templateF = template.F;
templateAE = template.AE;
lenT = length(templateT);
lenF = length(templateF);

% template AEs - centroids shifted
AE1 = templateAE;
num_t = size(AE1,1);
AE1(:,1) = templateAE(:,1)-min(templateAE(:,1));
AE1(:,3) = templateAE(:,3)-min(templateAE(:,3));
AE1(:,4) = templateAE(:,4)-min(templateAE(:,3));

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

% initialise
keep_overlap = zeros(num_g,1);
for ng = 1:num_g
    
    % get AEs in specific freq band - computed in scan_image_for_AEs.m
    num_d = (all_results(ng,5));
    if num_d==1
        AE_inds = all_results(ng,6);
    else
        dstring = '';
        for nd = 1:num_d;
            dstring = strcat(dstring,'%d');
        end
        tmp1 = str2mat(all_txt(ng+1,6));
        tmp2 = textscan(tmp1, dstring, 1);
        AE_inds = cell2mat(tmp2);
    end
    
    num_a = length(AE_inds);
    thisAE = allAE(AE_inds,:);
    
    % find closest centroids when laying image and template on top of each
    % other

    % test AEs - centroid shifted
    AE2 = thisAE;
    AE2(:,1) = thisAE(:,1)-min(thisAE(:,1));
    AE2(:,3) = thisAE(:,3)-min(thisAE(:,3));
    AE2(:,4) = thisAE(:,4)-min(thisAE(:,3));

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
    [tmp, loc_dist] = min(euc_dist,[],1); % location of nearest neighbouring test events
    num_ld = length(loc_dist);
    
    if num_ld> 0 
        % find percentage overlap between (rectangular) AEs and their closest template AE
        total_overlap = 0;
        for ll = 1:num_ld

            % find overlapping points
            start_t1 = tsAE1_pixels(loc_dist(ll));
            start_t2 = tsAE2_pixels(ll);
            end_t1 = tsAE1_pixels(loc_dist(ll)) + (tcAE1_pixels(loc_dist(ll))-tsAE1_pixels(loc_dist(ll)))*2;
            end_t2 = tsAE2_pixels(ll) + (tcAE2_pixels(ll)-tsAE2_pixels(ll))*2;
            start_f1 = fsAE1_pixels(loc_dist(ll));
            start_f2 = fsAE2_pixels(ll);
            end_f1 = fsAE1_pixels(loc_dist(ll)) + (fcAE1_pixels(loc_dist(ll))-fsAE1_pixels(loc_dist(ll)))*2;
            end_f2 = fsAE2_pixels(ll) + (fcAE2_pixels(ll)-fsAE2_pixels(ll))*2;

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
        keep_overlap(ng) = total_overlap;

        disp(total_overlap);
    end
%     pause
end
keep_overlap(isnan(keep_overlap)) = 0;
% store total_overlap
if length(keep_overlap)>0
    xlswrite(xlsfile, keep_overlap, strcat('G',num2str(2),':G',num2str(num_g+1)))
end
