% plot_classification

pathSF = '.\07-10\classification\SyllableFeature.csv';
pathMFCC = '.\07-10\classification\MFCC.csv';
pathWPDEnergy = '.\07-10\classification\WPDEnergy.csv'; 
pathPWSCC = '.\07-10\classification\PWSCC.csv';


MFCC = csvread(pathMFCC);
SF = csvread(pathSF);
WPDEnergy = csvread(pathWPDEnergy);
PWSCC = csvread(pathPWSCC);

stdMFCC = MFCC(2,:);
stdSF = SF(2,:);
stdWPDEnergy = WPDEnergy(2,:);
stdPWSCC  =PWSCC(2,:);

meanMFCC = MFCC(1,:);
meanSF = SF(1,:);
meanWPDEnergy = WPDEnergy(1,:);
meanPWSCC  =PWSCC(1,:);
 
barwitherr(  [ stdSF' stdMFCC' stdWPDEnergy' stdPWSCC'], 1:10, [meanSF' meanMFCC' meanWPDEnergy' meanPWSCC']);

label = {'CPA', 'LCA', 'LCS', 'LLA', 'LNA', 'MFS', 'MFI', 'NSI', 'UFA', 'ULA'};
set(gca, 'xtick', 1:10, 'XTickLabel', label);

ylabel(' Classification accuracy');
xlabel('Frog species')

legend('SF', 'MFCC', 'WPD-Energy', 'PWSCC');

export_fig '.\07-10\classificationAcc_new.png'

