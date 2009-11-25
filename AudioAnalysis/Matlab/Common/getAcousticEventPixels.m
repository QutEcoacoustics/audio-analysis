function I2 = getAcousticEventPixels(I1,AE,T,F,c)
% Returns I1 pixels that are inside acoustic event (AE) boundaries
% All other areas are zero
%
% bmp 20091123

[M,N] = size(I1);
I2 = zeros(M,N);
numAE = size(AE,1);

% showImage(c,I1,T,F,70,AE)

for aa=1:numAE
    
    [tmp,left] = min(abs(T-AE(aa,1)));
    [tmp,right] = min(abs(T-(AE(aa,1)+AE(aa,2))));
    [tmp,bottom] = min(abs(F-AE(aa,3)));
    [tmp,top] = min(abs(F-AE(aa,4)));
    
    I2(bottom:top,left:right) = I1(bottom:top,left:right);
    
end
% showImage(c,I2,T,F,71,AE)


