
function [AcousticFrequency1,AcousticFrame1,OutStartH,OutEndH,OutStartL,OutEndL,HFrq,LFrq]=WhipClustering(AcousticFrequency1,AcousticFrame1, AcousticFrequency2,AcousticFrame2,AcousticFrequency3,AcousticFrame3,fs,window,c,I1,T,F)
%This function is about to detect the whip in recordings'spectrogram mainly based on dB
% AcousticFrequency: the frequency bin of whistle, whip, oscillation
% AcouticFrame: the frame of whistle,whip, oscillation
%there are two parameters to adjust: 
%1) timeInterval  (0.03--0.05)
%2) frameThreshold (3-6)
% meanwhile, please pay attention to filter's bandwidth

% timeInterval=0.05;
timeBetw=0.05;
frameThreshold=8;
freqNThe=10;
freqDistance=1;

widthBetWhip=fix(timeBetw*fs/window);

% widthOfWhip=fix(timeInterval*fs/window);
GroupNumbers=size(AcousticFrame1,1);
% WhipFrame=zeros(GroupNumbers,size(AcousticFrame1,2));
% WhipFrequency=zeros(GroupNumbers,size(AcousticFrequency1,2));
showImage1(c,I1,AcousticFrequency1,AcousticFrame1,AcousticFrequency2,AcousticFrame2,AcousticFrequency3,AcousticFrame3,T,F,15,2);
% first need to combine the data generated from three band pass filters

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
showImage1(c,I1,AcousFrq,AcousFrm,T,F,15,2);
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


index4=1;
index5=1;
index7=1;
index8=1;
for groupIndex=1:GroupNumbers
    %first,we need to compress whip dots into one group
    count=1;
    count1=1;    
    for index=1:size(AcousFrm,2)
        if AcousFrm(groupIndex,index)~=0
            if count==1
                tem(count)=AcousFrm(groupIndex,index);
                teq(count)=AcousFrq(groupIndex,index);               
            elseif count>=2
                tem(count)=AcousFrm(groupIndex,index);
                teq(count)=AcousFrq(groupIndex,index);
                prev=tem(count-1);
                cur=tem(count);
                distance=cur-prev;
                if distance>=0&&distance<=widthBetWhip
                    count1=count1+1;
                    if count1==frameThreshold %连续5个点间距小于相邻whip的间距，则将5个点的第一个点作为whip的起始点，继续进行下一个点判断
                       StartPoint(index4)=tem(count-count1+1);
                       StartFrq(index4)=teq(count-count1+1);
                       index4=index4+1;
                    end
                elseif (count1<frameThreshold)&&(distance>widthBetWhip)
                    for index1=1:count %擦去之前的tem值。放在第一格中当前值
                        tem(count)=0;
                        teq(count)=0;
                    end
                    count=1; 
                    tem(count)=AcousFrm(groupIndex,index);
                    teq(count)=AcousFrq(groupIndex,index);
                    count1=1;
                elseif (count1>frameThreshold)&&(distance>widthBetWhip)
                    EndPoint(index5)=tem(count-1);
                    EndFrq(index5)=teq(count-1);
                    count1=1;
                    index5=index5+1;
                    for index1=1:count %擦去之前的tem值。放在第一格中当前值
                        tem(index1)=0;
                        teq(index1)=0;
                    end
                    count=1; 
                    tem(count)=AcousFrm(groupIndex,index);
                    teq(count)=AcousFrq(groupIndex,index);
                    %calculate how many frequency bins contained in this
                    %whip
                    CopyOfAcousticFreq1=AcousFrq(groupIndex,StartPoint(index4-1):EndPoint(index5-1));
                    MaxFreq1=1;
                    index1=1;
                    while MaxFreq1
                        MaxFreq1=max(CopyOfAcousticFreq1);
                        PrefreqBins1(index1)=MaxFreq1;
                        for index2=1:size(CopyOfAcousticFreq1,2)
                            if CopyOfAcousticFreq1(index2)==MaxFreq1
                                CopyOfAcousticFreq1(index2)=0;
                            end
                        end
                        index1=index1+1;
                    end
                    % get rid of frequency band 0
                    FreqBins1=PrefreqBins1(1:(index1-2));
                    
                    if size(FreqBins1,2)>=freqNThe
                        %calculate the distance between two highest frequency bins
                        preValue=FreqBins1(1);
                        for index1=2:3%another threshold for frequency bins 
                            distance=abs(FreqBins1(index1)-preValue);
                            if distance>freqDistance%another threshold for frequency bins,just calculates the first 3 frequency bands
                                if index1==2
                                   newFrq=FreqBins1(index1:size(FreqBins1,2));
                                elseif index1==3
                                   newFrq=FreqBins1(index1:size(FreqBins1,2));
                                end
                                preValue=newFrq(1);
                                for index2=StartPoint(index4-1):EndPoint(index5-1)
                                    if index1==2
                                        if AcousFrq(groupIndex,index2)==FreqBins1(1);
                                            AcousFrq(groupIndex,index2)=0;
                                            AcousFrm(groupIndex,index2)=0;
                                        end
                                    elseif index1==3
                                        if AcousFrq(groupIndex,index2)==FreqBins1(1);
                                            AcousFrq(groupIndex,index2)=0;
                                            AcousFrm(groupIndex,index2)=0;
                                        end
                                        if AcousFrq(groupIndex,index2)==FreqBins1(2);
                                            AcousFrq(groupIndex,index2)=0;
                                            AcousFrm(groupIndex,index2)=0;
                                        end
                                    end
                                end                            
                            elseif  distance<=freqDistance
                                preValue=FreqBins1(index1);
                                if index1==2;
                                    newFrq=FreqBins1;
                                elseif index1==3;
                                    newFrq=newFrq;
                                end
                            end                             
                        end
                        %calculate the distance between two lowest frequency
                        %bins.After this step, newFrq stores the frequency
                        %bands within this current whip, from highest to the
                        %lowest
                        preValue=newFrq(size(newFrq,2));
                        for index1=(size(newFrq,2)-1):-1:(size(newFrq,2)-2)%another threshold for frequency bins 
                            distance=abs(newFrq(index1)-preValue);
                            if distance>freqDistance%another threshold for frequency bins,just calculates the first 3 frequency bands
                                if index1==(size(newFrq,2)-1)
                                   newFrq1=newFrq(1:index1);
                                elseif index1==(size(newFrq,2)-2)
                                   newFrq1=newFrq(1:index1);
                                end
                                preValue=newFrq1(size(newFrq1,2));
                                for index2=StartPoint(index4-1):EndPoint(index5-1)
                                    if index1==(size(newFrq,2)-1)
                                        if AcousFrq(groupIndex,index2)==newFrq(size(newFrq,2));
                                            AcousFrq(groupIndex,index2)=0;
                                            AcousFrm(groupIndex,index2)=0;
                                        end
                                    elseif index1==(size(newFrq,2)-2)
                                        if AcousFrq(groupIndex,index2)==newFrq((size(newFrq,2)-1));
                                            AcousFrq(groupIndex,index2)=0;
                                            AcousFrm(groupIndex,index2)=0;
                                        end
                                    end
                                end                            
                            elseif  distance<=freqDistance
                                preValue=newFrq(index1);
                                if index1==(size(newFrq,2)-1);
                                    newFrq1=newFrq;
                                elseif index1==(size(newFrq,2)-2);
                                    newFrq1=newFrq1;
                                end
                            end                             
                        end

                        %cluster the whip
                        WhipFrame=AcousFrm(groupIndex,StartPoint(index4-1):EndPoint(index5-1));
                        WhipFrequency=AcousFrq(groupIndex,StartPoint(index4-1):EndPoint(index5-1));
                        %calculate how many dots in the highest frequency bands
                        index6=1;
                        
                        for index1=1:size(WhipFrequency,2)
                            if WhipFrequency(index1)==newFrq1(1)
                                frame(index6)=WhipFrame(index1);
                                index6=index6+1;
                             end
                        end
                        
                        MaxFrmHFrq=max(frame);
                        OutEndH(index7)=MaxFrmHFrq;
                        if (index6-1)>=2
                            index11=1;
                            while MaxFrmHFrq
                                for index1=1:size(frame,2)
                                    if MaxFrmHFrq==frame(index1)
                                        frame(index1)=0;
                                    end
                                end
                                MaxFrmHFrq=max(frame);
                                zan(index11)=MaxFrmHFrq;
                                index11=index11+1;
                            end
                            MinFrmHFrq=zan(index11-2);
                        else
                            MinFrmHFrq=max(frame);
                        end
                        HFrequency=newFrq1(1);
                        OutStartH(index7)=MinFrmHFrq;                        
                        HFrq(index7)=HFrequency;
                        index7=index7+1;
                        for index9=1:(index6-1)
                            frame(index9)=0;                       
                        end
                        %calculate how many dots in the lowest frequency
                        %bands
                        index6=1;
                        for index1=1:size(WhipFrequency,2)
                            if WhipFrequency(index1)==newFrq1(size(newFrq1,2))
                                frame1(index6)=WhipFrame(index1);
                                index6=index6+1;
                            end
                        end
                    
                        MaxFrmLFrq=max(frame1);
                        OutEndL(index8)=MaxFrmLFrq;
                        if (index6-1)>=2
                            index11=1;
                            while MaxFrmLFrq
                                for index1=1:size(frame1,2)
                                    if MaxFrmLFrq==frame1(index1)
                                        frame1(index1)=0;
                                    end
                                end
                                MaxFrmLFrq=max(frame1);
                                zan1(index11)=MaxFrmLFrq;
                                index11=index11+1;
                            end
                             MinFrmLFrq=zan1(index11-2);
                        else
                            MinFrmLFrq=max(frame1);
                        end                       
                       
                        LFrequency=newFrq1(size(newFrq1,2));
                        OutStartL(index8)=MinFrmLFrq;
                        LFrq(index8)=LFrequency;
                        index8=index8+1;
                        for index9=1:(index6-1)
                            frame1(index9)=0;                       
                        end
                    end
                end
            end
            count=count+1;
        end
    end
end 
end

