% plot coefficients

pathMFCC = '.\07-10\MFCC\Litoria nasuta - testingMFCC.csv';
pathMel = '.\07-04\original\MelCC\Litoria nasutaMelCC.csv';
pathPWSCC = '.\07-10\PWSCC\Litoria nasuta - testingPWSCC.csv';

%%
fid = fopen(pathMFCC,'rt');
out = textscan(fid, '%s %s %s %s %s %s %s %s %s %s %s %s %s', 'delimiter', ',');
fclose(fid);

len = length(out{1});
MFCCFeature = zeros(len - 1, 12);

for i = 1: 12
    temp = str2double(out{i}(2: len));
    MFCCFeature(:, i) = (temp - min(temp)) / range(temp);
end

plot(MFCCFeature')
export_fig '.\07-10\MFCC.png'

%%
fid = fopen(pathMel,'rt');
out = textscan(fid, '%s %s %s %s %s %s %s %s %s %s %s %s %s', 'delimiter', ',');
fclose(fid);

len = length(out{1});
MelFeature = zeros(len - 1, 12);

for i = 1: 12
    temp = str2double(out{i}(2: len));
    MelFeature(:, i) = (temp - min(temp)) / range(temp);
end

plot(MelFeature')
export_fig '.\07-04\Mel.png'


%%
fid = fopen(pathPWSCC,'rt');
out = textscan(fid, '%s %s %s %s %s %s %s %s %s %s %s %s %s', 'delimiter', ',');
fclose(fid);

len = length(out{1});
PWSCCFeature = zeros(len - 1, 12);

for i = 1: 12
    temp = str2double(out{i}(2: len));
    PWSCCFeature(:, i) = (temp - min(temp)) / range(temp);
end

plot(PWSCCFeature')
export_fig '.\07-10\PWSCC.png'


