function do_wiener_snr_tests
% This code tests snrs resulting from different w selections
% 
% Test applied to BAC2 recordings in 
% G:\Birgit\Sensor\image_proc\Bac2 - audio
%
% Sensor Networks Project
% Birgit Planitz
% 20090417




% save parameters
xls_file = 'wiener_tests3.xls';
xlswrite(xls_file, {'Image', 'Year', 'Month', 'Day', 'Hour', 'Minute', 'Second','min_orig','max_orig','Q_orig','min_w3','max_w3','Q_w3','min_w5','max_w5','Q_w5','min_w7','max_w7','Q_w7'}, 'A3:S3');

cntr = 3; % cntr for rows in excel file



% open excel timeslot sheet, which contains sorted data
cd 'G:\Birgit\Sensor\image_proc\Bac2 - analyses'
sorted_files = xlsread('BAC2 sorted files.xls');
[tmp sorted_filenames] = xlsread('BAC2 sorted files.xls','D2:D678');
cd 'G:\Birgit\Sensor\image_proc\Acoustic Analysis - Brandes - 20090417\'
num_files = length(sorted_filenames);



timeslots = linspace(0,23.5,48)*60;
this_timeslot = 1;
last_timeslot = 1;
ts_cntr = 0;
max_ts_cntr = 5;

for nf = 1:num_files
    
    disp(nf/num_files)
    
    this_filename = char(sorted_filenames(nf));
    this_filename = this_filename(1:end-4);
    
    
    year = str2num(this_filename(6:9));
    month = str2num(this_filename(10:11));
    day = str2num(this_filename(12:13));
    hour = str2num(this_filename(15:16));
    minute = str2num(this_filename(17:18));
    second = str2num(this_filename(19:20));

    
    this_timeframe = hour*60 + minute;
    [timelocs] = find((timeslots - this_timeframe)>=0);
    if (isempty(timelocs))
        this_timeslot = 48;
    else
        this_timeslot = timelocs(1)-1;
    end
    
    if (this_timeslot==last_timeslot)
        if (ts_cntr < max_ts_cntr)
    
            % read audio data
            cd 'G:\Birgit\Sensor\image_proc\Bac2 - audio'
            [y, fs, nbits, opts] = wavread(strcat(this_filename,'.wav'));
            cd 'G:\Birgit\Sensor\image_proc\Acoustic Analysis - Brandes - 20090417'


            % STEP 1: GENERATE SPECTROGRAM
            window = 512; % hamming window using 512 samples
            noverlap = round(0.5*window); % 50% overlap between frames
            nfft = 256*2-1; % yield 512 frequency bins
            [S,F,T,P] = spectrogram(y,window,noverlap,nfft,fs);

            % convert amplitude to dB
            I1 = 10*log10(abs(P));
            [n,locs] = hist(I1(:),[min(I1(:)):max(I1(:))]);
%             figure(6), clf, bar(locs,n)
            maxl = locs(find(n==max(n)));
%             [max(I1(:)), maxl(1)]
            snr_orig = max(I1(:)) - maxl(1);
            minI1 = min(I1(:));
            maxI1 = max(I1(:));
            QI1 = maxl;

            % STEP 2: WIENER FILTERING = w=3
            w = 3;
            I2 = wiener2(I1, [w w]);
            [n,locs] = hist(I2(:),[min(I2(:)):max(I2(:))]);
%             figure(5), clf, bar(locs,n)
            maxl = locs(find(n==max(n)));
%             [max(I1(:)), maxl(1)]
%             snr3 = max(I1(:)) - maxl(1);
            minw3 = min(I2(:));
            maxw3 = max(I2(:));
            Qw3 = maxl;
            
            % w=5
            w = 5;
            I2 = wiener2(I1, [w w]);
            [n,locs] = hist(I2(:),[min(I2(:)):max(I2(:))]);
%             figure(5), clf, bar(locs,n)
            maxl = locs(find(n==max(n)));
%             [max(I1(:)), maxl(1)]
%             snr5 = max(I1(:)) - maxl(1);
            minw5 = min(I2(:));
            maxw5 = max(I2(:));
            Qw5 = maxl;
            
            % w=7
            w = 7;
            I2 = wiener2(I1, [w w]);
            [n,locs] = hist(I2(:),[min(I2(:)):max(I2(:))]);
%             figure(5), clf, bar(locs,n)
            maxl = locs(find(n==max(n)));
%             [max(I1(:)), maxl(1)]
%             snr7 = max(I1(:)) - maxl(1);
            minw7 = min(I2(:));
            maxw7 = max(I2(:));
            Qw7 = maxl;
            
            
            % store results in an excel file
            cntr = cntr + 1;
            xlswrite(xls_file, {this_filename}, strcat('A',num2str(cntr),':A',num2str(cntr)))

            xlswrite(xls_file, year, strcat('B',num2str(cntr),':B',num2str(cntr)))
            xlswrite(xls_file, month, strcat('C',num2str(cntr),':C',num2str(cntr)))
            xlswrite(xls_file, day, strcat('D',num2str(cntr),':D',num2str(cntr)))
            xlswrite(xls_file, hour, strcat('E',num2str(cntr),':E',num2str(cntr)))
            xlswrite(xls_file, minute, strcat('F',num2str(cntr),':F',num2str(cntr)))
            xlswrite(xls_file, second, strcat('G',num2str(cntr),':G',num2str(cntr)))

            xlswrite(xls_file, {minI1}, strcat('H',num2str(cntr),':H',num2str(cntr)))
            xlswrite(xls_file, {maxI1}, strcat('I',num2str(cntr),':I',num2str(cntr)))
            xlswrite(xls_file, {QI1}, strcat('J',num2str(cntr),':J',num2str(cntr)))
            
            xlswrite(xls_file, {minw3}, strcat('K',num2str(cntr),':K',num2str(cntr)))
            xlswrite(xls_file, {maxw3}, strcat('L',num2str(cntr),':L',num2str(cntr)))
            xlswrite(xls_file, {Qw3}, strcat('M',num2str(cntr),':M',num2str(cntr)))
            
            xlswrite(xls_file, {minw5}, strcat('N',num2str(cntr),':N',num2str(cntr)))
            xlswrite(xls_file, {maxw5}, strcat('O',num2str(cntr),':O',num2str(cntr)))
            xlswrite(xls_file, {Qw5}, strcat('P',num2str(cntr),':P',num2str(cntr)))
            
            xlswrite(xls_file, {minw7}, strcat('Q',num2str(cntr),':Q',num2str(cntr)))
            xlswrite(xls_file, {maxw7}, strcat('R',num2str(cntr),':R',num2str(cntr)))
            xlswrite(xls_file, {Qw7}, strcat('S',num2str(cntr),':S',num2str(cntr)))
            
%             xlswrite(xls_file, {snr_orig}, strcat('H',num2str(cntr),':H',num2str(cntr)))
%             xlswrite(xls_file, {snr3}, strcat('I',num2str(cntr),':I',num2str(cntr)))
%             xlswrite(xls_file, {snr5}, strcat('J',num2str(cntr),':J',num2str(cntr)))
%             xlswrite(xls_file, {snr7}, strcat('K',num2str(cntr),':K',num2str(cntr)))
    
%             pause
        end
        ts_cntr = ts_cntr + 1;
        
    else
        ts_cntr = 0;
    end
    
    last_timeslot = this_timeslot;
    
end
