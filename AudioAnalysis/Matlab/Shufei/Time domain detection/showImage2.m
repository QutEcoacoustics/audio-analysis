% function showImage1(c,I15,IstdH1,IstdV1,IstdH2,IstdV2,IstdH3,IstdV3,T,F,fig_num,o)
function showImage2(c,I15,AcousSig,IstdH1,IstdV1,OutStartH,OutEndH,OutStartL,OutEndL,HFrq,LFrq,T,F,fig_num,o)
%

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
     IstdV1_SamplesLength  = size(AcousSig,2);
  
    
    IstdH1_Frquencybinnumber=size(I15,1);
 
     
     IstdH1 = (IstdH1 /IstdH1_Frquencybinnumber) * max(F);
   
%      
      IstdV1 = (IstdV1 / IstdV1_SamplesLength) * max(T) ;
      OutStartH=(OutStartH / IstdV1_SamplesLength) * max(T) ;
      OutEndH=(OutEndH / IstdV1_SamplesLength) * max(T) ;
      OutStartL=(OutStartL/ IstdV1_SamplesLength) * max(T) ;
      OutEndL=(OutEndL/ IstdV1_SamplesLength) * max(T) ;
 
    
   HFrq=(HFrq/IstdH1_Frquencybinnumber) * max(F);
   LFrq=(LFrq /IstdH1_Frquencybinnumber) * max(F);
    % cycle through each plot 
     numGroups1 = size(IstdV1,1); 

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
                for index=1:size(IstdV1,2)  
                    if (IstdV1(groupIndex, index)~=0)&&(IstdH1(groupIndex, index)~=0)
                        plot(IstdV1(groupIndex, index), IstdH1(groupIndex, index), char(colors(groupIndex1)));
                    end
                end               
            groupIndex1=groupIndex1+1;
            end
            %show whistle line
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
            hold off
            
            
            
            
            
            
            
%             for groupIndex =numGroups1:-1:1
%                 for index1=1:size(OutStartH,2)
%                         if OutStartH(index1)~=0
%                             for index=1:size(IstdV1,2) 
% %                                 try
%                                 if (IstdV1(groupIndex, index)==OutStartH(index1)) && (IstdH1(groupIndex,index)==HFrq(index1))
%                                     if OutStartH(index1)<=OutStartL(index1)
%                                           line([OutStartH(index1),OutStartL(index1)],[HFrq(index1),LFrq(index1)],'Color','g');
%                                           line([OutStartH(index1),OutEndH(index1)],[HFrq(index1),HFrq(index1)],'color','g');
%                                           line([OutStartL(index1),OutEndL(index1)],[LFrq(index1),LFrq(index1)],'color','g');
%                                           line([OutEndH(index1),OutEndL(index1)],[HFrq(index1),LFrq(index1)],'color','g');
%                                         
% %                                         line([IstdV1(groupIndex,OutStartH(index1)),IstdV1(groupIndex,OutStartL(index1))],[HFrq(index1),LFrq(index1)],'Color','g');
% %                                         line([IstdV1(groupIndex,OutStartH(index1)),IstdV1(groupIndex,OutEndH(index1))],[HFrq(index1),HFrq(index1)],'color','g');
% %                                         line([IstdV1(groupIndex,OutStartL(index1)),IstdV1(groupIndex,OutEndL(index1))],[LFrq(index1),LFrq(index1)],'color','g');
% %                                         line([IstdV1(groupIndex,OutEndH(index1)),IstdV1(groupIndex,OutEndL(index1))],[HFrq(index1),LFrq(index1)],'color','g');
%                                     else
%                                         line([OutStartL(index1),OutStartH(index1)],[LFrq(index1),HFrq(index1)],'Color','g');
%                                         line([OutStartH(index1),OutEndH(index1)],[HFrq(index1),HFrq(index1)],'color','g');
%                                         line([OutStartL(index1),OutEndL(index1)],[LFrq(index1),LFrq(index1)],'color','g');
%                                         line([OutEndL(index1),OutEndH(index1)],[LFrq(index1),HFrq(index1)],'color','g');
% %                                         line([IstdV1(groupIndex,OutStartL(index1)),IstdV1(groupIndex,OutStartH(index1))],[LFrq(index1),HFrq(index1)],'Color','g');
% %                                         line([IstdV1(groupIndex,OutStartH(index1)),IstdV1(groupIndex,OutEndH(index1))],[HFrq(index1),HFrq(index1)],'color','g');
% %                                         line([IstdV1(groupIndex,OutStartL(index1)),IstdV1(groupIndex,OutEndL(index1))],[LFrq(index1),LFrq(index1)],'color','g');
% %                                         line([IstdV1(groupIndex,OutEndL(index1)),IstdV1(groupIndex,OutEndH(index1))],[LFrq(index1),HFrq(index1)],'color','g');
%                                     end
%                                 end
% %                                 catch problem
% %                                     a = problem;
%                             end
%                         
%                         end                    
%                    end
%             end
%             hold off
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
