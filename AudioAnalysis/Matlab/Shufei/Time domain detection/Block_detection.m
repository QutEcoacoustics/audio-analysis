function [BlockFrequency,BlockFrame]=Block_detection(AcousticFrequency,AcousticFrame,fs,window)
%This code is about to detect the location of Block in Spectrogram mainly based on dB
% AcousticFrequency: the frequency bin of whistle, whip, Block
% AcouticFrame: the frame of whistle,whip, Block
timeInterval=1;
overlapPent=0.5;
frameThres=5;
freqThresh=5; %ok, after test, this parameter doesn't affect the code. This means there is something wrong with it. need to adjust
Min=0.02;%ok, trying to adjust these two parameters can't solve the problem. The thing is that there is something wrong with the frequency bands group. 
Max=0.20; %ok, same as up line.

GroupNumbers=size(AcousticFrame,1);
BlockFrame=zeros(GroupNumbers,size(AcousticFrame,2));
BlockFrequency=zeros(GroupNumbers,size(AcousticFrequency,2));
windowOfFrame=fix(timeInterval*fs/window);
overlap=fix(overlapPent*windowOfFrame);
minDistance=round(Min*fs/window);
maxDistance=round(Max*fs/window);


for groupIndex=1:GroupNumbers
    %calculate the number of frequency bins and store in FreqBins from high to low.
    CopyOfAcousticFreq=AcousticFrequency;
    MaxFreq=1;
    index1=1;
    while MaxFreq
          MaxFreq=max(CopyOfAcousticFreq(groupIndex,:));
          PrefreqBins(groupIndex,index1)=MaxFreq;
          for index=1:size(AcousticFrame,2)
              if CopyOfAcousticFreq(groupIndex,index)==MaxFreq
                 CopyOfAcousticFreq(groupIndex,index)=0;
              end
          end
          index1=index1+1;
    end
    FreqBins=zeros(GroupNumbers,index1-2);
    FreqBins(groupIndex,1:(index1-2))=PrefreqBins(groupIndex,1:(index1-2));
    if size(FreqBins,2)>=freqThresh %if the number of frequency bands is lower than frequency threshold,directly go to next group
        count1=1;
        for freqIndex=1:(size(FreqBins,2)-1)
%         to make sure there are adjacent freqency bands because of 
%         Block has consequent frequency bands
        difference= FreqBins(groupIndex,freqIndex)- FreqBins(groupIndex,freqIndex+1);
        if difference==1
           count1=count1+1;
        else
%            just left those consequent frequency bands
           FreqBins(groupIndex,freqIndex)=0;
        end
        end        
        if count1>=freqThresh
      % get rid of frequency band 0
         newindex=1;
         for index=1:size(FreqBins,2)
             if FreqBins(index)~=0
                 FreqBins(groupIndex,newindex)=FreqBins(groupIndex,index);
                 newindex=newindex+1;
             end             
         end
     
        Block=zeros(size(FreqBins,2),size(AcousticFrame,2));
        frequency=zeros(size(FreqBins,2),size(AcousticFrame,2));
    %calculate the frame numbers falling into the frame window
        for index=1:size(FreqBins,2)
            for frameIndex=1:size(AcousticFrame,2)
                if AcousticFrequency(groupIndex,frameIndex)==FreqBins(groupIndex,index)
                    Block(index,frameIndex)=AcousticFrame(groupIndex,frameIndex);
                    frequency(index,frameIndex)=AcousticFrequency(groupIndex,frameIndex);
                end
            end             
            for index1=1:overlap:(size(AcousticFrame,2)-2*windowOfFrame)
                
                temStore=Block(index,index1:(windowOfFrame+index1-1));
                count=0;
                index3=1;
                nonzeros=zeros(1,size(AcousticFrame,2));
                indexSeries=zeros(1,size(AcousticFrame,2));
                for index2=1:windowOfFrame
                    if temStore(index2)~=0
                       nonzeros(index3)=temStore(index2);
                       indexSeries(index3)=index2;
                       count=count+1;
                       index3=index3+1;
                    end
                    %add the frame threshold
                    if temStore(index2)~=0
                        if count>=frameThres
                          for index4=1:(index3-2)
                              distance=nonzeros(index4+1)-nonzeros(index4);
                              if (distance>=maxDistance)&&(nonzeros(index4+2)==0)
                                  BlockFrame(groupIndex,(indexSeries(index4)+index1-1))=AcousticFrame(groupIndex,(indexSeries(index4)+index1-1));
                                  BlockFrequency(groupIndex,(indexSeries(index4)+index1-1))=AcousticFrequency(groupIndex,(indexSeries(index4)+index1-1));
                              end                                
                              if (distance>=minDistance&&distance<=maxDistance)
                                  BlockFrame(groupIndex,(indexSeries(index4)+index1-1))=AcousticFrame(groupIndex,(indexSeries(index4)+index1-1));
                                  BlockFrequency(groupIndex,(indexSeries(index4)+index1-1))=AcousticFrequency(groupIndex,(indexSeries(index4)+index1-1));
                              end
                          end
                        end 
                    end
                 
                end         
             end       
        end
        end
    end
end
end