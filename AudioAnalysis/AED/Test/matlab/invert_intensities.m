function I2 = invert_intensities(I1)
% Inverts intensity values of image; black->white and white->black
%
% Sensor Networks Project
% Birgit Planitz
% 20090309


maxI = max(I1(:));
I2 = maxI - I1;
