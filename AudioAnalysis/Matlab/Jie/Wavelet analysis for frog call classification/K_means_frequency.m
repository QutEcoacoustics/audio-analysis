% clustering the dominant frequency results into 10 centorids which can be
% used to generate the scale for WPD

% read the dominant frequency information

clc; close all; clear;

csvFolder = '.\07-22\syllableFeature\*.csv';
csvFolderName = '.\07-22\syllableFeature\fileName';

csvList = dir(csvFolder);
% remove hidden files
csvList = csvList(arrayfun(@(x) ~strcmp(x.name(1), '.'), csvList));

finalDF = 0;
for iCSV= 1:length(csvList) 
    
    frogName = csvList(iCSV);
    frogCsvPath = strrep(csvFolderName, 'fileName', frogName.name);
    
    % read the csv file
    fid = fopen(frogCsvPath);
    out = textscan(fid, '%s %s %s %s %s %s %s', 'delimiter', ',');
    fclose(fid);
    
    dominantFreq = out{1};
    dominantFreq(1) = [];
    dominantFreq = str2double(dominantFreq);
    
    % connect DF of different frog species together
    finalDF = [finalDF; dominantFreq];
    
end

hist(finalDF)
xlabel('Dominant frequency');
ylabel('No. of syllables');
export_fig '.\07-22\dfHist.png'

finalDF(1) = [];
finalDF(finalDF ==0) = [];
% apply k-means to the DF
[idx,C] = kmeans(finalDF, 10, 'Distance', 'cityblock', 'Replicates', 10);

% save the centroids of k-means result
csvwrite('.\07-22\centroids.csv', sort(C));












