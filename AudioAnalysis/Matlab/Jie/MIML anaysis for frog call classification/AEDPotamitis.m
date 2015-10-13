function [ AE4 ] = AEDPotamitis(  specClean, T, F  )

% method 1
% parameter setting
% 1. Gaussian size
% 2. edge enhancement
% 3. binary threshold
% 4. elment size
% 5. small area size

% create gaussian filter
gSize = 7;
G = fspecial('gaussian',[gSize gSize], 2);
specSmooth=conv2(specClean, G, 'same');  % smooth image by Gaussiin convolution

% edge enhancement
% specEdge = edge(specSmooth, 'sobel'); 
% specEnhance = specEdge .* specSmooth;

specEnhance = imsharpen(specSmooth,'Radius',2,'Amount',1);

% opening and closing of the picture
SE1 = strel('rectangle', [7 5]);
specOpen = imopen(specEnhance, SE1);

SE2 = strel('square', 5);
specClose = imclose(specOpen, SE2);

% filling the holes 
specHole = imfill(specClose, 'holes');

% remove small objects that are too small
[B, ~] = bwboundaries(specHole,'noholes');

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

