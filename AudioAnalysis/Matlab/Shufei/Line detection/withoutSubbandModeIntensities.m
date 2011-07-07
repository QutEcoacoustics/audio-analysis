function I2 = withoutSubbandModeIntensities(I1)
% Returns image I2 with modal intensity removed from each subband in a 
% spectrogram
% Spectrogram must have 256 pixels in frequency (y) axis
% 0->11kHz
%
% Sensor Networks Project
% Birgit Planitz
% 20090417 

[M,N] = size(I1);
I2 = zeros(M,N);
mode1 = zeros(1,M);

for nf = 1:M  
    thisI = I1(nf,:);
    
    thisI(isinf(thisI)) = nan; % need to check for inf - 20100218
    maxI = max(thisI(:));
    minI = min(thisI(:));
    
    threshI = (minI-maxI)/2;
    
    hvec = [min(thisI(:)):max(thisI(:))];

    histI = hist(thisI(:),hvec);
    [tmp,loc] = max(histI(:));
    mode1_tmp = hvec(loc(1));
    
    mode1_tmp(mode1_tmp>threshI) = threshI;
    
    mode1(nf) = mode1_tmp;
end

mode2 = smooth(mode1,11);

for nf = 1:M
    I2(nf,:) = I1(nf,:)-mode2(nf);
end

