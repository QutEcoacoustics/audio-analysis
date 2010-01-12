function writeTemplateTxtFile
% bmp 20100112

load('GParrots_JB2_20090607-173000.wav_minute_3.wav_Intensity_Thresh_3dB_Small_area_thresh_max_100_template72.mat')

AE = template.AE;
AE(:,2) = AE(:,1) + AE(:,2);
fid = fopen('gpTemplate.txt', 'wt');
fprintf(fid, '%2.6f\t%2.6f\t%4.6f\t%4.6f\n', AE');
fclose(fid)
