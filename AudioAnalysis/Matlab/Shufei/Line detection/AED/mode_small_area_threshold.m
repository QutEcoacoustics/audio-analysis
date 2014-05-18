function [AE_new] = mode_small_area_threshold(AE_old, largest_thresh)
% Find cut-off threshold to remove small areas from acoustic event pool
% Uses mode
%
% Sensor Networks Project
% Birgit Planitz
% 20090304

AE_new = [];

areas = AE_old(3,:).*AE_old(4,:);
areas2 = areas;
areas2(areas>largest_thresh) = nan;

numbins=[0:9] * round(largest_thresh*0.1) + round(largest_thresh*0.05); %bin centroids
[n,loc] = hist(areas2,numbins);
% run along loc (frequency of area size bins) to find first minimum
tmp = n(1:end-1)-n(2:end);
tmploc = find(tmp<0);
if ~isempty(tmploc)
    small_area_thresh = loc(tmploc(1)); %first minimum in histogram
else
    tmploc = find(tmp==0);
    if ~isempty(tmploc)
        small_area_thresh = loc(tmploc(1));
    else
        small_area_thresh = largest_thresh;
    end
end

numB = size(AE_old,2);
for bb=1:numB
    if (areas(bb) > small_area_thresh)
        AE_new = [AE_new AE_old(:,bb)];
    end
end
