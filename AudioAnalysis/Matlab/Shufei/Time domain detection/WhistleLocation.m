function [WhistleFrequency, WhistleFrame]=WhistleLocation(AcousticFrequency,AcousticFrame,fs,window)
% This function is to group the whistle in Spectrogram
% AcousticFrequency: the frequency bin of whistle, whip, oscillation
% AcouticFrame: the frame of whistle,whip, oscillation
%there are three parameters: 1)frame interval (time interval):0.2s
%                          2)number threshold of frames in each frame
%                          interval: 4
%                          3)overlap pencentage of the frame window: 50% 

        timeInterval=0.2;
        frameThreshold=3;
        overlapPencen=0.5;

              
        %calculate the number of groups
        GroupNumbers=size(AcousticFrame,1);
        WhistleFrame=zeros(GroupNumbers,size(AcousticFrame,2));
        WhistleFrequency=zeros(GroupNumbers,size(AcousticFrame,2));
%         PrefreqBins=zeros(GroupNumbers,:);
        for groupIndex=1:GroupNumbers
            %calculate the number of frequency bins and store in FreqBins
            %from high to low.
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
         % get rid of frequency band 0
            FreqBins=PrefreqBins(groupIndex,1:(index1-2));
            whistle=zeros(size(FreqBins,2),size(AcousticFrame,2));
            frequency=zeros(size(FreqBins,2),size(AcousticFrame,2));

           for index2=1:size(FreqBins,2)    % this is the highest frequency bin
               for index=1:size(AcousticFrame,2) % to grab the acoustic frame in this frequency bin. 
                   if AcousticFrequency(groupIndex,index)==FreqBins(groupIndex,index2)
                       whistle(index2,index)=AcousticFrame(groupIndex,index); % restore these acoustic frames in order of time-series
                       frequency(index2,index)=AcousticFrequency(groupIndex,index);                
                   end             
               end
               windowOfFrame=fix(timeInterval*fs/window); %every 0.2s, count the frame number of whistle
               overlap=fix(windowOfFrame*overlapPencen);
               i=0;
               for index3=1:overlap:(size(whistle,2)-2*windowOfFrame) %to avoid the boundary effect
                   frameOfWhistle=whistle(index2,index3:(windowOfFrame+i*overlap));
                   count=0;
                   for index4=1:windowOfFrame
                       if frameOfWhistle(index4)~=0
                           count=count+1;
                       end
                   end
                   if count<frameThreshold    %add rules of detecting whistle. Only count number >=4, frames are called whistle.
                      whistle(index2,index3:index3+overlap)=0;
                      frequency(index2,index3:index3+overlap)=0;
                   end
                   i=i+1;
               end
               for index=1:size(whistle,2)
                   if (whistle(index2,index)~=0)&&(frequency(index2,index)~=0)
                       WhistleFrame(groupIndex,index)=whistle(index2,index);
                       WhistleFrequency(groupIndex,index)=frequency(index2,index);
                   end
               end
        
           end           
       end
        
end
             
               
             
