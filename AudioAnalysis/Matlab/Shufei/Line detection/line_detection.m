function line_detection
% This code starts with a raw audio file; applies image processing
% and returns areas (four points of a rectangle) that are designated as
% acoustic events
%
% Sensor Networks Project
% Birgit Planitz
% 20090415

clear;
clc;
% PARAMETERS
w = 5; %window length of wxw window used in wiener filtering
int_thresh = 9; % intensity threshold
big_area_thresh = 3000;
small_area_thresh = 200; % maximum cut-off point for area size
ctmp = colormap(gray); c = flipud(ctmp); %colormap for plotting in grayscale

%Console.WriteLine('kjiykliyku');
% STEP 1: COMPUTE SPECTROGRAM
addpath('../Common')

[y,fs,I1,F,T,] = wavToSpectrogram('C:\SensorNetworks\WavFiles\3hours samford tagged dataset\single channel\2 mins\NEJB_NE465_20101013-070000\NEJB_NE465_20101013-071400.wav');

[M,N] = size(I1);
showImage(c,I1,T,F,1);
% csvout('I1.csv',I1)

% STEP 2: WIENER FILTERING
I2 = wiener2(I1, [w w]);
% csvout('I2.csv',I2)
showImage(c,I2,T,F,2);

% STEP 3: GET MODAL INTENSITIES OF SUBBBANDS and REMOVE FROM IMAGE
I3 = withoutSubbandModeIntensities(I2);
 %csvout('I3.csv',I3)

% STEP 4: CONVERTS IMAGE TO BLACK AND WHITE USING INTENSITY THRESHOLD
I4 = image_thresh_bw(I3,int_thresh);
% csvout('I4.csv',I4)

% STEP 5: JOIN VERTICAL LINES IN IMAGE
I5 = join_vertical_lines(I4);
% csvout('I5.csv',I5)

% STEP 6: JOIN HORIZONTAL LINES IN IMAGE
I6 = join_horizontal_lines(I5);
% csvout('I6.csv',I6)

% STEP 7: GET ACOUSTIC EVENTS
[AE,L] = get_acoustic_events(I6,I1,I2,I3);
% csvout('AE1.csv',AE)

% STEP 8: SEPARATE EVENTS THAT ARE TOO LARGE INTO SMALLER EVENTS
AE2 = [];
if (~isempty(AE)) % do this next step only if acoustic events have been detected
    AE2 = separate_large_AEs_areas(AE,L,big_area_thresh,I6,I1,I2,I3); % separate large area events into smaller events
end
% csvout('AE2.csv',AE2)

% STEP 9: KEEP LARGE EVENTS ONLY
AE3 = [];
if (~isempty(AE2))
    AE3 = mode_small_area_threshold(AE2, small_area_thresh); % compute small area threshold for culling acoustic events & cull small events
end
% csvout('AE3.csv',AE3)
showImage(c,I3,T,F,3);
set(gca, 'position', [0 0 1 1], 'visible', 'off');% really cool! save the image without the boundary
saveas(gcf, 'spectrogram', 'jpg');
Y=imread('spectrogram.jpg');
Y1=rgb2gray(Y);



  fltr4img = [1 2 3 2 1; 2 3 4 3 2; 3 4 6 4 3; 2 3 4 3 2; 1 2 3 2 1];
  fltr4img = fltr4img / sum(fltr4img(:));
  imgfltrd = filter2( fltr4img ,Y1 );
  tic;
  [accum, axis_rho, axis_theta,lineprm, lineseg] = ...
      Hough_Grd(imgfltrd, 3, 0.02);
  toc;
  figure(4); imagesc(axis_theta*(180/pi), axis_rho, accum); colormap('gray'); axis xy;
  xlabel('Theta (degree)'); ylabel('Pho (pixels)');
  title('Accumulation Array from Hough Transform');
saveas(gcf,'Accumulation Array','jpg');
 figure(5); imagesc(Y1); colormap('gray'); axis image;
  DrawLines_2Ends(lineseg);
  title('Raw Image with Line Segments Detected');
 saveas(gcf,'Hough-transform','jpg');
 
 %Canny Edge Detector
BW=edge(Y1,'canny',0.3);
figure(6);
imshow(BW);
saveas(gcf,'Canny-edge','jpg');

end



