function [h1,S]=ChebyshevFilter(N,R,fstop1,fstop2,fsample,Y)

wp=[fstop1/fsample fstop2/fsample];
[Z,P,K]=cheby1(N,R,wp,'bandpass');
[sos,g]=zp2sos(Z,P,K);
h1=dfilt.df2sos(sos,g);
y= filter(h1, Y);S=y;

end