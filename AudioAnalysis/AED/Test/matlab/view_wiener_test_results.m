function view_wiener_test_results
% This code views test results of different w selections
% 
% Test applied to BAC2 recordings in 
% G:\Birgit\Sensor\image_proc\Bac2 - audio
%
% Sensor Networks Project
% Birgit Planitz
% 20090415


% read xls file
xls_file = 'wiener_tests3.xls';
xls_values = xlsread(xls_file);
num_files = size(xls_values,1);

hours = xls_values(9:end,4);
minutes = xls_values(9:end,5);
timeframes = hours*60 + minutes;
TS = 48;
timeslots = linspace(0,23.5,48)*60;

minI1 = xls_values(:,7);
maxI1 = xls_values(:,8);
QI1 = xls_values(:,9);

minw3 = xls_values(:,10);
maxw3 = xls_values(:,11);
Qw3 = xls_values(:,12);

minw5 = xls_values(:,13);
maxw5 = xls_values(:,14);
Qw5 = xls_values(:,15);

minw7 = xls_values(:,16);
maxw7 = xls_values(:,17);
Qw7 = xls_values(:,18);



minI1_time = zeros(1,TS);
maxI1_time = zeros(1,TS);
QI1_time = zeros(1,TS);
snrI1_time = zeros(1,TS);
rangeI1_time = zeros(1,TS);
ratioI1_time = zeros(1,TS);
diffI1_time = zeros(1,TS);
noiseI1_time = zeros(1,TS);

minw3_time = zeros(1,TS);
maxw3_time = zeros(1,TS);
Qw3_time = zeros(1,TS);
snrw3_time = zeros(1,TS);
rangew3_time = zeros(1,TS);
ratiow3_time = zeros(1,TS);
diffw3_time = zeros(1,TS);
noisew3_time = zeros(1,TS);

minw5_time = zeros(1,TS);
maxw5_time = zeros(1,TS);
Qw5_time = zeros(1,TS);
snrw5_time = zeros(1,TS);
rangew5_time = zeros(1,TS);
ratiow5_time = zeros(1,TS);
diffw5_time = zeros(1,TS);
noisew5_time = zeros(1,TS);

minw7_time = zeros(1,TS);
maxw7_time = zeros(1,TS);
Qw7_time = zeros(1,TS);
snrw7_time = zeros(1,TS);
rangew7_time = zeros(1,TS);
ratiow7_time = zeros(1,TS);
diffw7_time = zeros(1,TS);
noisew7_time = zeros(1,TS);

for num_ts = 1:TS
    
    start_time = timeslots(num_ts);
    if num_ts<TS
        end_time = timeslots(num_ts+1);
    else
        end_time = 48*60;
    end
    [timelocs] = find(timeframes>=start_time & timeframes<end_time);
    
    minI1_time(num_ts) = mean(minI1(timelocs));
    maxI1_time(num_ts) = mean(maxI1(timelocs));
    QI1_time(num_ts) = mean(QI1(timelocs));
    snrI1_time(num_ts) = mean(maxI1(timelocs)-QI1(timelocs));
    rangeI1_time(num_ts) = mean(maxI1(timelocs)-minI1(timelocs));
    ratioI1_time(num_ts) = mean( (maxI1(timelocs)-QI1(timelocs)) ./ (maxI1(timelocs)-minI1(timelocs)) );
    diffI1_time(num_ts) = mean( (maxI1(timelocs)-QI1(timelocs)) - (QI1(timelocs)-minI1(timelocs)));
    noiseI1_time(num_ts) = mean(QI1(timelocs)-minI1(timelocs));
    
    minw3_time(num_ts) = mean(minw3(timelocs));
    maxw3_time(num_ts) = mean(maxw3(timelocs));
    Qw3_time(num_ts) = mean(Qw3(timelocs));
    snrw3_time(num_ts) = mean(maxw3(timelocs)-Qw3(timelocs));
    rangew3_time(num_ts) = mean(maxw3(timelocs)-minw3(timelocs));
    ratiow3_time(num_ts) = mean( (maxw3(timelocs)-Qw3(timelocs)) ./ (maxw3(timelocs)-minw3(timelocs)) );
    diffw3_time(num_ts) = mean( (maxw3(timelocs)-Qw3(timelocs)) - (Qw3(timelocs)-minw3(timelocs)));
    noisew3_time(num_ts) = mean(Qw3(timelocs)-minw3(timelocs));
    
    minw5_time(num_ts) = mean(minw5(timelocs));
    maxw5_time(num_ts) = mean(maxw5(timelocs));
    Qw5_time(num_ts) = mean(Qw5(timelocs));
    snrw5_time(num_ts) = mean(maxw5(timelocs)-Qw5(timelocs));
    rangew5_time(num_ts) = mean(maxw5(timelocs)-minw5(timelocs));
    ratiow5_time(num_ts) = mean( (maxw5(timelocs)-Qw5(timelocs)) ./ (maxw5(timelocs)-minw5(timelocs)) );
    diffw5_time(num_ts) = mean( (maxw5(timelocs)-Qw5(timelocs)) - (Qw5(timelocs)-minw5(timelocs)));
    noisew5_time(num_ts) = mean(Qw5(timelocs)-minw5(timelocs));
    
    minw7_time(num_ts) = mean(minw7(timelocs));
    maxw7_time(num_ts) = mean(maxw7(timelocs));
    Qw7_time(num_ts) = mean(Qw7(timelocs));
    snrw7_time(num_ts) = mean(maxw7(timelocs)-Qw7(timelocs));
    rangew7_time(num_ts) = mean(maxw7(timelocs)-minw7(timelocs));
    ratiow7_time(num_ts) = mean( (maxw7(timelocs)-Qw7(timelocs)) ./ (maxw7(timelocs)-minw7(timelocs)) );
    diffw7_time(num_ts) = mean( (maxw7(timelocs)-Qw7(timelocs)) - (Qw7(timelocs)-minw7(timelocs)));
    noisew7_time(num_ts) = mean(Qw7(timelocs)-minw7(timelocs));
    
end

disp('snr')
snr_totI1 = maxI1-QI1;
snr_totw3 = maxw3-Qw3;
snr_totw5 = maxw5-Qw5;
snr_totw7 = maxw7-Qw7;
[mean(snr_totI1-snr_totw3) mean(snr_totI1-snr_totw5) mean(snr_totI1-snr_totw7)]

disp('range')
range_totI1 = maxI1-minI1;
range_totw3 = maxw3-minw3;
range_totw5 = maxw5-minw5;
range_totw7 = maxw7-minw7;
[mean(range_totI1-range_totw3) mean(range_totI1-range_totw5) mean(range_totI1-range_totw7)]

disp('snr-noise')
mean(snr_totI1-(range_totI1-snr_totI1))
mean(snr_totw3-(range_totw3-snr_totw3))
mean(snr_totw5-(range_totw5-snr_totw5))
mean(snr_totw7-(range_totw7-snr_totw7))

% disp('ratio snr:range')
% ratio_totI1 = (maxI1-QI1) ./ (maxI1-minI1);
% ratio_totw3 = (maxw3-Qw3) ./ (maxw3-minw3);
% ratio_totw5 = (maxw5-Qw5) ./ (maxw5-minw5);
% ratio_totw7 = (maxw7-Qw7) ./ (maxw7-minw7);
% [mean(ratio_totI1-ratio_totw3) mean(ratio_totI1-ratio_totw5) mean(ratio_totI1-ratio_totw7)]

%------------------
% PLOTTING
figure(1), clf, plot(timeslots/60,minI1_time,'b','Linewidth',1)
hold on, plot(timeslots/60,maxI1_time,'b','Linewidth',1)
hold on, plot(timeslots/60,QI1_time,'b','Linewidth',1)
hold on, plot(timeslots/60,minw3_time,'r','Linewidth',1)
hold on, plot(timeslots/60,maxw3_time,'r','Linewidth',1)
hold on, plot(timeslots/60,Qw3_time,'r','Linewidth',1)
hold on, plot(timeslots/60,minw5_time,'g','Linewidth',1)
hold on, plot(timeslots/60,maxw5_time,'g','Linewidth',1)
hold on, plot(timeslots/60,Qw5_time,'g','Linewidth',1)
hold on, plot(timeslots/60,minw7_time,'k','Linewidth',1)
hold on, plot(timeslots/60,maxw7_time,'k','Linewidth',1)
hold on, plot(timeslots/60,Qw7_time,'k','Linewidth',1)
axis tight, title('Average min,Q,max dB over 24hr cycle','FontSize',20), ylabel('dB','FontSize',20), set(gca,'XTick',0:2:23.5,'FontSize',20), xlabel('Time (hours)','FontSize',20)

figure(2), clf, plot(timeslots/60,snrI1_time,'b','Linewidth',1)
hold on, plot(timeslots/60,snrw3_time,'r','Linewidth',1)
hold on, plot(timeslots/60,snrw5_time,'g','Linewidth',1)
hold on, plot(timeslots/60,snrw7_time,'k','Linewidth',1)
axis tight, title('Average SNR over 24hr cycle','FontSize',20), ylabel('dB','FontSize',20), set(gca,'XTick',0:2:23.5,'FontSize',20), xlabel('Time (hours)','FontSize',20)

figure(3), clf, plot(timeslots/60,rangeI1_time,'b','Linewidth',1)
hold on, plot(timeslots/60,rangew3_time,'r','Linewidth',1)
hold on, plot(timeslots/60,rangew5_time,'g','Linewidth',1)
hold on, plot(timeslots/60,rangew7_time,'k','Linewidth',1)
axis tight, title('Average range over 24hr cycle','FontSize',20), ylabel('dB','FontSize',20), set(gca,'XTick',0:2:23.5,'FontSize',20), xlabel('Time (hours)','FontSize',20)


figure(4), clf, plot(timeslots/60,noiseI1_time,'b','Linewidth',1)
hold on, plot(timeslots/60,noisew3_time,'r','Linewidth',1)
hold on, plot(timeslots/60,noisew5_time,'g','Linewidth',1)
hold on, plot(timeslots/60,noisew7_time,'k','Linewidth',1)
axis tight, title('Average noise over 24hr cycle','FontSize',20), ylabel('dB','FontSize',20), set(gca,'XTick',0:2:23.5,'FontSize',20), xlabel('Time (hours)','FontSize',20)


