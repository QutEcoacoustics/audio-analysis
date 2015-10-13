function [ AE4 ] = AEDJie( specClean, T, F )

% wiener filtering
[specWie,~] = wiener2(specClean, [7 7]);

% adaptive thresholding
specTrs = otsu(specWie, 2);

% opening and closing of the picture
SE1 = strel('rectangle', [7 5]);
specOpen = imopen(specTrs, SE1);

SE2 = strel('square', 5);
specClose = imclose(specOpen, SE2);

specHole = imfill(specClose, 'holes');
specHole = 1 - specHole;

% connect the spectrogram
gSize = 3;
G = fspecial('gaussian',[gSize gSize], 2);
specSmooth=conv2(specHole, G, 'same');  % smooth image by Gaussiin convolution

[B, L] = bwboundaries(specSmooth,'noholes');
 
% remove small objects that are too small

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

% segment large events into small events
[newAE] = separate_large_AEs_areas(AE, L, 3000, specHole);

% remove small events
smallAreaThresh = 500;
AE3 = mode_small_area_threshold(newAE, smallAreaThresh);

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

