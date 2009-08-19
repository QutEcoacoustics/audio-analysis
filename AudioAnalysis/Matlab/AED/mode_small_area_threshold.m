function [AE_new,small_area_thresh] = mode_small_area_threshold(AE_old)
% Find cut-off threshold to remove small areas from acoustic event pool
% Uses mode
%
% Sensor Networks Project
% Birgit Planitz
% 20090304

AE_new = [];

largest_thresh = 200; % maximum cut-off point for area size

areas = AE_old(3,:).*AE_old(4,:);
areas2 = areas;
areas2(areas>largest_thresh) = nan;

numbins=[0:9] * round(largest_thresh*0.1) + round(largest_thresh*0.05); %bin centroids
[n,loc] = hist(areas2,numbins);
% figure(11), clf, bar(loc,n)
% set(gca,'FontSize',30), axis tight
% title('Histogram of acoustic event sizes','FontSize',30)
% ylabel('Number of events','FontSize',30)
% xlabel('Size','FontSize',30)
% pause
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
