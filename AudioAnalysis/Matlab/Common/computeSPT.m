function [peaksI3,peaksI3_h,peaksI3_v] = computeSPT(I1,F,T,peaks_int_thresh)
% compute and return spectrogram peaktracks
% 
% IT TAKES ABOUT 60 sec TO COMPUTE PEAK TRACKS FOR A 1-min recording
% using int_thresh = 9dB
%
% bmp 20091103



% PARAMETERS
peaks_dist_thresh = 2;
smooth_par = 3;
ctmp = colormap(gray); c = flipud(ctmp); % for greyscale plotting




%%% IMAGE PROCESSING %%%
% wiener filtering 
w = 5; %window length of wxw window used in wiener filtering
I2 = wiener2(I1, [w w]);


% remove subband noise
I3 = withoutSubbandModeIntensities(I2);
[M,N] = size(I3);
% showImage(c,I3,T,F,1)
% csvout('I3.csv', I3)


%%% COMPUTING PEAK TRACKS ON I3 %%%
% look for peaks in x direction
I3c = zeros(M,N);
for ii=1:N
    
    % do a smooth 
    vec_M = smooth(I3(:,ii),smooth_par);
    
    % find peaks
    [pks_M,locs_M] = findpeaks(vec_M);
    
    % keep peaks that are greater than peaks_int_thresh
    I3c(locs_M(pks_M>=peaks_int_thresh),ii) = pks_M(pks_M>=peaks_int_thresh);
    
end
% showImage(c,I3c,T,F,2)
% csvout('I3c.csv', I3c)

peaksI3_h = zeros(size(I3)); % init
for ii=1:N-1
    
    tmp = I3c(:,ii);
    pks_l = tmp(tmp>0);
    locs_l = find(tmp>0);
    tmp = I3c(:,ii+1);
    pks_r = tmp(tmp>0);
    locs_r = find(tmp>0);
    
    % either start a new track or continue an existing one
    % find a close neighbouring right-hand peak, if unavailable, discard
    for pp=1:length(pks_l)
        
        % find largest neighbouring peak in preset freq range
        diff_locs = abs(locs_l(pp)-locs_r);
        locs_keep1 = find(diff_locs<=peaks_dist_thresh);
        if ~isempty(locs_keep1)
            [tmp,locs_keep2] = max(pks_r(locs_keep1));
            peaksI3_h(locs_l(pp),ii) = peaksI3_h(locs_l(pp),ii) + 1;
            peaksI3_h(locs_r(locs_keep2),ii+1) = peaksI3_h(locs_r(locs_keep2),ii+1) + 1;
        end
        
    end
    
%     pause
end
peaksI3_h(peaksI3_h>1)=1;
% showImage(c,peaksI3_h,T,F,3)


% look for peaks in y direction
I3r = zeros(M,N);
for jj=1:M
    
    % do a smooth 
    vec_N = smooth(I3(jj,:),smooth_par);
    
    % find peaks
    [pks_N,locs_N] = findpeaks(vec_N);
    
    % keep peaks that are greater than peaks_int_thresh
    I3r(jj,locs_N(pks_N>=peaks_int_thresh)) = pks_N(pks_N>=peaks_int_thresh);
    
end

peaksI3_v = zeros(size(I3)); % init
for jj=1:M-1
    
    tmp = I3r(jj,:);
    pks_b = tmp(tmp>0);
    locs_b = find(tmp>0);
    tmp = I3r(jj+1,:);
    pks_t = tmp(tmp>0);
    locs_t = find(tmp>0);
    
    % either start a new track or continue an existing one
    % find a close neighbouring right-hand peak, if unavailable, discard
    for pp=1:length(pks_b)
        
        % find largest neighbouring peak in preset freq range
        diff_bocs = abs(locs_b(pp)-locs_t);
        locs_keep1 = find(diff_bocs<=peaks_dist_thresh);
        if ~isempty(locs_keep1)
            [tmp,locs_keep2] = max(pks_t(locs_keep1));
            peaksI3_v(jj,locs_b(pp)) = peaksI3_v(jj,locs_b(pp)) + 1;
            peaksI3_v(jj+1,locs_t(locs_keep2)) = peaksI3_v(jj+1,locs_t(locs_keep2)) + 1;
        end
        
    end
    
%     pause
end
peaksI3_v(peaksI3_v>1)=1;
% showImage(c,peaksI3_v,T,F,4)


% combine horizontal and vertical peak tracks
peaksI3 = peaksI3_h + peaksI3_v;
peaksI3(peaksI3>1)=1;


% dilate area around peaks and fill tracks and neighbouring regions with I3 image data
dilateI3 = peaksI3;
se = strel('square',3);
dilateI3 = imdilate(dilateI3,se);

% discard small tracks
L = bwlabel(peaksI3,8);
small_track_thresh = 15; %minimum track length
for ll=1:max(L(:))
    [rows,cols] = find(L==ll);
    if length(rows) < small_track_thresh
        for rr=1:length(rows)
            peaksI3(rows(rr),cols(rr)) = 0;
        end
    end
%     pause
end


peaksI3(dilateI3==1) = I3(dilateI3==1);
peaksI3(peaksI3<peaks_int_thresh) = 0;
% showImage(c,peaksI3,T,F,5)
end

function csvout(name, M)
    dlmwrite(name, M, 'precision', 8)
end
