function [dB,ZeroCrossing,Std,fz,X]=ExtractfeaturesInTimeDomain(window,S,L,fs)

% hwin=window/2;
% PdB=zeros(1,L);
% Pa1=zeros(1,L);
% PAvg=zeros(1,L);
% PStd=zeros(1,L);

X=(round(L/window)-1);%calculate the total window number of the recording 
for q=1:X,n=0:window:(L-window);%aim to store the zero crossing number of each window 
      P1=S((1+n(q)):(window+n(q)));%withdraw samples of each window.
                                  % P  when without high-pass filter
                                  % P1  when with high-pass filter
      
    %Apply the high pass filter
      P=zero_mean(P1,window);
      ZeroCount=0;
      k=1;
      Sum2=0;
      for m=1:(window-1)
          if P(m)~=0
            Sum2=Sum2+log10(abs(P(m)));
          end
      end
     dB(q)=20*Sum2/window;
%       PdB(n(q)+hwin)=20*Sum2/window;
      prevPvalue=P(1);
      for m=2:window
         if prevPvalue*P(m)<0 %calculate the zero crossing numbers
             ZeroCount=ZeroCount+1;% to store the zero crossing number 
             prevPvalue=P(m);
             num(k)=m;% to store the No. of previous zero point
            k=k+1;                         
          end         
      end
      ZeroCrossing(q)=ZeroCount;% a1 stores the zero crossing mumber
      fz(q)=(fs*ZeroCrossing(q))/(window*2);
%       Pa1(n(q)+hwin)=a;
%       Pfz(n(q)+hwin)=fz(q);
        % to calculate the sample numbers between consecutive zero points
      l=1;
      Sum=0;
      Sum1=0;
      for kk=1:(k-2)
          SamN(l)=num(kk+1)-num(kk);%SamN stores the numbers between consecutive zero points
          l=l+1;
      end
      for k4=1:(l-1)
          Sum=Sum+SamN(k4);
      end
      Avg(q)=Sum/(l-1);
%       PAvg(n(q)+hwin)=Sum/(l-1);
      for k4=1:(l-1)
          Sum1=Sum1+(SamN(k4)-Avg(q))^2;
      end
      %Calculate the Std(standard deviation)
      Std(q)=sqrt(Sum1/ZeroCrossing(q));%Std stores the Std  
%       PStd(n(q)+hwin)=sqrt(Sum1/a1(q));              
end

% csvwrite('dB.csv',dB');
% csvwrite('a1.csv',a1');
% csvwrite('Std.csv',Std');
% csvwrite('fz.csv',fz');


end