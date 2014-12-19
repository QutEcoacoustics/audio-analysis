function [ cEnergy,cEnergyThreshold ] = column_energy(I,lowBin,highBin)
% calculate the energy for each column
% return the threshold for energy segmentation
    N = size(I,2); 
    cEnergy = zeros(1,N);
    
    for c = 1:N       
        cEnergy(c) = sum(I(lowBin:highBin,c).^2);        
    end
    
    [counts,mids] = hist(cEnergy);
    sCounts = smooth(counts,3);
    [~,maxIndex] = max(sCounts);
    cEnergyThreshold = mids(maxIndex)*2*0.8;
 
end
% EOF