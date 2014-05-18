function [Q4,oneStd,b]=noiseremoval(w,L4,o)

%Calculate the Min & Max in the RdB array
Min=min(w);
Max=max(w);

%set up the histogram
binCount=100; 
binWidth=(Max-Min)/binCount; %histogram bins to cover full range of dB values
histo=zeros(binCount);%number of histograms

%calculate the bin ID & draw the histogram
for k=1:L4
    id=fix((w(k)-Min)/binWidth);% fix() È¡Õûº¯Êý
    if id>=binCount
        id=binCount-1;
    elseif id<=0
        id=1;
    end
    histo(id)=histo(id)+1; %draw the histogram
end
% figure(3);bar(1:binCount, histo,'b');% draw the histogram
% title('Histogram of dB','fontsize',15);
% ylabel('Frame numbers falling into each bin','fontsize',15);
% xlabel('Histogram bin','fontsize',15);
%smooth the histogtam
 smoothHisto=smooth_filter(histo,binCount,7);
% figure(2);bar(1:binCount, smoothHisto,'r');
% title('Histogram of dB','fontsize',15);
% ylabel('Frame numbers falling into each bin','fontsize',15);
% xlabel('Histogram bin','fontsize',15);

%find the index of the Max value of smoothHisto
Max2=max(smoothHisto);
for k2=1:binCount
    if smoothHisto(k2)==Max2
        peakID=k2;
    end
end

%Calculate Q:the "averaged" noise background level
Noiselevel=Min+((peakID+1)*binWidth);
%subtract modal noise and return array
%return back the Nstd value TO Q4
l=0;
l1=0;
Q6=w;
Q7=w;

for k3=1:L4
    Q4(k3)=w(k3)-Noiselevel;
    Q5(k3)=w(k3)-Noiselevel;
    if Q4(k3)>0 
      Q6(k3)=0;
      l=l+1;%Std
    end
    if Q5(k3)<0
      Q7(k3)=0;
      l1=l1+1;%dB
    end
end

        
%calculate the oneStd of background noise
Sum=0;
Sum1=0;%dB
ss=0;
ss1=0;
for k3=1:L4 
    if Q7(k3)~=0 
    Sum=Sum+(Q7(k3)-Noiselevel)^2;
    end
    if Q6(k3)~=0
    Sum1=Sum1+(Q6(k3)-Noiselevel)^2;%dB
    end
end
ss=Sum/l;  %Variance--Std
ss1=Sum1/l1;%dB

switch(o)
    case 1
        oneStd=sqrt(ss);% Std
    case 2
        oneStd=sqrt(ss1);%dB
end

b=Noiselevel;
switch (o)
    case 1
        for index=1:length(Q6)
            if Q6(index)==0
                Q6(index)=NaN;
            end
        end
        Q4=Q6;
    case 2
        for index=1:length(Q7)
            if Q7(index)==0
                Q7(index)=NaN;
            end
        end
        Q4=Q7;
end

end