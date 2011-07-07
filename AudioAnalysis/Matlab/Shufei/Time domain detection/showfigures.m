function showfigures(PdB2,Pa12,PAvg2,PStd2,y3)

figure(3),plot(Pa12);axis xy; axis tight; view(0,90);title('zero crossing number of windows','Fontsize',20);
ylabel('Number','Fontsize',20);
xlabel('Time(s)','Fontsize',20);
saveas(gcf,'zero crossing number of windows','fig');

figure(4),plot(PAvg2);axis xy; axis tight; view(0,90);title('Average Sample Number between two consecutive zero points of windows','Fontsize',20);
ylabel('Number','Fontsize',20);
xlabel('Time(s)','Fontsize',20);
saveas(gcf,'Average Sample Number between two consecutive zero points of windows','fig');

figure(5),plot(PStd2);axis xy; axis tight; view(0,90);title('Standard deviation of Windows','Fontsize',20);
ylabel('Stddev','Fontsize',20);
xlabel('Time(s)','Fontsize',20);
saveas(gcf,'Standard deviation of Windows','fig');

figure(6),plot(PdB2);
axis xy; axis tight; view(0,90);title('dB Value of Windows','Fontsize',20);
ylabel('dB','Fontsize',20);
xlabel('Time(s)','Fontsize',20);
saveas(gcf,'dB Value of Windows','fig');

figure(7);
subplot(5,1,1),plot(y3);axis xy; axis tight; view(0,90);title('Original data','Fontsize',20);
ylabel('Amplitude','Fontsize',20);

subplot(5,1,2),plot(PdB2);
axis xy; axis tight; view(0,90);title('dB Value of Windows','Fontsize',20);
ylabel('dB','Fontsize',20);

subplot(5,1,3),plot(Pa12);axis xy; axis tight; view(0,90);title('zero crossing number of windows','Fontsize',20);
ylabel('Number','Fontsize',20);

subplot(5,1,4),plot(PAvg2);axis xy; axis tight; view(0,90);title('Average Sample Number between two consecutive zero points of windows','Fontsize',20);
ylabel('Number','Fontsize',20);
subplot(5,1,5),plot(PStd2);axis xy; axis tight; view(0,90);title('Standard deviation of Windows','Fontsize',20);
ylabel('Stddev','Fontsize',20);
xlabel('Time(s)','Fontsize',20);
saveas(gcf,'Time series analysis','fig');
end
