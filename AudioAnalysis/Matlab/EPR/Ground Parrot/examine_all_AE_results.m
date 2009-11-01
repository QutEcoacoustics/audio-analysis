function examine_all_AE_results(name, file_path_audio, file_path_acoustic_events, results_path, int_thresh, small_events_thresh, xlsfile, match_score)
% check out matching results
% bmp 20090917


working_path = pwd;


% OTHER PARAMETERS - hardcoded for the moment
window = 512; % hamming window using 512 samples
noverlap = round(0.5*window); % 50% overlap between frames
nfft = 256*2; % yield 512 frequency bins


% GENERATE SPECTROGRAM
% read audio data
cd(file_path_audio)
[y, fs, nbits, opts] = wavread(name);
cd(working_path)
leny = length(y);

% get original image
[S,F,T,P] = spectrogram(y,window,noverlap,nfft,fs);
I1 = 10*log10(abs(P)); % convert amplitude to dB
% variables below are for plotting - later in code
[M,N] = size(I1);
tmax = length(y)/fs; %length of signal in seconds
fmax = 11025;
T = linspace(0,tmax,N);
F = linspace(0,fmax,M);
% wiener filtering
w = 5; %window length of wxw window used in wiener filtering
I2 = wiener2(I1, [w w]);
% remove subband noise
I3 = subband_mode_intensities(I2);


% GET ACOUSTIC EVENTS
cd(file_path_acoustic_events)
[this_results] = xlsread(strcat(name,'_Intensity_Thresh_',num2str(int_thresh),'dB_Small_area_thresh_max_',num2str(small_events_thresh),'.xls'));
cd(working_path)
allAE = this_results(:,1:4); % all acoustic events
num_AE = size(allAE,1);


% to read in AEs from a specific pattern...
cd(results_path)
[all_results,all_txt] = xlsread(xlsfile);
cd(working_path)
num_g = size(all_results,1);
this_results_list = char(all_txt(2:end,1));
    





% get AEs that have been tested against template
notAE = [];
isAE = [];
for ng=1:num_g

    minlen = min(length(name),length(this_results_list(ng,1:end-1)));

    if issame(this_results_list(ng,1:minlen),name(1:minlen))

        % get specific events
        num_d = (all_results(ng,6));
        if num_d==1
            AE_inds = all_results(ng,7);
        else
            dstring = '';
            for nd = 1:num_d;
                dstring = strcat(dstring,'%d');
            end
            tmp1 = str2mat(all_txt(ng+1,8));
            tmp2 = textscan(tmp1, dstring, 1);
            AE_inds = cell2mat(tmp2);
        end
        num_a = length(AE_inds);
        thisAE = allAE(AE_inds,:);

        score =  all_results(ng,8);
        if score < match_score
            notAE = [notAE; thisAE(1,:)];
        else
            isAE = [isAE; thisAE(1,:)];
        end
    end
end


show_image(I2,T,F,tmax,fmax,6,allAE',name)
show_image(I2,T,F,tmax,fmax,6,isAE',name,1)
show_image(I2,T,F,tmax,fmax,6,notAE',name,2)
    
    
    




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



