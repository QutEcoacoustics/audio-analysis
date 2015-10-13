% reduce noise based on spectral subtraction
% input: original spectrogram
function I3 = noise_reduce_jie(I1,~)

[M,N] = size(I1);
% calculate the noise profile for each frequency bin
noise_base = rep(0,M);
pitch = round(N / 8);

for nf = 1:M
    thisI = I1(nf,:);
    thisI(isinf(thisI)) = nan;    
    [histI,mid] = hist(thisI,pitch);
    % smooth histI
    counts = smooth(histI);
    [~,index] = max(counts);   
    if index > pitch*0.95
        bandNoise = mid(round(0.95*pitch));
    else
        bandNoise = mid(index);                
    end    
    noise_base(nf) = bandNoise;   
end
% smooth noiseBase
smoothNoiseBase = smooth(noise_base,5);
% remove noiseBase
I2 = zeros(M,N);
for n = 1:M 
    I2(n,:) = I1(n,:) - smoothNoiseBase(n);
end
I2(I2<0) = 0;
% remove neighbour noise
% setup a matrix for accelarating speed
bin = max(M/32 + 1 , 3);
frame = max(bin/3 , 3);
I3 = zeros(M,N);
for i = 0:(frame-1)
    for j = 0:(bin-1)       
        upDownI2 = shiftmat(I2,-j);
        resultI2= shiftmat(upDownI2,-i,2);    
        I3 = resultI2 + I3;
    end    
end

I3 = I3 / (frame*bin);
thresholdI = min(I3(I3(:) > 0)) + 3;
I3(I3 < thresholdI) = 0;

end
