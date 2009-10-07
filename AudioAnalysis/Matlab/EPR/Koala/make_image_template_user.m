function template_name = make_image_template_user(name, file_path_audio, file_path_acoustic_events, results_path, int_thresh, small_events_thresh)
% Create image template
% User has control over template size and which AEs should be
% included/discarded
% bmp 20090917



working_path = pwd;



% OTHER PARAMETERS - hardcoded for the moment
window = 512; % hamming window using 512 samples
noverlap = round(0.5*window); % 50% overlap between frames
nfft = 256*2-1; % yield 512 frequency bins



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
show_image(I2,T,F,tmax,fmax,2,allAE')



% GENERATE TEMPLATE
% user selects four points of template
user_entry1 = input('Enter time start & end (in seconds), and freq start and end (in Hz), e.g. [2 5 1590 2980];\n');

% template parameters
tstart = user_entry1(1);
tend = user_entry1(2);
fbottom = user_entry1(3);
ftop = user_entry1(4);

% show template
[tmp2, left] = min(abs(T-tstart));
[tmp2, right] = min(abs(T-tend));
[tmp2, bottom] = min(abs(F-fbottom));
[tmp2, top] = min(abs(F-ftop));
thisT1 = T(left:right);
thisF1 = F(bottom:top);
I3_seg = I3(bottom:top,left:right);
% figure(2), clf, imagesc(T(left:right),F(bottom:top),I3_seg), colormap(jet), axis xy

% get AEs in specified time and frequency range
indAE = find( (allAE(:,1) >= tstart) & ((allAE(:,1)+allAE(:,2)) <= tend) & (allAE(:,3) >= fbottom) & (allAE(:,4) <= ftop) );
numAE = length(indAE);

if numAE==0
    disp('No acoustic events; template not saved')
else
    AE = allAE(indAE,:);
       
    % user selects which AEs to keep
    keepAE = [];
    indAE2 = [];
    for aa=1:numAE
        show_image_seg(I3_seg,thisT1,thisF1,2,AE',0)
        show_image_seg(I3_seg,thisT1,thisF1,2,AE(aa,:)',1)
        user_entry2 = input('Keep acoustic event (if yes hit enter/if no type n)?','s');
        if ~issame(user_entry2,'n')
            keepAE = [keepAE; AE(aa,:)];
            indAE2 = [indAE2, indAE(aa)];
        end
    end
    
    % resize template to fit snuggly around all AEs
    thisT = T(left:right);
    thisF = F(bottom:top);

    tmp = min(keepAE(:,1));
    [tmp2, tleft] = min(abs(thisT-tmp));
    tmp = max(keepAE(:,1) + keepAE(:,2));
    [tmp2, tright] = min(abs(thisT-tmp));
    tmp = min(keepAE(:,3));
    [tmp2, tbottom] = min(abs(thisF-tmp));
    tmp = max(keepAE(:,4));
    [tmp2, ttop] = min(abs(thisF-tmp));
    Itemplate = zeros(length(tbottom:ttop),length(tleft:tright));
    Itemplate = I3_seg(tbottom:ttop,tleft:tright);
    thisT = thisT(tleft:tright);
    thisF = thisF(tbottom:ttop);
    show_image_seg(Itemplate,thisT,thisF,3,keepAE',0)

    

    % store template
        
    % starting index - left most AE - used in template name
    [tmp1, tmp2] = min(keepAE(:,1)-tstart); 
    leftAE_ind = indAE2(tmp2);

    template = {};
    template_name = strcat(name,'_Intensity_Thresh_',num2str(int_thresh),'dB_Small_area_thresh_max_',num2str(small_events_thresh),'_template',num2str(leftAE_ind));
    template.filename = template_name;
    template.AE = keepAE;
    template.I = Itemplate;
    template.T = thisT;
    template.F = thisF;
    cd(results_path)
    save(strcat(template_name,'.mat'),'template')
    cd(working_path)

end








function show_image(I1,T1,F1,tmax,fmax,fig_num,AE1,holdonyes)

c = colormap(gray);
c = flipud(c);

if nargin ==6
    
    figure(fig_num), clf, imagesc(T1,F1,I1);
    axis xy; axis tight; colormap(c); view(0,90);
    ylabel('Frequency (kHz)','FontSize',20)
    xlabel('Time (s)','FontSize',20)
    set(gca,'XTick',[0:10:tmax],'FontSize',20)
    set(gca,'YTick',[0:2000:fmax],'FontSize',20)
    colorbar
   
elseif nargin ==7 
    
    if isempty(AE1)

        figure(fig_num), clf, imagesc(T1,F1,I1);
        axis xy; axis tight; colormap(c); view(0,90);
        ylabel('Frequency (kHz)','FontSize',20)
        xlabel('Time (s)','FontSize',20)
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
        title('Filtered Image with Marqueed Acoustic Events AE1','FontSize',20)
        ylabel('Frequency (kHz)','FontSize',20)
        xlabel('Time (s)','FontSize',20)
        colorbar
    end    
    
    elseif nargin ==8 
    
    if ( ~isempty(AE1) & (holdonyes==1) )

        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(3,:)],'Color','r')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(4,:); AE1(4,:)],'Color','r')
        figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)], [AE1(3,:); AE1(4,:)],'Color','r')
        figure(fig_num), hold on, line([AE1(1,:)+AE1(2,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(4,:)],'Color','r')
        set(gca,'XTick',[0:10:tmax],'FontSize',20)
        set(gca,'YTick',[0:2000:fmax],'FontSize',20)
        title('Filtered Image with Marqueed Acoustic Events AE1','FontSize',20)
        ylabel('Frequency (kHz)','FontSize',20)
        xlabel('Time (s)','FontSize',20)
        colorbar
    end    
end


function show_image_seg(I1,T1,F1,fig_num,AE1,holdonyes)

c = colormap(gray);
c = flipud(c);

   
if isempty(AE1)

    figure(fig_num), clf, imagesc(T1,F1,I1);
    axis xy; axis tight; colormap(c); view(0,90);
end
if (holdonyes==0)

    figure(fig_num), clf, imagesc(T1,F1,I1);
    axis xy; axis tight; colormap(c); view(0,90);
    figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(3,:)],'Color','g')
    figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(4,:); AE1(4,:)],'Color','g')
    figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)], [AE1(3,:); AE1(4,:)],'Color','g')
    figure(fig_num), hold on, line([AE1(1,:)+AE1(2,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(4,:)],'Color','g')
else
    figure(fig_num), hold on
    figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(3,:)],'Color','r')
    figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)+AE1(2,:)], [AE1(4,:); AE1(4,:)],'Color','r')
    figure(fig_num), hold on, line([AE1(1,:); AE1(1,:)], [AE1(3,:); AE1(4,:)],'Color','r')
    figure(fig_num), hold on, line([AE1(1,:)+AE1(2,:); AE1(1,:)+AE1(2,:)], [AE1(3,:); AE1(4,:)],'Color','r')
end    
    
   

