function I2 = image_thresh_bw(I1,int_thresh)
% Converts image to black and white using intensity threshold
%
% Sensor Networks Project
% Birgit Planitz
% 20090310 


 
I2 = zeros(size(I1)); % init
I2(I1>=int_thresh) = 1;




