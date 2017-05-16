% function showImage1(c,I15,IstdH1,IstdV1,IstdH2,IstdV2,IstdH3,IstdV3,T,F,fig_num,o)
function showImage1(c,I15,IstdH1,IstdV1,startpoint,endpoint,FreqBins,OutStart1,OutEnd1,OutStart2,OutEnd2,SFrq,EFrq,T,F,fig_num,o)
%,IstdH2,IstdV2,IstdH3,IstdV3

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
            title('Spectrogram with markers based on Std','FontSize',20);
            ylabel('Frequency (Hz)','FontSize',20);
            xlabel('Time (s)','FontSize',20);     
            hold on;
        case 2
            title('Spectrogram with markers based on dB','FontSize',20);
            ylabel('Frequency (Hz)','FontSize',20);
            xlabel('Time (s)','FontSize',20);     
            hold on;
    end
       
    IstdV1_SamplesLength  = size(IstdV1,2);
%    IstdV2_SamplesLength  = size(IstdV2,2);
%     IstdV3_SamplesLength  = size(IstdV2,2);
    
    IstdH1_Frquencybinnumber=size(I15,1);
%       IstdH2_Frquencybinnumber=size(I15,1);
%      IstdH3_Frquencybinnumber=size(I15,1);
     
     IstdH1 = (IstdH1 /IstdH1_Frquencybinnumber) * max(F);
%      IstdH2 = (IstdH2 /IstdH2_Frquencybinnumber) * max(F);
%      IstdH3= (IstdH3 /IstdH3_Frquencybinnumber) * max(F);
%      
      IstdV1 = (IstdV1 / IstdV1_SamplesLength) * max(T) ;
%      IstdV2 = (IstdV2 / IstdV2_SamplesLength) * max(T) ;
%     IstdV3= (IstdV3 / IstdV3_SamplesLength) * max(T) ;
%     
FreqBins= (FreqBins /IstdH1_Frquencybinnumber) * max(F);
SFrq=(SFrq /IstdH1_Frquencybinnumber) * max(F);
EFrq=(EFrq /IstdH1_Frquencybinnumber) * max(F);
    % cycle through each plot 
     numGroups1 = size(IstdV1,1); 
     numGroups2 = size(IstdV2,1); 
     numGroups3= size(IstdV3,1); 
    switch (o)
        case 1
            colors = {'xb';'xg';'xy';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr'};%  red dots represent the first group which locates at the bottom of the NdB/Nsig
                                      % ......
                                      %  green dots represent the last
                                      %  group which locates at the top
                                      %  of the NdB / Nsig
   
            if (length(colors) < numGroups1)
                error('Not enough colors!');
            end   
            groupIndex1=1;
%             groupIndex1=2;
%             for groupIndex =(numGroups1-1):-1:1
            for groupIndex =numGroups1:-1:1   
                for index=1:IstdV1_SamplesLength
                    if (IstdV1(groupIndex, index)~=0)&&(IstdH1(groupIndex, index)~=0)
                        plot(IstdV1(groupIndex, index), IstdH1(groupIndex, index), char(colors(groupIndex1)));
                    end
                end
                % end for (group)
               groupIndex1=groupIndex1+1;
            end
            %show whistle line
            for groupIndex =numGroups1:-1:1
                for indexFreq=size(FreqBins,2):-1:1
                    for index1=1:size(startpoint,2)
                        if startpoint(indexFreq,index1)~=0
                            for index=1:IstdV1_SamplesLength
                                if (index==startpoint(indexFreq,index1)) && (IstdH1(groupIndex,index)==FreqBins(indexFreq))
                                    line([IstdV1(groupIndex,index),IstdV1(groupIndex,endpoint(indexFreq,index1))],[IstdH1(groupIndex,index),IstdH1(groupIndex,index)],'Color','g');
                                end
                            end
                        end
                    end
                end
            end
            %show whistle clusters
            for groupIndex=numGroups1:-1:1
                for index=1:size(OutStart1,2)
                    for index1=1:IstdV1_SamplesLength
                        if (index1==OutStart1(index)) &&(IstdH1(groupIndex,index1)==SFrq(index))
                            line([IstdV1(groupIndex,OutStart1(index)),IstdV1(groupIndex,OutStart2(index))],[SFrq(index),EFrq(index)],'color','g');
                        elseif (index1==OutEnd1(index))&&(IstdH1(groupIndex,index1)==SFrq(index))
                            line([IstdV1(groupIndex,OutEnd1(index)),IstdV1(groupIndex,OutEnd2(index))],[SFrq(index),EFrq(index)],'color','g');
                        end
                    end
                end
            end
% % % % % %             

%             if (length(colors) < numGroups2)
%                 error('Not enough colors!');
%             end   
%             groupIndex1=1;
% %             groupIndex1=2;
%             for groupIndex =numGroups2:-1:1
% %                  for groupIndex =(numGroups2-1):-1:1
%                 for index=1:IstdV2_SamplesLength
%                     if (IstdV2(groupIndex, index)~=0)&&(IstdH2(groupIndex, index)~=0)
%                         plot(IstdV2(groupIndex, index), IstdH2(groupIndex, index), char(colors(groupIndex1)));
%                     end
%                 end
%                 % end for (group)
%                groupIndex1=groupIndex1+1;
%             end
%             
%             if (length(colors) < numGroups3)
%                 error('Not enough colors!');
%             end   
%             groupIndex1=1;
% %             groupIndex1=2;
%             for groupIndex =numGroups3:-1:1
% %            for groupIndex =(numGroups3-1):-1:1
%                 for index=1:IstdV3_SamplesLength
%                     if (IstdV3(groupIndex, index)~=0)&&(IstdH3(groupIndex, index)~=0)
%                         plot(IstdV3(groupIndex, index), IstdH3(groupIndex, index), char(colors(groupIndex1)));
%                     end
%                 end
%                 % end for (group)
%                groupIndex1=groupIndex1+1;
%             end
          


            hold off
        case 2
            colors = {'xb';'xg';'xy';'xr'; 'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr';'xr'};%  red dots represent the first group which locates at the bottom of the NdB/Nsig
                                      % ......
                                      %  green dots represent the last
                                      %  group which locates at the top
                                      %  of the NdB / Nsig
                                                                          

            if (length(colors) < numGroups1)
                error('Not enough colors!');
            end   
            for groupIndex = 1: numGroups1
%             for groupIndex = 2: numGroups1
                for index=1:IstdV1_SamplesLength 
                    if (IstdV1(groupIndex, index)~=0)&&(IstdH1(groupIndex, index)~=0)
                        plot(IstdV1(groupIndex, index), IstdH1(groupIndex, index), char(colors(groupIndex)));
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