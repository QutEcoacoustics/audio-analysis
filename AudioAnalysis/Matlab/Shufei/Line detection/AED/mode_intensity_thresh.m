function int_thresh = mode_intensity_thresh(I1)
% Computes intensity threshold using mode
%
% Sensor Networks Project
% Birgit Planitz
% 20090408

[M,N] = size(I1);

% plotting - to see how intensity values of image behave
% [n,locs] = hist(double(I1(:)),[0:(M-1)]);
% figure(1), clf, subplot(311), bar(locs,n), axis tight
% I1tmp = I1;
% I1tmp(I1==0) = nan;
% [n,locs] = hist(double(I1tmp(:)),[0:(M-1)]);
% subplot(312), bar(locs,n), axis tight


testdata_tmp = I1(:);
% cull zeros
testdata = testdata_tmp(testdata_tmp>0);
% mirror in negative 
testdata = [testdata; -testdata];
% add some zeros to create normal distribution
len1 = length(find(testdata==1));
testdata = [testdata; zeros(len1,1)];
[n,locs] = hist(double(testdata),[-(M-1):(M-1)]);
% subplot(313), bar(locs,n), axis tight
[muhat,sigmahat] = normfit(double(testdata));
int_thresh = sigmahat;

