function [ AE4 ] = AEDLasseck( specClean, T, F )
% binary closing
% dilation
% median filtering
% remove of small objects

% closing binary
SE2 = strel('square', 5);
specClose = imclose(specClean, SE2);

% dilation
SE3 = strel('rectangle', [7 5]);
specDie = imdilate(specClose, SE3);

% median filter
specMed = medfilt2(specDie, [7 7]);

% remove small objects that are too small
[B, ~] = bwboundaries(specMed,'noholes');

AE = [];
numB = length(B);
for bb = 1:numB
    
    thisB = B{bb, 1};
    thisrows = thisB(:,1);
    thiscols = thisB(:,2);
    AE(1,bb) = min(thiscols); %left
    AE(2,bb) = min(thisrows); %bottom
    AE(3,bb) = max(thiscols)-min(thiscols)+1; %width
    AE(4,bb) = max(thisrows)-min(thisrows)+1; % height

end

smallAreaThresh = 500;
AE3 = mode_small_area_threshold(AE, smallAreaThresh);

AE4 = [];
if (~isempty(AE3))
    [rAE,~] = size(AE3');
    AE4 = zeros(rAE,4);
    AE4(:,1) = T([AE3(1,:)]);
    AE4(:,2) = T([AE3(3,:)]);
    AE4(:,3) = F([AE3(2,:)]);
    AE4(:,4) = F([AE3(2,:) + AE3(4,:) - 1]);
end

end

