function [ osc_rate ] = osc_spectrogram( track, I, frame_second )

    % oscillation calculation based on spectrogram
    [M,N] = size(I);
    
    % dct duration:0.2s
    dct_length = round(0.2 * frame_second);
 
    % calculate the bin
    d_bin = round(mean(track{1}.arrayp)) ;

    low_bin = max(d_bin - 5,1);
    high_bin = min(d_bin + 5,M);
    
    power = zeros(1,N);
    for n =1:N
        power(n) = sum(I(low_bin:high_bin,n).^2);
    end
        
    n_power = (power - min(power))/ (max(power) - min(power));
    start = round(length(n_power) / 5);
    stop = round(4 * length(n_power) / 5);
    [c_power,~] = autocorr(n_power(start:stop),stop-start);
    c_power = c_power - mean(c_power);   
    
    dct_power =  abs(dct(c_power,dct_length));   
    nc_power = (dct_power - min(dct_power))/range(dct_power);
    nc_power(1:5) = 0;
    [~,loc] = find(nc_power == max(nc_power));
    interval = 1 / (loc / dct_length * 0.5); % frame
    osc_rate = 1 / (interval / frame_second);

end

