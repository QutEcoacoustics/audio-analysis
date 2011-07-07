function [AE,L] = get_acoustic_events(I1,Ia,Ib,Ic)
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
    AE(1,bb) = min(thiscols); %left
    AE(2,bb) = min(thisrows); %bottom
    AE(3,bb) = max(thiscols)-min(thiscols)+1; %width
    AE(4,bb) = max(thisrows)-min(thisrows)+1; % height
    
    thisIa = Ia(AE(2,bb):(AE(2,bb)+AE(4,bb)-1),AE(1,bb):(AE(1,bb)+AE(3,bb)-1));
    AE(5,bb) = mean(thisIa(:));
    AE(6,bb) = var(thisIa(:));
    thisIb = Ib(AE(2,bb):(AE(2,bb)+AE(4,bb)-1),AE(1,bb):(AE(1,bb)+AE(3,bb)-1));
    AE(7,bb) = mean(thisIb(:));
    AE(8,bb) = var(thisIb(:));
    thisIc = Ic(AE(2,bb):(AE(2,bb)+AE(4,bb)-1),AE(1,bb):(AE(1,bb)+AE(3,bb)-1));
    AE(9,bb) = mean(thisIc(:));
    AE(10,bb) = var(thisIc(:));
    
end
    
