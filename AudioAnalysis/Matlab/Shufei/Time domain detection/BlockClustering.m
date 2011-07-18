function [AcousSig,frequency,timeFrame,OutStartH,OutEndH,OutStartL,OutEndL,HFrq,LFrq]=BlockClustering(AcousticFrequency1,AcousticFrame1, AcousticFrequency2,AcousticFrame2,AcousticFrequency3,AcousticFrame3,fs,window)
%This function is about to detect the block in recordings'spectrogram mainly based on dB
% AcousticFrequency: the frequency bin of whistle, Block, block
% AcouticFrame: the frame of whistle,Block,block

timeBetw=0.05;
frameThreshold=30;
freqNThe=6;
freqDistance=1;
freNumbThe=5;
frmInFrq=3;

widthBetBlock=fix(timeBetw*fs/window);

% widthOfBlock=fix(timeInterval*fs/window);
GroupNumbers=size(AcousticFrame1,1);
% BlockFrame=zeros(GroupNumbers,size(AcousticFrame1,2));
% BlockFrequency=zeros(GroupNumbers,size(AcousticFrequency1,2));
% showImage1(c,I1,AcousticFrequency1,AcousticFrame1,AcousticFrequency2,AcousticFrame2,AcousticFrequency3,AcousticFrame3,T,F,15,2);
% first need to combine the data generated from three band pass filters

%calculate the frequency bins covered by the AcousticFrequency 
FreqBins1=freqBins_calculation(AcousticFrequency1);
FreqBins2=freqBins_calculation(AcousticFrequency2);
FreqBins3=freqBins_calculation(AcousticFrequency3);

MaxFreq=max(FreqBins2(1),FreqBins3(1));
MinFreq=min(FreqBins1(size(FreqBins1,2)),FreqBins2(size(FreqBins2,2)));
FreqBands=MaxFreq;

AcousSig=zeros(FreqBands,size(AcousticFrame1,2));
for groupIndex=1:GroupNumbers
    for index2=1:size(AcousSig,1)
        for index1=1:size(AcousticFrequency1,2)
            if (AcousticFrequency1(groupIndex,index1)~=0)&&(AcousticFrequency1(groupIndex,index1))==index2
                for index3=1:size(AcousSig,2)
                    if AcousticFrame1(groupIndex,index1)==index3
                       AcousSig(MaxFreq+1-index2,index3)=100;
                    end
                end
            end
            if (AcousticFrequency2(groupIndex,index1)~=0)&&(AcousticFrequency2(groupIndex,index1))==index2
                for index3=1:size(AcousSig,2)
                    if AcousticFrame2(groupIndex,index1)==index3
                       AcousSig(MaxFreq+1-index2,index3)=100;
                    end
                end
            end
            if (AcousticFrequency3(groupIndex,index1)~=0)&&(AcousticFrequency3(groupIndex,index1))==index2
                for index3=1:size(AcousSig,2)
                    if AcousticFrame3(groupIndex,index1)==index3
                       AcousSig(MaxFreq+1-index2,index3)=100;
                    end
                end
            end
        end
    end    
end

index4=1;
index5=1;
index7=1;
index8=1;

    %first,we need to compress block dots into one group
count=1;
count1=1;
count2=0;
index16=1;
for index=1:size(AcousSig,2)
    for index1=1:size(AcousSig,1)
         if AcousSig(index1,index)~=0
            frequency(index16)=MaxFreq+1-index1;
            timeFrame(index16)=index;
            index16=index16+1;
            if count==1
                AcousFrm(count)=index;
                AcousFrq(count)=MaxFreq+1-index1;               
            elseif count>=2
                AcousFrm(count)=index;
                AcousFrq(count)=MaxFreq+1-index1;
                prev=AcousFrm(count-1);
                cur=AcousFrm(count);
                distance=cur-prev;
                if distance==0
                    count2=count2+1;
                end
                if distance>=1&&distance<=widthBetBlock
                    count1=count1+1;
                    if count1==frameThreshold %连续5个点间距小于相邻Block的间距，则将5个点的第一个点作为Block的起始点，继续进行下一个点判断
                       StartPoint(index4)=AcousFrm(count-count1-count2+1);
                       StartFrq(index4)=AcousFrq(count-count1-count2+1);
                       index4=index4+1;
                    end
                elseif (count1<frameThreshold)&&(distance>widthBetBlock)
                    for index15=1:count %擦去之前的tem值。放在第一格中当前值
                        AcousFrm(index15)=0;
                        AcousFrq(index15)=0;
                    end
                    count=1; 
                    AcousFrm(count)=index;
                    AcousFrq(count)=MaxFreq+1-index1;
                    count1=1;
                    count2=0;
                elseif (count1>frameThreshold)&&(distance>widthBetBlock)
                    EndPoint(index5)=AcousFrm(count-1);
                    EndFrq(index5)=AcousFrq(count-1);
                    count1=1;
                    index5=index5+1;                    
                    %calculate how many frequency bins contained in this
                    %Block 
                    FreqBins1=0;
                    FreqBins1=freqBins_calculation(AcousFrq(1:count-1));
                    %get rid of frequency bands which have only one or two
                    %hits
                    for randIndex=1:size(FreqBins1,2)
                        num=0;
                        for randIndex1=1: (count-1)
                            if AcousFrq(randIndex1)==FreqBins1(randIndex)
                                num=num+1;
                            end
                        end
                        if num>=0&&num<=frmInFrq
                            for t=1: (count-1)
                                if AcousFrq(t)==FreqBins1(randIndex);
                                    AcousFrq(t)=0;
                                    AcousFrm(t)=0;
                                end
                            end
                            FreqBins1(randIndex)=0;
                        end
                    end
                    q=1;
                    for y=1:size(FreqBins1,2)
                        if FreqBins1(y)~=0
                           storeFre(q)=FreqBins1(y);
                           q=q+1;
                        end
                    end
                    FreqBins1=0;
                    FreqBins1=storeFre;
                    storeFre=0;
                    q=1;
                    for y=1:(count-1)
                        if AcousFrq(y)~=0
                            storeFre1(q)=AcousFrq(y);
                            storeFre2(q)=AcousFrm(y);
                            q=q+1;
                        end
                    end
                    AcousFrq=0;
                    AcousFrm=0;
                    AcousFrq=storeFre1;
                    AcousFrm=storeFre2;
                    storeFre1=0;
                    storeFre2=0;
                    
                    if size(FreqBins1,2)>=freqNThe
                        %add rules to  frequency bands
                        countFre=0;
                        temFreq=0;
                        for freIndex=1:(size(FreqBins1,2)-1)
                            diff=abs(FreqBins1(freIndex+1)-FreqBins1(freIndex));
                            if diff==freqDistance
                                countFre=countFre+1;
                                temFreq(countFre)=FreqBins1(freIndex);                                
                                if countFre==freNumbThe
                                    FreStart=temFreq(1);
                                        
                                end
                                if (countFre>=freNumbThe)&&(freIndex==size(FreqBins1,2)-1)
                                   temFreq(countFre+1)=FreqBins1(freIndex+1);
                                   FreEnd=temFreq(countFre+1);
                                    q=1;
                                    for y=1:(countFre+1)
                                        for y2=1:size(AcousFrq,2)
                                            if AcousFrq(y2)==temFreq(y)
                                                storeFre1(q)=AcousFrq(y2);
                                                storeFre2(q)=AcousFrm(y2);
                                                q=q+1;
                                            end
                                        end
                                    end
                                   countFre=0;
                                                                      
                                    BlockFrame=storeFre2;
                                    BlockFrequency=storeFre1;
                                    MaxFrmHFrq=max(BlockFrame);
                                    MaxFrmLFrq=max(BlockFrame);
                                    MinFrmHFrq=min(BlockFrame);
                                    MinFrmLFrq=min(BlockFrame);
                                    
                                    OutEndH(index7)=MaxFrmHFrq;
                                    HFrequency=FreStart;
                                    OutStartH(index7)=MinFrmHFrq;                        
                                    HFrq(index7)=HFrequency;
                                    index7=index7+1;
                                    OutEndL(index8)=MaxFrmLFrq;
                                    LFrequency=FreEnd;
                                    OutStartL(index8)=MinFrmLFrq;
                                    LFrq(index8)=LFrequency;
                                    index8=index8+1;
                                 
                                    temFreq=0;
                                   
                                end
                               
                            elseif (countFre<freNumbThe)&&(diff>freqDistance)

                                countFre=0;
                                for i=1:size(AcousFrq,2)
                                    if AcousFrq(i)==FreqBins1(freIndex)
                                        AcousFrq(i)=0;
                                        AcousFrm(i)=0;
                                    end
                                end
                                
                            elseif (countFre>=freNumbThe)&&(diff>freqDistance)
                                    temFreq(countFre+1)=FreqBins1(freIndex);
                                    FreEnd=temFreq(countFre+1);
                                    
                                    q=1;
                                    for y=1:(countFre+1)
                                        for y2=1:size(AcousFrq,2)
                                            if AcousFrq(y2)==temFreq(y)
                                                storeFre1(q)=AcousFrq(y2);
                                                storeFre2(q)=AcousFrm(y2);
                                                q=q+1;
                                            end
                                        end
                                    end

                                    countFre=0;
                                    
                                    BlockFrame=storeFre2;
                                    BlockFrequency=storeFre1;
                                    
                                    MaxFrmHFrq=max(BlockFrame);
                                    MinFrmHFrq=min(BlockFrame);
                                    OutEndH(index7)=MaxFrmHFrq;                                   
                                    HFrequency=FreStart;
                                    OutStartH(index7)=MinFrmHFrq;                        
                                    HFrq(index7)=HFrequency;
                                    index7=index7+1;
                                    
                                    
                                    MaxFrmLFrq=max(BlockFrame);
                                    MinFrmLFrq=min(BlockFrame);
                                    OutEndL(index8)=MaxFrmLFrq;                                                     
                                    LFrequency=FreEnd;
                                    OutStartL(index8)=MinFrmLFrq;
                                    LFrq(index8)=LFrequency;
                                    index8=index8+1;
                                  
                                    temFreq=0;
                                end
                                             
                        end
                        
                      %擦去之前的tem值。放在第一格中当前值
                        AcousFrm=0;
                        AcousFrq=0;
                        
                        count=1; 
                        count2=0;
                        AcousFrm(count)=index;
                        AcousFrq(count)=MaxFreq+1-index1;
                    end
                end
            end
            count=count+1;
        end
    end
end 
end             
                    
                    

function FreqBins=freqBins_calculation(array)
%calculate the frequency bins covered by the AcousticFrequency 
CopyOfAcousticFreq=array;
MaxFreq=1;
index1=1;
while MaxFreq
    MaxFreq=max(CopyOfAcousticFreq);
    PrefreqBins(index1)=MaxFreq;
    for index=1:size(array,2)
        if CopyOfAcousticFreq(index)==MaxFreq
            CopyOfAcousticFreq(index)=0;
        end
    end
    index1=index1+1;
end
% get rid of frequency band 0
FreqBins=PrefreqBins(1:(index1-2));

end


