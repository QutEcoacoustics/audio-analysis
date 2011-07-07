function [Q8,Q11,freqBin,FrameZero]=framenumber(w3,w4,F,L5)
%Q8: frame number
%w3:signal
%w4:frequency
%L5: X

%calculate the frame number of signal
Q8=zeros(1,L5);
Q9=isnan(w3);
Q10=Q9';
freqBin=zeros(1,L5);
for k=1:L5
    if Q10(k)==0
       Q8(k)=k;
       Q11(k)=w4(k);
       %store the frequency index according to F
       for index=1:(size(F)-1)
           if w4(k)==F(index)
               freqBin(k)=index;
           elseif w4(k)==F(index+1)
               freqBin(k)=index+1;
           elseif (w4(k)>F(index))&&(w4(k)<F(index+1))
               freqBin(k)=index;           
           end
       end
    else
        Q8(k)=NaN;
        Q11(k)=NaN;
    end     
end
FrameZero=Q8;
for k=1:L5
    if isnan(Q8(k))==1
        FrameZero(k)=0;
    end
end

end