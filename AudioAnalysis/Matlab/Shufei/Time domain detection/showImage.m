function showImage(c,I1,T,F,fig_num,AE1)


warning off % this line is included to suppress the warning that MATLAB 
            % flashes everytime it displays a sonogram that's too large to 
            % fit image parameters




% tmin = min(T);
% tmax = max(T);
% tvec = linspace(tmin,tmax,6);
% fmin = min(F);
% fmax = max(F);
% fvec = linspace(fmin,fmax,6);

if nargin == 5
    
    
    figure(fig_num), clf, imagesc(T,F,I1);
    axis xy; axis tight; colormap(c); view(0,90);
    
    title('Spectrogram','FontSize',20)
    ylabel('Frequency (Hz)','FontSize',20)
    xlabel('Time (s)','FontSize',20)
%     set(gca,'XTick',tvec,'FontSize',20)
%     set(gca,'YTick',fvec,'FontSize',20)
%     colorbar
   
elseif nargin == 6 
    
    if isempty(AE1)

        figure(fig_num), clf, imagesc(T,F,I1);
        axis xy; axis tight; colormap(c); view(0,90);
        title('Spectrogram','FontSize',20)
        ylabel('Frequency (Hz)','FontSize',20)
       xlabel('Time (s)','FontSize',20)
%         set(gca,'XTick',tvec,'FontSize',20)
%         set(gca,'YTick',fvec,'FontSize',20)
%         colorbar
    
    else
        AE1 = AE1';
        figure(fig_num), clf, imagesc(T,F,I1);
        axis xy; axis tight; colormap(c); view(0,90);
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(3,:)],'Color','b')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(4,:); AE1(4,:)],'Color','b')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)], [AE1(3,:); AE1(4,:)],'Color','b')
        figure(fig_num), hold on, line([AE1(1,:)+AE1(2,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(4,:)],'Color','b')
%         set(gca,'XTick',tvec,'FontSize',20)
%         set(gca,'YTick',fvec,'FontSize',20)
      title('Acoustic Events on Spectrogram','FontSize',20)
       ylabel('Frequency (Hz)','FontSize',20)
       xlabel('Time (s)','FontSize',20)
   %      colorbar
    end    
       
end
