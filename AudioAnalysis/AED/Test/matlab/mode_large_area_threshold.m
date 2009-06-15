function large_area_thresh = mode_large_area_threshold(AE_old)
% Find cut-off threshold to separate large areas into smaller ones
% Uses mode
%
% Sensor Networks Project
% Birgit Planitz
% 20090407

large_area_thresh = 0;
smallest_thresh = 3000; % maximum cut-off point for area size
areas = AE_old(3,:).*AE_old(4,:);
areas2 = areas;
numbins = [0:1000:10000]; %bin centroids

if length(numbins)>1
    [n,loc] = hist(areas,numbins);
%     figure(11), clf, bar(loc,n)

    % run along loc (frequency of area size bins) from right to left to find right-most minimum
    tmp = n(1:end-1)-n(2:end);
    % reverse order of tmp
    lentmp = length(tmp);
    tmp2 = zeros(lentmp);
    loc2 = zeros(lentmp);
    for tt=1:length(tmp)
        tmp2(lentmp-tt+1) = tmp(tt);
        loc2(lentmp-tt+1) = loc(tt);
    end
    
    tmploc2 = find(tmp2>0);
    if (~isempty(tmploc2) & (tmploc2>1))
        large_area_thresh = loc2(tmploc2(1)-1); %first minimum in reversed histogram
    end
    
end

large_area_thresh(large_area_thresh<smallest_thresh) = smallest_thresh;



