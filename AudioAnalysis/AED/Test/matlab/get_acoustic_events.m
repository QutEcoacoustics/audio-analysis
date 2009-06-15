function [AE,L] = get_acoustic_events(I1)
% Returns rectangular boundaries of acoustic events in black and white
% image I1
%
% Sensor Networks Project
% Birgit Planitz
% 20090310 

AE = [];
L = [];

[B,L] = bwboundaries(I1,'noholes');
numB = length(B);
% stats = regionprops(L, 'Centroid');




for bb=1:numB
    
    thisB = B{bb,1};
    thisrows = thisB(:,1);
    thiscols = thisB(:,2);
    AE(1,bb) = min(thiscols); %top
    AE(2,bb) = min(thisrows); %bot
    AE(3,bb) = max(thiscols)-min(thiscols)+1; %height
    AE(4,bb) = max(thisrows)-min(thisrows)+1; % width
    AE(5,bb) = AE(1,bb) + (AE(3,bb)-1)/2; %col centroid
    AE(6,bb) = AE(2,bb) + (AE(4,bb)-1)/2; %row centroid
    
end
    
