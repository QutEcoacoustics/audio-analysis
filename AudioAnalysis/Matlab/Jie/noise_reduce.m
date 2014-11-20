% Reduce noise of the spectrogram
function I3 = noise_reduce(I1)

[M,N] = size(I1);
% Calculate the noise profile for each frequency band
noiseBase = rep(0,M);

pitch = round(N/4);
 
if pitch > 300
    pitch = 300;
end

for nf = 1:M
    thisI = I1(nf,:);
    thisI(isinf(thisI)) = nan;
      
    [histI,mid] = hist(thisI,pitch);
    % Smooth histI
    counts = smooth(histI);
    [~,index] = max(counts);
    
    if index > pitch*0.95
        bandNoise = mid(round(0.95*pitch));
    else
        bandNoise = mid(index);                
    end    
    noiseBase(nf) = bandNoise;
end

% Smooth noiseBase
smoothNoiseBase = smooth(noiseBase);

% Remove noiseBase
I2 = zeros(M,N);
for n = 1:M 
    I2(n,:) = I1(n,:) - smoothNoiseBase(n);
end

I2(I2<0) = 0;

% Remove neighbour noise
% setup a matrix for accelarating speed
bin = max(M/32 + 1 , 9);
frame = max(bin/3 , 3);

I3 = zeros(M,N);
for i = 0:(frame-1)
    for j = 0:(bin-1)       
        I2UD = shiftmat(I2,-j);
        I2Result = shiftmat(I2UD,-i,2);    
        I3 = I2Result + I3;
    end    
end

I3 = I3 / (frame*bin);
ThresholdI = min(I3(I3(:) > 0)) + 3;
I3(I3 < ThresholdI) = 0;

end