function showComponents(c,I15, AcousticFrequency,AcousticFrame,startpoint,endpoint,FreqBins,OutStart1,OutEnd1,OutStart2,OutEnd2,SFrq,EFrq,AcousSig,frequency,timeframe,OutStartH,OutEndH,OutStartL,OutEndL,HFrq,LFrq,AcousSigWhip,frequencyWhip,timeframeWhip,OutStartHW,OutEndHW,OutStartLW,OutEndLW,HFrqW,LFrqW,T,F,fig_num,o)
%



%this function is used to show components in one spectrogram
warning off % this line is included to suppress the warning that MATLAB 
            % flashes everytime it displays a sonogram that's too large to 
            % fit image parameters

 
    figure(fig_num);
    clf;
    hold on;
    imagesc(T,F,I15);
     axis xy; 
    axis tight; 
   colormap(c); 
    view(0,90);
    switch(o)
        case 1
            title('Spectrogram','FontSize',20);
            ylabel('Frequency (Hz)','FontSize',20);
            xlabel('Time (s)','FontSize',20);     
            hold on;
        case 2
            title('Spectrogram','FontSize',20);
            ylabel('Frequency (Hz)','FontSize',20);
            xlabel('Time (s)','FontSize',20);     
            hold on;
    end
    %set up parameters for showing whistle    
     AcousticFrame_SamplesLength = size(AcousticFrame,2);    
     AcousticFrequency_Frquencybinnumber=size(I15,1);     
     AcousticFrequency = (AcousticFrequency /AcousticFrequency_Frquencybinnumber) * max(F); 
     AcousticFrame = (AcousticFrame / AcousticFrame_SamplesLength) * max(T) ;

     FreqBins= (FreqBins /AcousticFrequency_Frquencybinnumber) * max(F);
     SFrq=(SFrq /AcousticFrequency_Frquencybinnumber) * max(F);
     EFrq=(EFrq /AcousticFrequency_Frquencybinnumber) * max(F);
     
     %set up parameters for showing block
     timeframe_SamplesLength  = size(AcousSig,2);    
     frequency_Frquencybinnumber=size(I15,1);     
     frequency = (frequency /frequency_Frquencybinnumber) * max(F);     
     timeframe = (timeframe / timeframe_SamplesLength) * max(T) ;
      
      OutStartH=(OutStartH / timeframe_SamplesLength) * max(T) ;
      OutEndH=(OutEndH / timeframe_SamplesLength) * max(T) ;
      OutStartL=(OutStartL/ timeframe_SamplesLength) * max(T) ;
      OutEndL=(OutEndL/ timeframe_SamplesLength) * max(T) ; 
    
      HFrq=(HFrq/frequency_Frquencybinnumber) * max(F);
     LFrq=(LFrq /frequency_Frquencybinnumber) * max(F);
     
     %set up parameters for showing whip
     timeframeWhip_SamplesLength  = size(AcousSigWhip,2);    
     frequencyWhip_Frquencybinnumber=size(I15,1);     
     frequencyWhip = (frequencyWhip /frequencyWhip_Frquencybinnumber) * max(F);     
     timeframeWhip = (timeframeWhip / timeframeWhip_SamplesLength) * max(T) ;
      
      OutStartHW=(OutStartHW / timeframeWhip_SamplesLength) * max(T) ;
      OutEndHW=(OutEndHW / timeframeWhip_SamplesLength) * max(T) ;
      OutStartLW=(OutStartLW/ timeframeWhip_SamplesLength) * max(T) ;
      OutEndLW=(OutEndLW/ timeframeWhip_SamplesLength) * max(T) ; 
    
      HFrqW=(HFrqW/frequencyWhip_Frquencybinnumber) * max(F);
      LFrqW=(LFrqW /frequencyWhip_Frquencybinnumber) * max(F); 
     
     
    % cycle through each plot 
     numGroups1 = size(timeframe,1); 

    switch (o)
        case 1
            colors = {'xy';'xg';'xy';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr'};%  red dots represent the first group which locates at the bottom of the NdB/Nsig
                                      % ......
                                      %  green dots represent the last
                                      %  group which locates at the top
                                      %  of the NdB / Nsig
   
            if (length(colors) < numGroups1)
                error('Not enough colors!');
            end   
            groupIndex1=1;

            for groupIndex =numGroups1:-1:1   
                for index=1:AcousticFrame_SamplesLength
                    if (AcousticFrame(groupIndex, index)~=0)&&(AcousticFrequency(groupIndex, index)~=0)
                        plot(AcousticFrame(groupIndex, index),AcousticFrequency(groupIndex, index), char(colors(groupIndex1)));
                    end
                end
                % end for (group)
               groupIndex1=groupIndex1+1;
            end
            
            groupIndex1=1;

            for groupIndex =numGroups1:-1:1   
                for index=1:size(timeframe,2)  
                    if (timeframe(groupIndex, index)~=0)&&(frequency(groupIndex, index)~=0)
                        plot(timeframe(groupIndex, index), frequency(groupIndex, index), char(colors(groupIndex1)));
                    end
                end               
            groupIndex1=groupIndex1+1;
            end
            
            groupIndex1=1;

            for groupIndex =numGroups1:-1:1   
                for index=1:size(timeframeWhip,2)  
                    if (timeframeWhip(groupIndex, index)~=0)&&(frequencyWhip(groupIndex, index)~=0)
                        plot(timeframeWhip(groupIndex, index), frequencyWhip(groupIndex, index), char(colors(groupIndex1)));
                    end
                end               
            groupIndex1=groupIndex1+1;
            end
            
            %show whistle line
            for groupIndex =numGroups1:-1:1
                for indexFreq=size(FreqBins,2):-1:1
                    for index1=1:size(startpoint,2)
                        if startpoint(indexFreq,index1)~=0
                            for index=1:AcousticFrame_SamplesLength
                                if (index==startpoint(indexFreq,index1)) && (AcousticFrequency(groupIndex,index)==FreqBins(indexFreq))
                                    line([AcousticFrame(groupIndex,index),AcousticFrame(groupIndex,endpoint(indexFreq,index1))],[AcousticFrequency(groupIndex,index),AcousticFrequency(groupIndex,index)],'LineWidth',2,'Color','b');
                                end
                            end
                        end
                    end
                end
            end
            %show whistle clusters
            for groupIndex=numGroups1:-1:1
                for index=1:size(OutStart1,2)
                    for index1=1:AcousticFrame_SamplesLength
                        if (index1==OutStart1(index)) &&(AcousticFrequency(groupIndex,index1)==SFrq(index))
                            line([AcousticFrame(groupIndex,OutStart1(index)),AcousticFrame(groupIndex,OutStart2(index))],[SFrq(index),EFrq(index)],'LineWidth',2,'color','b');
                        elseif (index1==OutEnd1(index))&&(AcousticFrequency(groupIndex,index1)==SFrq(index))
                            line([AcousticFrame(groupIndex,OutEnd1(index)),AcousticFrame(groupIndex,OutEnd2(index))],[SFrq(index),EFrq(index)],'LineWidth',2,'color','b');
                        end
                    end
                end
            end
            
            
            %show block clusters
            for groupIndex=numGroups1:-1:1
                for index1=1:size(OutStartH,2)
                    if OutStartH(index1)<=OutStartL(index1)
                        line([OutStartH(index1),OutStartL(index1)],[HFrq(index1),LFrq(index1)],'LineWidth',2,'Color','w');
                        line([OutStartH(index1),OutEndH(index1)],[HFrq(index1),HFrq(index1)],'LineWidth',2,'color','w');
                        line([OutStartL(index1),OutEndL(index1)],[LFrq(index1),LFrq(index1)],'LineWidth',2,'color','w');
                        line([OutEndH(index1),OutEndL(index1)],[HFrq(index1),LFrq(index1)],'LineWidth',2,'color','w');
                    else
                        line([OutStartL(index1),OutStartH(index1)],[LFrq(index1),HFrq(index1)],'LineWidth',2,'Color','w');
                        line([OutStartH(index1),OutEndH(index1)],[HFrq(index1),HFrq(index1)],'LineWidth',2,'color','w');
                        line([OutStartL(index1),OutEndL(index1)],[LFrq(index1),LFrq(index1)],'LineWidth',2,'color','w');
                        line([OutEndL(index1),OutEndH(index1)],[LFrq(index1),HFrq(index1)],'LineWidth',2,'color','w');
                    end
                end
            end
            
            %show whip clusters
            for groupIndex=numGroups1:-1:1
                for index1=1:size(OutStartHW,2)
                    if OutStartHW(index1)<=OutStartLW(index1)
                        line([OutStartHW(index1),OutStartLW(index1)],[HFrqW(index1),LFrqW(index1)],'LineWidth',2,'Color','g');
                        line([OutStartHW(index1),OutEndHW(index1)],[HFrqW(index1),HFrqW(index1)],'LineWidth',2,'color','g');
                        line([OutStartLW(index1),OutEndLW(index1)],[LFrqW(index1),LFrqW(index1)],'LineWidth',2,'color','g');
                        line([OutEndHW(index1),OutEndLW(index1)],[HFrqW(index1),LFrqW(index1)],'LineWidth',2,'color','g');
                    else
                        line([OutStartLW(index1),OutStartHW(index1)],[LFrqW(index1),HFrqW(index1)],'LineWidth',2,'Color','g');
                        line([OutStartHW(index1),OutEndHW(index1)],[HFrqW(index1),HFrqW(index1)],'LineWidth',2,'color','g');
                        line([OutStartLW(index1),OutEndLW(index1)],[LFrqW(index1),LFrqW(index1)],'LineWidth',2,'color','g');
                        line([OutEndLW(index1),OutEndHW(index1)],[LFrqW(index1),HFrqW(index1)],'LineWidth',2,'color','g');
                    end
                end
            end

            hold off
        case 2
            colors = {'xy';'xg';'xy';'xr'; 'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr'};%  red dots represent the first group which locates at the bottom of the NdB/Nsig
                                      % ......
                                      %  green dots represent the last
                                      %  group which locates at the top
                                      %  of the NdB / Nsig
                                                                          

            if (length(colors) < numGroups1)
                error('Not enough colors!');
            end   
            for groupIndex = 1: numGroups1
%             for groupIndex = 2: numGroups1
                for index=1:AcousticFrame_SamplesLength 
                    if (AcousticFrame(groupIndex, index)~=0)&&(AcousticFrequency(groupIndex, index)~=0)
                        plot(AcousticFrame(groupIndex, index),AcousticFrequency(groupIndex, index), char(colors(groupIndex)));
                    end
                end
            % end for (group)
            end
            
            if (length(colors) < numGroups2)
                error('Not enough colors!');
            end   
            for groupIndex = 1: numGroups2
%            for groupIndex = 2: numGroups2
                for index=1:IstdV2_SamplesLength 
                    if (IstdV2(groupIndex, index)~=0)&&(IstdH2(groupIndex, index)~=0)
                        plot(IstdV2(groupIndex, index), IstdH2(groupIndex, index), char(colors(groupIndex)));
                    end
                end
            % end for (group)
            end
            
            if (length(colors) < numGroups3)
                error('Not enough colors!');
            end   
            for groupIndex = 1: numGroups3
%             for groupIndex = 2: numGroups3
                for index=1:IstdV3_SamplesLength 
                    if (IstdV3(groupIndex, index)~=0)&&(IstdH3(groupIndex, index)~=0)
                        plot(IstdV3(groupIndex, index), IstdH3(groupIndex, index), char(colors(groupIndex)));
                    end
                end
            % end for (group)
            end
            hold off
    end




end