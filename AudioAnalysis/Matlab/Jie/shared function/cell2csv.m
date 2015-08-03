function cell2csv(filename,cellArray,delimiter,mode) 
% Writes cell array content into a *.csv file. 
% 
% CELL2CSV(filename,cellArray,delimiter) 
% 
% filename = Name of the file to save. [ i.e. 'text.csv' ] 
% cellarray = Name of the Cell Array where the data is in 
% delimiter = seperating sign, normally:',' (it's default) 
% mode = specifies the mode of opening the file. See fopen() for a detailed 
% list (default is overwrite i.e. 'w') 
% 
% by Sylvain Fiedler, KA, 2004 
% modified by Rob Kohr, Rutgers, 2005 - changed to english and fixed delimiter 
if nargin<3 
delimiter = ','; 
end 
if nargin<4 
mode = 'w'; 
end

datei = fopen(filename,mode); 
for z=1:size(cellArray,1) 
for s=1:size(cellArray,2) 

var = eval(['cellArray{z,s}']); 

if size(var,1) == 0 
var = ''; 
end 

if isnumeric(var) == 1 
var = num2str(var); 
end 

fprintf(datei,var); 

if s ~= size(cellArray,2) 
fprintf(datei,[delimiter]); 
end 
end 
fprintf(datei,'\n'); 
end 
fclose(datei);