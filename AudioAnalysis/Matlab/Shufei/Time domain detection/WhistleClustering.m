function [StartPoint,EndPoint,AcousFrq,AcousFrm,FreqBins,OutStart1,OutEnd1,OutStart2,OutEnd2,SFrq,EFrq]=WhistleClustering(AcousticFrequency1,AcousticFrame1, AcousticFrequency2,AcousticFrame2,AcousticFrequency3,AcousticFrame3,fs,window)
%this code is about to cluster all points generated from three band pass
%filters. 
%parameters

timeInterval=0.1;
frmNumThe=5;
frmClusThe=3;
whisFramTheh=fix(timeInterval*fs/window);


StartPoint=0;
EndPoint=0;
AcousFrq=0;
AcousFrm=0;
OutStart1=0;
OutEnd1=0;
OutStart2=0;
OutEnd2=0;
SFrq=0;
EFrq=0;




% first we need to combine the data generated from three band pass filters
for index=1:size(AcousticFrame1,2)
    if (AcousticFrequency2(index)~=0)&&(AcousticFrame2(index)~=0)
        AcousticFrequency1(index)=AcousticFrequency2(index);
        AcousticFrame1(index)=AcousticFrame2(index);
    end
end

for index=1:size(AcousticFrame1,2)
    if (AcousticFrequency3(index)~=0)&&(AcousticFrame3(index)~=0)
        AcousticFrequency1(index)=AcousticFrequency3(index);
        AcousticFrame1(index)=AcousticFrame3(index);
    end
end
AcousFrq=AcousticFrequency1;
AcousFrm=AcousticFrame1;

%calculate the frequency bins covered by the AcousticFrequency 
CopyOfAcousticFreq=AcousFrq;
MaxFreq=1;
index1=1;
while MaxFreq
    MaxFreq=max(CopyOfAcousticFreq);
    PrefreqBins(index1)=MaxFreq;
    for index=1:size(AcousFrm,2)
        if CopyOfAcousticFreq(index)==MaxFreq
            CopyOfAcousticFreq(index)=0;
        end
    end
    index1=index1+1;
end
% get rid of frequency band 0
FreqBins=PrefreqBins(1:(index1-2));

%cluster the whistle
groupIndex=size(AcousFrq,1);
temFrm=zeros(size(FreqBins,2),size(AcousFrq,2));
temFrq=zeros(size(FreqBins,2),size(AcousFrm,2));
StartPoint=zeros(size(FreqBins,2),size(AcousFrq,2));
EndPoint=zeros(size(FreqBins,2),size(AcousFrq,2));


for index=1:size(FreqBins,2)
    for index1=1:size(AcousFrm,2)
        if AcousFrq(groupIndex,index1)==FreqBins(groupIndex,index)
            temFrm(index,index1)=AcousFrm(groupIndex,index1);
            temFrq(index,index1)=AcousFrq(groupIndex,index1);
        end
    end  
end

%add rules to group whistles
index2=1;
nonValue=zeros(size(FreqBins,2),size(AcousticFrame1,2));
indexSeries=zeros(size(FreqBins,2),size(AcousticFrame1,2));
for index=size(temFrq,1):-1:1
    
    count=0;
    index1=1;
    for indexFrm=1:size(temFrm,2)
        if temFrm(index,indexFrm)~=0
           count=count+1;
           nonValue(index,index1)=temFrm(index,indexFrm);
           indexSeries(index,index1)=indexFrm;
           index1=index1+1;
        end
        if temFrm(index,indexFrm)~=0
        if count>=2&& count<=frmNumThe
           distance=nonValue(index,index1-1)-nonValue(index,index1-2);
           if distance>whisFramTheh
               count=1;
               nonValue(index,index1-2)=0;
               indexSeries(index,index1-2)=0;
           end
        end
        if count>=frmNumThe
           Sum=0;
           for index4=(indexFrm+1):size(temFrm,2)
               Sum=Sum+temFrm(index,index4);
           end
           
           if Sum>0
                  distance1=nonValue(index,index1-1)-nonValue(index,index1-2);
                  if distance1>whisFramTheh
                     StartPoint(index,index2)=nonValue(index,index1-count);
                     EndPoint(index,index2)=nonValue(index,index1-2);
                     index2=index2+1;
                     for index3=1:(index1-2)
                          nonValue(index,index3)=0;
                          indexSeries(index,index3)=0;
                     end
                     count=1; 
                  end 
           elseif Sum==0
                  distance1=nonValue(index,index1-1)-nonValue(index,index1-2);
                  if distance1<=whisFramTheh
                     StartPoint(index,index2)=nonValue(index,index1-count);
                     EndPoint(index,index2)=nonValue(index,index1-1);
                     index2=index2+1;
                  else
                     StartPoint(index,index2)=nonValue(index,index1-count);
                     EndPoint(index,index2)=nonValue(index,index1-2);
                     index2=index2+1; 
                  end
                  for index3=1:(index1-2)
                      nonValue(index,index3)=0;
                      indexSeries(index,index3)=0;
                  end
                  count=1;               
           end
           
         end
        end
    end    
end
index1=1;

%add more rules to group whistle in different frequency bands.
for indexFrq=size(StartPoint,1):-1:2  %indexFrq represents the index of frequency bins
    for indexFrm=1:size(StartPoint,2)
       % indexFrm represents the index of frame 
        if StartPoint(indexFrq,indexFrm)~=0 
            for indexFrm1=1:size(StartPoint,2)
                % indexFrm1 represents the index of frames in second frequency bin
                if StartPoint(indexFrq-1,indexFrm1)~=0
                    %begin to join the whistles in two frequency bins
                    
                    S1=StartPoint(indexFrq,indexFrm);
                    S2=StartPoint(indexFrq-1,indexFrm1);
                    E1=EndPoint(indexFrq,indexFrm);
                    E2=EndPoint(indexFrq-1,indexFrm1);
                  
                    %first case output (start,end)=(StartPoint2,EndPoint1)
                    if ((S1-E2>=0) &&(S1-E2)<=frmClusThe)||((S1>=S2)&&(S1<=E2))
                        
                            OutStart1(index1)=S2;
                            OutStart2(index1)=S1;
                            OutEnd1(index1)=E2;
                            OutEnd2(index1)=E1;
                            SFrq(index1)=FreqBins(indexFrq-1);
                            EFrq(index1)=FreqBins(indexFrq);
                            index1=index1+1;
                                                                                       
                    %second case output(start,end)=(S1,E2)
                    elseif ((S2-E1>=0)&&(S2-E1<=frmClusThe))||((S2>=S1)&&(S2<=E1))
                        OutStart1(index1)=S1;
                        OutStart2(index1)=S2;
                        OutEnd1(index1)=E1;
                        OutEnd2(index1)=E2;
                        SFrq(index1)=FreqBins(indexFrq);
                        EFrq(index1)=FreqBins(indexFrq-1);
                        index1=index1+1;
    
                    %3rd case output(start,end)=(S1,E1)
                    elseif ((S2>=S1)&&(S2<=E1))&&((E2>=S1)&&(E2<=E1))
                        OutStart1(index1)=S1;
                        OutStart2(index1)=S2;
                        OutEnd1(index1)=E2;
                        OutEnd2(index1)=E1;
                       SFrq(index1)=FreqBins(indexFrq);
                        EFrq(index1)=FreqBins(indexFrq-1);
                        index1=index1+1;                          

                    %4th case output(start,end)=(S2,E2)
                    elseif ((S1>=S2)&&(S1<=E2))&&((E1>=S2)&&(E1<=E2))
                        OutStart1(index1)=S2;
                        OutStart2(index1)=S1;
                        OutEnd1(index1)=E1;
                        OutEnd2(index1)=E2;
                        SFrq(index1)=FreqBins(indexFrq-1);
                        EFrq(index1)=FreqBins(indexFrq);
                        index1=index1+1;                                              
                    end                  
                end
                
             end
        end
        
     
    end
  
end
end





