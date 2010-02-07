function examine_all_AE_results(name, I3, T, F, int_thresh, small_events_thresh, xlsfile, match_score)
% check out matching results
% bmp 20090917


ctmp = colormap(gray);
c = flipud(ctmp);



[M,N] = size(I3);
tmax = max(T); %length of signal in seconds
fmax = max(F);


% GET ACOUSTIC EVENTS
[this_results] = xlsread(strcat(name,'_Intensity_Thresh_',num2str(int_thresh),'dB_Small_area_thresh_max_',num2str(small_events_thresh),'.xls'));
allAE = this_results(:,1:4); % all acoustic events
num_AE = size(allAE,1);


% to read in AEs from a specific pattern...
[all_results,all_txt] = xlsread(xlsfile);
num_g = size(all_results,1);
this_results_list = char(all_txt(2:end,1));
    





% get AEs that have been tested against template
notAE = [];
isAE = [];
for ng=1:num_g

    % get specific events
    num_d = (all_results(ng,5));
    if num_d==1
        AE_inds = all_results(ng,6);
    else
        dstring = '';
        for nd = 1:num_d;
            dstring = strcat(dstring,'%d');
        end
        tmp1 = str2mat(all_txt(ng+1,6));
        tmp2 = textscan(tmp1, dstring, 1);
        AE_inds = cell2mat(tmp2);
    end
    num_a = length(AE_inds);
    thisAE = allAE(AE_inds,:);

    % find left-most AE
    [tmp,indL] = min(thisAE(:,1));

    score =  all_results(ng,7);
    if score < match_score
        notAE = [notAE; thisAE(indL,:)];
    else
        isAE = [isAE; thisAE(indL,:)];
    end
end

showImage(c,I3,T,F,1,allAE)
showImage(c,I3,T,F,2,isAE)
showImage(c,I3,T,F,3,notAE)
    
    
    




function show_image(I1,T1,F1,tmax,fmax,fig_num,AE1,this_name,holdonyes)

c = colormap(gray);
c = flipud(c);

if nargin ==6
    
    figure(fig_num), clf, imagesc(T1,F1,I1);
    axis xy; axis tight; colormap(c); view(0,90);
    ylabel('Frequency (kHz)','FontSize',20)
    xlabel('Time (s)','FontSize',20)
    title(this_name,'FontSize',20)
    set(gca,'XTick',[0:10:tmax],'FontSize',20)
        set(gca,'YTick',[0:2000:fmax],'FontSize',20)
        colorbar
   
elseif nargin ==8 
    
    if isempty(AE1)

        figure(fig_num), clf, imagesc(T1,F1,I1);
        axis xy; axis tight; colormap(c); view(0,90);
        ylabel('Frequency (kHz)','FontSize',20)
        xlabel('Time (s)','FontSize',20)
        title(this_name,'FontSize',20)
        set(gca,'XTick',[0:10:tmax],'FontSize',20)
        set(gca,'YTick',[0:2000:fmax],'FontSize',20)
        colorbar
    
    else

        figure(fig_num), clf, imagesc(T1,F1,I1);
        axis xy; axis tight; colormap(c); view(0,90);
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(3,:)],'Color','b')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(4,:); AE1(4,:)],'Color','b')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)], [AE1(3,:); AE1(4,:)],'Color','b')
        figure(fig_num), hold on, line([AE1(1,:)+AE1(2,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(4,:)],'Color','b')
        set(gca,'XTick',[0:10:tmax],'FontSize',20)
        set(gca,'YTick',[0:2000:fmax],'FontSize',20)
        title(this_name,'FontSize',20)
        ylabel('Frequency (kHz)','FontSize',20)
        xlabel('Time (s)','FontSize',20)
        colorbar
    end    
    
    elseif nargin ==9 
    
    if ( ~isempty(AE1) & (holdonyes==1) )

        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(3,:)],'Color','g')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(4,:); AE1(4,:)],'Color','g')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)], [AE1(3,:); AE1(4,:)],'Color','g')
        figure(fig_num), hold on, line([AE1(1,:)+AE1(2,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(4,:)],'Color','g')
        set(gca,'XTick',[0:10:tmax],'FontSize',20)
        set(gca,'YTick',[0:2000:fmax],'FontSize',20)
        title(this_name,'FontSize',20)
        ylabel('Frequency (kHz)','FontSize',20)
        xlabel('Time (s)','FontSize',20)
        colorbar
    end
    if ( ~isempty(AE1) & (holdonyes==2) )

        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(3,:)],'Color','r')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(4,:); AE1(4,:)],'Color','r')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)], [AE1(3,:); AE1(4,:)],'Color','r')
        figure(fig_num), hold on, line([AE1(1,:)+AE1(2,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(4,:)],'Color','r')
        set(gca,'XTick',[0:10:tmax],'FontSize',20)
        set(gca,'YTick',[0:2000:fmax],'FontSize',20)
        title(this_name,'FontSize',20)
        ylabel('Frequency (kHz)','FontSize',20)
        xlabel('Time (s)','FontSize',20)
        colorbar
    end    
end



