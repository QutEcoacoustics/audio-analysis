function [WhipFrequency,WhipFrame]=WhipDetection(AcousticFrequency,AcousticFrame,fs,window)
%This function is about to detect the whip in recordings'spectrogram mainly based on dB
% AcousticFrequency: the frequency bin of whistle, whip, oscillation
% AcouticFrame: the frame of whistle,whip, oscillation
%there are two parameters to adjust: 
%1) timeInterval  (0.03--0.05)
%2) frameThreshold (3-6)
% meanwhile, please pay attention to filter's bandwidth

timeInterval=0.05;
frameThreshold=5;

widthOfWhip=fix(timeInterval*fs/window);
GroupNumbers=size(AcousticFrame,1);
WhipFrame=zeros(GroupNumbers,size(AcousticFrame,2));
WhipFrequency=zeros(GroupNumbers,size(AcousticFrequency,2));


for groupIndex=1:GroupNumbers
    index1=1;
    NonzerosValue=zeros(1,size(AcousticFrame,2));
    indexSeries=zeros(1,size(AcousticFrame,2));
    count=0;
    count1=1;
    for index=1:size(AcousticFrame,2)
        %withdrawn the nonzeros value from AcousticFrame
        if AcousticFrame(groupIndex,index)~=0
            NonzerosValue(index1)=AcousticFrame(groupIndex,index);
            indexSeries(index1)=index;
            if count==0
            baseValue=NonzerosValue(1);
            end
            count=count+1;
            index1=index1+1;
        end
           
      if AcousticFrame(groupIndex,index)~=0
          % conduct the difference between first two nonzero values 
        if count==2
            difference=NonzerosValue(index1-1)-baseValue;            
            if (difference>=0)&&(difference<=widthOfWhip)
                count1=count1+1; 
            end                    
        end
        %  from the third nonzero value, calculate the difference between the current value with the former value and the base value     
        if count>=3
            difference1=NonzerosValue(index1-1)-NonzerosValue(index1-2);
            difference2=NonzerosValue(index1-1)-baseValue;
            %compare the difference with width of whip and output the new
            %array
            if difference2<=widthOfWhip
                count1=count1+1;
                if count1>=frameThreshold
                   for index3=(index1-1-count1+1):(index1-1)
                       WhipFrame(groupIndex,indexSeries(index3))=AcousticFrame(groupIndex,indexSeries(index3));
                       WhipFrequency(groupIndex,indexSeries(index3))=AcousticFrequency(groupIndex,indexSeries(index3));
                   end                    
                end
            elseif (difference2>widthOfWhip)&&(difference1>widthOfWhip)
                baseValue=NonzerosValue(index1-1);
                count1=1;
            elseif (difference2>widthOfWhip)&&(difference1<=widthOfWhip)
                baseValue=NonzerosValue(index1-2);
                count1=2;                    
            end
        end
      end
    end
end
end