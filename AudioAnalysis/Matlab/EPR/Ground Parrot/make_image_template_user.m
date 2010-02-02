function template_name = make_image_template_user
% Create image template
% User has control over template size and which AEs should be
% included/discarded
% bmp 20091102

% image name and initialised template name
name = 'GParrots_JB2_20090607-173000.wav_minute_3.wav';
template_name = '';

% parameters
w = 5; 
int_thresh = 3; 
small_events_thresh = 100;
ctmp = colormap(gray); c = flipud(ctmp); 

% compute spectrogram
addpath('../../Common')
[y,fs,I1,F,T,] = wavToSpectrogram(name);
[M,N] = size(I1);

% wiener filtering
I2 = wiener2(I1, [w w]);

% remove subband noise
I3 = withoutSubbandModeIntensities(I2);



% GET ACOUSTIC EVENTS
[this_results] = xlsread(strcat(name,'_Intensity_Thresh_',num2str(int_thresh),'dB_Small_area_thresh_max_',num2str(small_events_thresh),'.xls'));
allAE = this_results(:,1:4); % all acoustic events
showImage(c,I3,T,F,1,allAE);



% GENERATE TEMPLATE
% user selects four points of template
user_entry1 = input('Enter time start & end (in seconds), and freq start and end (in Hz), e.g. [4.5 9 3500 5050];\n');

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
        showImage(c,I3_seg,thisT1,thisF1,2,AE)
        showImage(c,I3_seg,thisT1,thisF1,3,AE(aa,:))
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
    thisT2 = thisT(tleft:tright);
    thisF2 = thisF(tbottom:ttop);
    thisT = thisT2;
    thisF = thisF2;
    showImage(c,Itemplate,thisT,thisF,4,keepAE)
    
    

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
    save(strcat(template_name,'.mat'),'template')
    
end







