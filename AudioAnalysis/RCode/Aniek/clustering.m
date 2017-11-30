%open csv file
M = csvread('/Volumes/Nifty/QUT/60clusters/Fastafiles/matlab_Gympie_numbers_midnight.txt');
%calculate the counts of the 60 clusters per row/day
counts = hist(M', 1:60);
counts = counts';
%calculate the total counts of the 60 clusters
counts_tot = sum(counts);
%normalize the counts per day with the total counts
counts_nor = bsxfun(@rdivide, counts, counts_tot);

%cluster the days using k-means clustering trying k of size 1-50
clust = zeros(size(counts_nor,1),50);
for i=1:50
    rng();
    clust(:,i) = kmeans(counts_nor,i,'replicate',5);
end

clust_cor = zeros(size(counts_nor,1),50);
for i=1:50
    rng();
    clust_cor(:,i) = kmeans(counts_nor,i,'replicate',5,'Distance','correlation');
end
evaCH = evalclusters(counts_nor,clust,'CalinskiHarabasz');
evaDB = evalclusters(counts_nor,clust, 'DaviesBouldin');
evaSH = evalclusters(counts_nor,clust,'Silhouette');
%% Create gridplot of resulting group assignment

x = [1:10];
y = [1:398];
[X,Y] = meshgrid(x,y);
Z = clust(:,1:10);
Z_cor = clust_cor(:,1:10);

figure
%use the user defined colormap for figure.
colormap(lines(10));
%plot the figure
pcolor(X,Y,Z);
%ax = gca;
%startDate = datenum('06-22-2015');
%endDate = datenum('07-23-2016');
%yData = linspace(startDate,endDate,26);
%ax.YTick = 1:10:398;
%datetick(ax,'y',1,'keepticks');

%formatSpec = '%{yyyy-MM-dd}D%f';
%dates = table2cell(readtable('/Volumes/Nifty/QUT/datelist.txt', 'Delimiter', ' ','Format',formatSpec,'ReadVariableNames',false));
%ax.YTickLabel = dates(1:10:398,1);

xlabel('K clusters');
ylabel('Day number');
colorbar('eastoutside');

figure
colormap(lines(10));
pcolor(X,Y,Z_cor);
xlabel('K clusters');
ylabel('Day number');
title('Correlation distance');
colorbar('eastoutside');