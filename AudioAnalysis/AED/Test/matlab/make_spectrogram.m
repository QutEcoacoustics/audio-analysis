function make_spectrogram
% Sensor Network Project
% Birgit Planitz
% 20090409

txt = 'Honeymoon Bay - St Bees_20081120-183000'; % test file
% txt = '20090317-143000[1]';
% txt = '20090319-070105[1]';
% txt = '20090320-070105[1]';
% txt = '20090317-173000[1]'

% read audio data
[y, fs, nbits, opts] = wavread(strcat(txt,'.wav'));
% figure(1), plot(y)
T = length(y)/fs; %length of signal in seconds



% generate spectrogram
window = 512; % hamming window using 512 samples
noverlap = round(0.5*window); % 50% overlap between frames
nfft = 256*2-1; % yield 512 frequency bins
[S,F,T,P] = spectrogram(y,window,noverlap,nfft,fs);
% figure(2), clf, surf(T,F,10*log10(abs(P)),'EdgeColor','none');
% axis xy; axis tight; colormap(jet); view(0,90);
% xlabel('Time (s)');
% ylabel('Frequency (Hz)');


% convert amplitude to dB
%A = -10*log10(abs(P));
A = 10*log10(abs(P));
minA = floor(min(A(:)));
maxA = ceil(max(A(:)));
[n,locs] = hist(A(:),[minA:maxA]);
figure(1), clf, bar(locs,n)


% % shift modal value to be 0 dB
% modal = locs(n==max(n));
% A2 = A-modal;
% minA2 = floor(min(A2(:)));
% maxA2 = ceil(max(A2(:)));
% [n2,locs2] = hist(A2(:),[minA2:maxA2]);
% figure(2), clf, bar(locs2,n2)


figure(3), clf, surf(T,F,A,'EdgeColor','none');
axis xy; axis tight; colormap(gray); view(0,90);
xlabel('Time (s)');
ylabel('Frequency (Hz)');
colorbar

% wiener filtering
w = 5;
A2 = wiener2(A, [w w]);
figure(4), clf, surf(T,F,A2,'EdgeColor','none');
axis xy; axis tight; colormap(gray); view(0,90);
xlabel('Time (s)');
ylabel('Frequency (Hz)');
colorbar

return
% convert spectrogram to grayscale image; normalise so that lowest A value
% corresponds with '0' and highest corresponds with '1'
I1 = mat2gray(A,[min(A(:)) max(A(:))]);
[M,N] = size(I1);
% figure(3), clf, imshow(I1)



% flip image about x axis
I2 = zeros(size(I1));
for ii=1:M/2
    I2(ii,:) = I1(M-ii+1,:);
    I2(M-ii+1,:) = I1(ii,:);
end
% figure(4), clf, imshow(I2)
imwrite(I2,strcat(txt,'.bmp'),'bmp');