
function [AcousSig,frequency,timeFrame,OutStartH,OutEndH,OutStartL,OutEndL,HFrq,LFrq]=WhipClustering(AcousticFrequency1,AcousticFrame1, AcousticFrequency2,AcousticFrame2,AcousticFrequency3,AcousticFrame3,fs,window)
%This function is about to detect the whip in recordings'spectrogram mainly based on dB
% AcousticFrequency: the frequency bin of whistle, whip, oscillation
% AcouticFrame: the frame of whistle,whip, oscillation
%there are two parameters to adjust: 
%1) timeInterval  (0.03--0.05)
%2) frameThreshold (3-6)
% meanwhile, please pay attention to filter's bandwidth

% timeInterval=0.05;
timeBetw=0.05; % whip中点的间距，如果大于此间距，视为下一信号的开始，不在本whip之内。
frameThreshold=8;% 连续这么多个点之间的间距小于timeBetW，则将这几个连续值的第一个视为whip的开端点。然后继续检测，知道间距大于timeBetW，将终点值存放
freqNThe=4;%在检测到的假设whip区间内，如果总共跨越频带多余这个阈值，则视为存在whip信号，否则放弃，继续检测下一序列信号
% freqDistance=5;%粗略估算，如果相邻频带间距超越5个带宽，
% freNumbThe=3;
gapThe=15;  %在上述通过time frame 限定的假设whip区间内，如果相邻频带的间距小于此阈值，则判定相邻两点同属于一个whip信号，如果大于，则放弃，继续检测下一信号 
numThe=3;   % 继续增强上一阈值，如果连续numThe个点所在频带的间距小于gapThe，则，将这连续numThe的第一个值作为信号的开端，继续检测终端，直到，相邻频带距离大于gapThe，确定终端位置
widthBetWhip=fix(timeBetw*fs/window);

% widthOfWhip=fix(timeInterval*fs/window);
GroupNumbers=size(AcousticFrame1,1);
% WhipFrame=zeros(GroupNumbers,size(AcousticFrame1,2));
% WhipFrequency=zeros(GroupNumbers,size(AcousticFrequency1,2));
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

%first,we need to compress whip dots into one group
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
                if distance>=1&&distance<=widthBetWhip
                    count1=count1+1;
                    if count1==frameThreshold %连续5个点间距小于相邻whip的间距，则将5个点的第一个点作为whip的起始点，继续进行下一个点判断
                       StartPoint(index4)=AcousFrm(count-count1-count2+1);
                       StartFrq(index4)=AcousFrq(count-count1-count2+1);
                       index4=index4+1;
                    end
                elseif (count1<frameThreshold)&&(distance>widthBetWhip)
                    for index15=1:count %擦去之前的tem值。放在第一格中当前值
                        AcousFrm(index15)=0;
                        AcousFrq(index15)=0;
                    end
                    count=1; 
                    AcousFrm(count)=index;
                    AcousFrq(count)=MaxFreq+1-index1;
                    count1=1;
                    count2=0;
                elseif (count1>frameThreshold)&&(distance>widthBetWhip)
                    EndPoint(index5)=AcousFrm(count-1);
                    EndFrq(index5)=AcousFrq(count-1);
                    count1=1;
                    index5=index5+1;                    
                    %calculate how many frequency bins contained in this
                    %whip 
                    FreqBins1=freqBins_calculation(AcousFrq(1:count-1));
                    if size(FreqBins1,2)>=freqNThe
                         %cluster the whip
                         WhipFrame=AcousFrm(1:count-1);
                         WhipFrequency=AcousFrq(1:count-1);
                         %need to set threshold between
                         %frequency bands
                         cal=0;
                         zerocount=0;
                         for i=1:(size(WhipFrequency,2)-1)
                             gap=abs(WhipFrequency(i+1)-WhipFrequency(i));                         
                             if gap==0
                                 zerocount=zerocount+1;
                             end
                             if gap>0&&gap<=gapThe %frequency gap threshold for dots within whip
                                 cal=cal+1;                                                                     
                                 if cal==numThe %number threshold for a continous whip
                                     startfreq=WhipFrequency(i-cal-zerocount+1);
                                     startframe=WhipFrame(i-cal-zerocount+1);
                                     startIndex=i-cal-zerocount+1;
                                 end
                             elseif cal<numThe && gap>gapThe                                            
%                                             for index15=1:cal %擦去之前的tem值。放在第一格中当前值
%                                                 WhipFrequency(index15)=0;
%                                                 WhipFrame(index15)=0;
%                                             end
                                  cal=0;
                                  zerocount=0;
                                            %be careful, check whether
                                            %there are previous value left
%                                             WhipFrequency=WhipFrequency((i+1):size(WhipFrequency,2));
%                                             WhipFrame=WhipFrame((i+1):size(WhipFrame,2));
                                                
                                                
                               elseif cal>=numThe && gap>gapThe
                                   endfreq=WhipFrequency(i);
                                   endframe=WhipFrame(i);
                                   endIndex=i;
                                   whipdurFrm=WhipFrame(startIndex:endIndex);
                                   whipdurFrq=WhipFrequency(startIndex:endIndex);
                                   highFreq=max(whipdurFrq);
                                   lowFreq=min(whipdurFrq);
                                   i1=1;
                                   i2=1;
                                   for ranIndex=1:size(whipdurFrq,2)
                                       if whipdurFrq(ranIndex)==highFreq
                                           frameH(i1)=whipdurFrm(ranIndex);
                                           i1=i1+1;                                                      
                                       end
                                       if whipdurFrq(ranIndex)==lowFreq
                                           frameL(i2)=whipdurFrm(ranIndex);
                                           i2=i2+1;
                                       end
                                   end
                                   MaxFrmHFrq=max(frameH);
                                   OutEndH(index7)=MaxFrmHFrq;
                                   if (i1-1)>=2
                                       index11=1;
                                       while MaxFrmHFrq
                                           for index15=1:size(frameH,2)
                                               if MaxFrmHFrq==frameH(index15)
                                                   frameH(index15)=0;
                                               end
                                           end
                                           MaxFrmHFrq=max(frameH);
                                           zan(index11)=MaxFrmHFrq;
                                           index11=index11+1;
                                       end
                                       MinFrmHFrq=zan(index11-2);
                                   else
                                       MinFrmHFrq=max(frameH);
                                   end
                                   HFrequency=highFreq;
                                   OutStartH(index7)=MinFrmHFrq;                               
                                   HFrq(index7)=HFrequency;
                                   index7=index7+1;
                                   frameH=0;
                                   i1=1;
                                   MaxFrmLFrq=max(frameL);
                                   OutEndL(index8)=MaxFrmLFrq;
                                   if (i2-1)>=2
                                       index11=1;
                                       while MaxFrmLFrq
                                           for index15=1:size(frameL,2)
                                               if MaxFrmLFrq==frameL(index15)
                                                   frameL(index15)=0;
                                               end
                                           end
                                           MaxFrmLFrq=max(frameL);
                                           zan1(index11)=MaxFrmLFrq;
                                           index11=index11+1;
                                       end
                                       MinFrmLFrq=zan1(index11-2);
                                   else
                                       MinFrmLFrq=max(frameL);
                                   end
                                   LFrequency=lowFreq;
                                   OutStartL(index8)=MinFrmLFrq;
                                   LFrq(index8)=LFrequency;
                                   index8=index8+1; 
                                   frameL=0;                                   
                                   i2=1;
                                   cal=0;
                                   zerocount=0;
                             end
                         end                        
                    end
                    AcousFrm=0;
                    AcousFrq=0;                     
                    count=1;
                    count2=0;
                    AcousFrm(count)=index;
                    AcousFrq(count)=MaxFreq+1-index1;    
                end
             end
            count=count+1;
         end
    end
end 
end                                   
                        
                        
                        
                        
                        
%                         %add rules to  frequency bands
%                         countFre=0;
%                         temFreq=0;
%                         for freIndex=1:(size(FreqBins1,2)-1)
%                             diff=abs(FreqBins1(freIndex+1)-FreqBins1(freIndex));
%                            
%                             if diff<=freqDistance
%                                 countFre=countFre+1;
%                                 temFreq(countFre)=FreqBins1(freIndex);                                
%                                 if countFre==freNumbThe
%                                     FreStart=temFreq(1);                                    
%                                 end
%                                 if (countFre>=freNumbThe)&&(freIndex==size(FreqBins1,2)-1)
%                                    temFreq(countFre+1)=FreqBins1(freIndex+1);
%                                    FreEnd=temFreq(countFre+1);
%                                   
%                                    countFre=0;
%                                    %cluster the whip
%                                     WhipFrame=AcousFrm(1:count-1);
%                                     WhipFrequency=AcousFrq(1:count-1);
%                                     %need to set threshold between
%                                     %frequency bands
%                                     cal=0;
%                                     zerocount=0;
%                                     for i=1:(size(WhipFrequency,2)-1)
%                                         gap=abs(WhipFrequency(i+1)-WhipFrequency(i));
%                                         if gap==0
%                                            zerocount=zerocount+1;
%                                         end
%                                         if gap>0&&gap<=gapThe %frequency gap threshold for dots within whip                                            
%                                             cal=cal+1;                                                                        
%                                             if cal==numThe %number threshold for a continous whip
%                                                 startfreq=WhipFrequency(i-cal-zerocount+1);
%                                                 startframe=WhipFrame(i-cal-zerocount+1);
%                                                 startIndex=i-cal-zerocount+1;
%                                             end
%                                         elseif cal<numThe && gap>gapThe
%                                             
% %                                             for index15=1:cal %擦去之前的tem值。放在第一格中当前值
% %                                                 WhipFrequency(index15)=0;
% %                                                 WhipFrame(index15)=0;
% %                                             end
%                                             cal=0; 
%                                             zerocount=0;
%                                             %be careful, check whether
%                                             %there are previous value left
% %                                             WhipFrequency=WhipFrequency((i+1):size(WhipFrequency,2));
% %                                             WhipFrame=WhipFrame((i+1):size(WhipFrame,2));
%                                                 
%                                                 
%                                         elseif cal>=numThe && gap>gapThe 
%                                                 endfreq=WhipFrequency(i);
%                                                 endframe=WhipFrame(i);
%                                                 endIndex=i;
%                                                 
%                                                 whipdurFrm=WhipFrame(startIndex:endIndex);
%                                                 whipdurFrq=WhipFrequency(startIndex:endIndex);
%                                                 highFreq=max(whipdurFrq);
%                                                 lowFreq=min(whipdurFrq);
%                                                 i1=1;
%                                                 i2=1;
%                                                 for ranIndex=1:size(whipdurFrq,2)
%                                                     if whipdurFrq(ranIndex)==highFreq
%                                                         frameH(i1)=whipdurFrm(ranIndex);
%                                                         i1=i1+1;                                                          
%                                                     end
%                                                     if whipdurFrq(ranIndex)==lowFreq
%                                                          frameL(i2)=whipdurFrm(ranIndex);
%                                                          i2=i2+1;
%                                                     end
%                                                 end
%                                                 MaxFrmHFrq=max(frameH);
%                                                 OutEndH(index7)=MaxFrmHFrq;
%                                                 if (i1-1)>=2
%                                                     index11=1;
%                                                     while MaxFrmHFrq
%                                                         for index15=1:size(frameH,2)
%                                                             if MaxFrmHFrq==frameH(index15)
%                                                                 frameH(index15)=0;
%                                                             end
%                                                         end
%                                                         MaxFrmHFrq=max(frameH);
%                                                         zan(index11)=MaxFrmHFrq;
%                                                         index11=index11+1;
%                                                     end
%                                                     MinFrmHFrq=zan(index11-2);
%                                                 else
%                                                     MinFrmHFrq=max(frameH);
%                                                 end
%                                                 HFrequency=highFreq;
%                                                 OutStartH(index7)=MinFrmHFrq;                        
%                                                 HFrq(index7)=HFrequency;
%                                                 index7=index7+1;
%                                                 frameH=0;                       
%                                                 
%                                                 i1=1;
% 
%                                                 MaxFrmLFrq=max(frameL);
%                                                 OutEndL(index8)=MaxFrmLFrq;
%                                                 if (i2-1)>=2
%                                                     index11=1;
%                                                     while MaxFrmLFrq
%                                                         for index15=1:size(frameL,2)
%                                                             if MaxFrmLFrq==frameL(index15)
%                                                                 frameL(index15)=0;
%                                                             end
%                                                         end
%                                                         MaxFrmLFrq=max(frameL);
%                                                         zan1(index11)=MaxFrmLFrq;
%                                                         index11=index11+1;
%                                                     end
%                                                      MinFrmLFrq=zan1(index11-2);
%                                                 else
%                                                     MinFrmLFrq=max(frameL);
%                                                 end                       
% 
%                                                 LFrequency=lowFreq;
%                                                 OutStartL(index8)=MinFrmLFrq;
%                                                 LFrq(index8)=LFrequency;
%                                                 index8=index8+1;
%                                                
%                                                     frameL=0;                       
%                                                 
%                                                 temFreq=0;
%                                                 i2=1;
%                                                 cal=0;
%                                                 zerocount=0;
%                                          end
%                                     end                                 
%                                
%                                 elseif (countFre<freNumbThe)&&(diff>freqDistance)
%                                 countFre=0;
%                                 zerocount=0;
%                                 
%                                 elseif (countFre>=freNumbThe)&&(diff>freqDistance)
%                                    temFreq(countFre+1)=FreqBins1(freIndex);
%                                    FreEnd=temFreq(countFre+1);
%                                    countFre=0;
%                                     
%                                     %cluster the whip
%                                     WhipFrame=AcousFrm(1:count-1);
%                                     WhipFrequency=AcousFrq(1:count-1);
%                                     
%                                     cal=0;
%                                     for i=1:(size(WhipFrequency,2)-1)
%                                         gap=abs(WhipFrequency(i+1)-WhipFrequency(i));
%                                         if gap==0
%                                            zerocount=zerocount+1;
%                                         end
%                                         if gap>0&&gap<gapThe %frequency gap threshold for dots within whip                                            
%                                             cal=cal+1;                                                                        
%                                             if cal==numThe %number threshold for a continous whip
%                                                 startfreq=WhipFrequency(i-cal-zerocount+1);
%                                                 startframe=WhipFrame(i-cal-zerocount+1);
%                                                 startIndex=i-cal-zerocount+1;
%                                             end
%                                         elseif cal<numThe && gap>gapThe
%                                             
% %                                             for index15=1:cal %擦去之前的tem值。放在第一格中当前值
% %                                                 WhipFrequency(index15)=0;
% %                                                 WhipFrame(index15)=0;
% %                                             end
%                                             cal=0; 
%                                             zerocount=0;
%                                             %be careful, check whether
%                                             %there are previous value left
% %                                             WhipFrequency=WhipFrequency((i+1):size(WhipFrequency,2));
% %                                             WhipFrame=WhipFrame((i+1):size(WhipFrame,2));
% %                                                 
%                                                 
%                                         elseif cal>=numThe && gap>gapThe 
%                                                 endfreq=WhipFrequency(i);
%                                                 endframe=WhipFrame(i);
%                                                 endIndex=i;
%                                                 
%                                                 whipdurFrm=WhipFrame(startIndex:endIndex);
%                                                 whipdurFrq=WhipFrequency(startIndex:endIndex);
%                                                 highFreq=max(whipdurFrq);
%                                                 lowFreq=min(whipdurFrq);
%                                                 i1=1;
%                                                 i2=1;
%                                                 for ranIndex=1:size(whipdurFrq,2)
%                                                     if whipdurFrq(ranIndex)==highFreq
%                                                         frameH(i1)=whipdurFrm(ranIndex);
%                                                         i1=i1+1;                                                          
%                                                     end
%                                                     if whipdurFrq(ranIndex)==lowFreq
%                                                          frameL(i2)=whipdurFrm(ranIndex);
%                                                          i2=i2+1;
%                                                     end
%                                                 end
%                                                 MaxFrmHFrq=max(frameH);
%                                                 OutEndH(index7)=MaxFrmHFrq;
%                                                 if (i1-1)>=2
%                                                     index11=1;
%                                                     while MaxFrmHFrq
%                                                         for index15=1:size(frameH,2)
%                                                             if MaxFrmHFrq==frameH(index15)
%                                                                 frameH(index15)=0;
%                                                             end
%                                                         end
%                                                         MaxFrmHFrq=max(frameH);
%                                                         zan(index11)=MaxFrmHFrq;
%                                                         index11=index11+1;
%                                                     end
%                                                     MinFrmHFrq=zan(index11-2);
%                                                 else
%                                                     MinFrmHFrq=max(frameH);
%                                                 end
%                                                 HFrequency=highFreq;
%                                                 OutStartH(index7)=MinFrmHFrq;                        
%                                                 HFrq(index7)=HFrequency;
%                                                 index7=index7+1;
%                                                 
%                                                     frameH=0;                       
%                                                   
%                                                 i1=1;
% 
%                                                 MaxFrmLFrq=max(frameL);
%                                                 OutEndL(index8)=MaxFrmLFrq;
%                                                 if (i2-1)>=2
%                                                     index11=1;
%                                                     while MaxFrmLFrq
%                                                         for index15=1:size(frameL,2)
%                                                             if MaxFrmLFrq==frameL(index15)
%                                                                 frameL(index15)=0;
%                                                             end
%                                                         end
%                                                         MaxFrmLFrq=max(frameL);
%                                                         zan1(index11)=MaxFrmLFrq;
%                                                         index11=index11+1;
%                                                     end
%                                                      MinFrmLFrq=zan1(index11-2);
%                                                 else
%                                                     MinFrmLFrq=max(frameL);
%                                                 end                       
% 
%                                                 LFrequency=lowFreq;
%                                                 OutStartL(index8)=MinFrmLFrq;
%                                                 LFrq(index8)=LFrequency;
%                                                 index8=index8+1;
%                                                 
%                                                     frameL=0;                       
%                                                
%                                                 temFreq=0;
%                                                 i2=1;
%                                                 zerocount=0;
%                                          end
%                                     end  
%                                 end
%                             end
%                         end
%                         
%                        
%                         AcousFrm=0;
%                         AcousFrq=0;
%                       
%                         count=1; 
%                         count2=0;
%                         AcousFrm(count)=index;
%                         AcousFrq(count)=MaxFreq+1-index1;
%                     elseif size(FreqBins1,2)<freqNThe
%                         AcousFrm=0;
%                         AcousFrq=0;
%                         count=1;
%                         count2=0;
%                         AcousFrm(count)=index;
%                         AcousFrq(count)=MaxFreq+1-index1;
%                         
%                     end
%                 end
%             end
%             count=count+1;
%          end
%     end
% end 
% end             
                    
     

    
    
%     index6=1;                        
%                                     for index15=1:size(WhipFrequency,2)
%                                         if WhipFrequency(index15)==FreStart
%                                             frame(index6)=WhipFrame(index15);
%                                             index6=index6+1;
%                                          end
%                                     end 
%                                     MaxFrmHFrq=max(frame);
%                                     OutEndH(index7)=MaxFrmHFrq;
%                                     if (index6-1)>=2
%                                         index11=1;
%                                         while MaxFrmHFrq
%                                             for index15=1:size(frame,2)
%                                                 if MaxFrmHFrq==frame(index15)
%                                                     frame(index15)=0;
%                                                 end
%                                             end
%                                             MaxFrmHFrq=max(frame);
%                                             zan(index11)=MaxFrmHFrq;
%                                             index11=index11+1;
%                                         end
%                                         MinFrmHFrq=zan(index11-2);
%                                     else
%                                         MinFrmHFrq=max(frame);
%                                     end
%                                     HFrequency=FreStart;
%                                     OutStartH(index7)=MinFrmHFrq;                        
%                                     HFrq(index7)=HFrequency;
%                                     index7=index7+1;
%                                     for index9=1:(index6-1)
%                                         frame(index9)=0;                       
%                                     end    
%                                     index6=1;
% 
%                                     for index15=1:size(WhipFrequency,2)
%                                         if WhipFrequency(index15)==FreEnd
%                                             frame1(index6)=WhipFrame(index15);
%                                             index6=index6+1;
%                                         end
%                                     end
% 
%                                     MaxFrmLFrq=max(frame1);
%                                     OutEndL(index8)=MaxFrmLFrq;
%                                     if (index6-1)>=2
%                                         index11=1;
%                                         while MaxFrmLFrq
%                                             for index15=1:size(frame1,2)
%                                                 if MaxFrmLFrq==frame1(index15)
%                                                     frame1(index15)=0;
%                                                 end
%                                             end
%                                             MaxFrmLFrq=max(frame1);
%                                             zan1(index11)=MaxFrmLFrq;
%                                             index11=index11+1;
%                                         end
%                                          MinFrmLFrq=zan1(index11-2);
%                                     else
%                                         MinFrmLFrq=max(frame1);
%                                     end                       
% 
%                                     LFrequency=FreEnd;
%                                     OutStartL(index8)=MinFrmLFrq;
%                                     LFrq(index8)=LFrequency;
%                                     index8=index8+1;
%                                     for index9=1:(index6-1)
%                                         frame1(index9)=0;                       
%                                     end
%                                     temFreq=0;
%                                    
%                                 end
    
    
%  index6=1;                        
%                                     for index15=1:size(WhipFrequency,2)
%                                         if WhipFrequency(index15)==FreStart
%                                             frame(index6)=WhipFrame(index15);
%                                             index6=index6+1;
%                                          end
%                                     end 
%                                     MaxFrmHFrq=max(frame);
%                                     OutEndH(index7)=MaxFrmHFrq;
%                                     if (index6-1)>=2
%                                         index11=1;
%                                         while MaxFrmHFrq
%                                             for index15=1:size(frame,2)
%                                                 if MaxFrmHFrq==frame(index15)
%                                                     frame(index15)=0;
%                                                 end
%                                             end
%                                             MaxFrmHFrq=max(frame);
%                                             zan(index11)=MaxFrmHFrq;
%                                             index11=index11+1;
%                                         end
%                                         MinFrmHFrq=zan(index11-2);
%                                     else
%                                         MinFrmHFrq=max(frame);
%                                     end
%                                     HFrequency=FreStart;
%                                     OutStartH(index7)=MinFrmHFrq;                        
%                                     HFrq(index7)=HFrequency;
%                                     index7=index7+1;
%                                     for index9=1:(index6-1)
%                                         frame(index9)=0;                       
%                                     end    
%                                     index6=1;
% 
%                                     for index15=1:size(WhipFrequency,2)
%                                         if WhipFrequency(index15)==FreEnd
%                                             frame1(index6)=WhipFrame(index15);
%                                             index6=index6+1;
%                                         end
%                                     end
% 
%                                     MaxFrmLFrq=max(frame1);
%                                     OutEndL(index8)=MaxFrmLFrq;
%                                     if (index6-1)>=2
%                                         index11=1;
%                                         while MaxFrmLFrq
%                                             for index15=1:size(frame1,2)
%                                                 if MaxFrmLFrq==frame1(index15)
%                                                     frame1(index15)=0;
%                                                 end
%                                             end
%                                             MaxFrmLFrq=max(frame1);
%                                             zan1(index11)=MaxFrmLFrq;
%                                             index11=index11+1;
%                                         end
%                                          MinFrmLFrq=zan1(index11-2);
%                                     else
%                                         MinFrmLFrq=max(frame1);
%                                     end                       
% 
%                                     LFrequency=FreEnd;
%                                     OutStartL(index8)=MinFrmLFrq;
%                                     LFrq(index8)=LFrequency;
%                                     index8=index8+1;
%                                     for index9=1:(index6-1)
%                                         frame1(index9)=0;                       
%                                     end
%                                     temFreq=0;
%                                 end
                                             
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