function [newAE] = separate_large_AEs_areas(oldAE,L,area_thresh,I1)
% Seperates acoustic events that are too large in area
%
% Sensor Networks Project
% Birgit Planitz
% 20090420 

[M,N] = size(I1);


% find large acoustic events
ind = find( (oldAE(3,:).*oldAE(4,:))>=area_thresh);
workingAE = oldAE(:,ind);
[tmp, numW] = size(workingAE);

newAE = []; % init
for cntr = 1:size(oldAE,2)
    if (isempty(find(ind==cntr)))
        newAE = [newAE oldAE(:,cntr)];
    end
end



freq_thresh = 20;
time_thresh = 1/3*100; % events must be longer than this (bandwidth)

for nw=1:numW
    
    thisI = zeros(M,N);
    thisI(L==ind(nw)) = 1;
        
    startM = workingAE(2,nw);
    endM = workingAE(2,nw) + workingAE(4,nw) - 1;
    startN = workingAE(1,nw);
    endN = workingAE(1,nw) + workingAE(3,nw) - 1;
    thisI = thisI(startM:endM,startN:endN);
    [tM,tN] = size(thisI);
    figure(28), clf, imshow(thisI)
    
    
    
    pause
end


