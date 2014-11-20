function [ ] = pixel_spectrogram(I)
%PIXEL_SPECTROGRAM Summary of this function goes here
%   Detailed explanation goes here
    I1 = (I - min(I(:))) / (max(I(:)) - min(I(:)));
    I2 = floor(255 * I1);
    I2 = flipud(I2);
    imwrite((255 - I2),'.\spectrogram.png');
end

