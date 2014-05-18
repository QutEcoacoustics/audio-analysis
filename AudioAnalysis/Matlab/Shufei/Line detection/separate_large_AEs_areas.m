function [newAE] = separate_large_AEs_areas(oldAE,L,area_thresh,I1,Ia,Ib,Ic)
% Seperates acoustic events that are too large in area
%
% Sensor Networks Project
% Birgit Planitz
% 20090327 

[M,N] = size(I1);


% find large acoustic events
ind = find( (oldAE(3,:).*oldAE(4,:))>=area_thresh);
workingAE = oldAE(:,ind);
[tmp, numW] = size(workingAE);

newAE = []; % init
for cntr = 1:size(oldAE,2)
    if (isempty(find(ind==cntr)))
        newAE = [newAE oldAE(:,cntr)];
    end
end



freq_thresh = 20;
time_thresh = 1/3*100; % events must be longer than this (bandwidth)

for nw=1:numW
    
    thisI = zeros(M,N);
    thisI(L==ind(nw)) = 1;
        
    startM = workingAE(2,nw);
    endM = workingAE(2,nw) + workingAE(4,nw) - 1;
    startN = workingAE(1,nw);
    endN = workingAE(1,nw) + workingAE(3,nw) - 1;
    thisI = thisI(startM:endM,startN:endN);
    [tM,tN] = size(thisI);
%     figure(28), clf, imshow(thisI)
    
    tmp1 = sum(thisI,1);
    tmp2 = sum(thisI,2);
    
    tmp1 = tmp1./tM*100;
    tmp2 = tmp2./tN*100;
    
%     figure(30), clf, plot(tmp1)
%     figure(31), clf, plot(tmp2)
    
    %get main frequency rectangles
    f_ind = find(tmp2>freq_thresh);
    finalI2 = zeros(tM,tN);
    finalI2(f_ind,:) = 1;
    finalI2(thisI==0) = 0;
%     figure(32), clf, imshow(finalI2)
    % get rectagnular boundaries for each remaining labelled area
    finalI2L = bwlabel(finalI2);
    maxL = max(finalI2L(:));
    if maxL>0
        for kk=1:maxL
            [tmpx,tmpy] = find(finalI2L==kk);
            thisAE = zeros(6,1);
            thisAE(1) = startN -1 + min(tmpy); %top
            thisAE(2) = startM -1 + min(tmpx); %bot
            thisAE(3) = max(tmpy)-min(tmpy)+1; %height
            thisAE(4) = max(tmpx)-min(tmpx)+1; % width
            
            
            thisIa = Ia(thisAE(2):(thisAE(2)+thisAE(4)-1),thisAE(1):(thisAE(1)+thisAE(3)-1));
            thisAE(5) = mean(thisIa(:));
            thisAE(6) = var(thisIa(:));
            thisIb = Ib(thisAE(2):(thisAE(2)+thisAE(4)-1),thisAE(1):(thisAE(1)+thisAE(3)-1));
            thisAE(7) = mean(thisIb(:));
            thisAE(8) = var(thisIb(:));
            thisIc = Ic(thisAE(2):(thisAE(2)+thisAE(4)-1),thisAE(1):(thisAE(1)+thisAE(3)-1));
            thisAE(9) = mean(thisIc(:));
            thisAE(10) = var(thisIc(:));

            
            newAE = [newAE thisAE];
        end
    end
    
    
    
    %get main time rectangles
    finalI1 = thisI - finalI2;
    finalI1(finalI1<0) = 0;
%     figure(33), clf, imshow(finalI1)
    
    finalI1L = bwlabel(finalI1);
    maxL = max(finalI1L(:));
    final_finalI1 = zeros(size(finalI1));
    kcntr = 0;
    if maxL>0
        for kk=1:maxL
            [tmpx,tmpy] = find(finalI1L==kk);
            if ((max(tmpx)-min(tmpx))*100/tM)>=time_thresh
                kcntr = kcntr+1;
                final_finalI1(finalI1L==kk) = kcntr;
            end
        end
    end
    
    % get rectangular boundaries for each remaining labelled area - whole
    % frequency range
    maxL = max(final_finalI1(:));
    if maxL>0
        for kk=1:maxL
            [tmpx,tmpy] = find(final_finalI1==kk);
            thisAE = zeros(10,1);
            thisAE(1) = startN -1 + min(tmpy); %top
            thisAE(2) = startM; %bot
            thisAE(3) = max(tmpy)-min(tmpy)+1; %height
            thisAE(4) = tM; % width
            
%             
            thisIa = Ia(thisAE(2):(thisAE(2)+thisAE(4)-1),thisAE(1):(thisAE(1)+thisAE(3)-1));
            thisAE(5) = mean(thisIa(:));
            thisAE(6) = var(thisIa(:));
            thisIb = Ib(thisAE(2):(thisAE(2)+thisAE(4)-1),thisAE(1):(thisAE(1)+thisAE(3)-1));
            thisAE(7) = mean(thisIb(:));
            thisAE(8) = var(thisIb(:));
            thisIc = Ic(thisAE(2):(thisAE(2)+thisAE(4)-1),thisAE(1):(thisAE(1)+thisAE(3)-1));
            thisAE(9) = mean(thisIc(:));
            thisAE(10) = var(thisIc(:));

            
            newAE = [newAE thisAE];
        end
    end
%     figure(34), clf, imshow(final_finalI1)
    
    
    
%     pause
end


