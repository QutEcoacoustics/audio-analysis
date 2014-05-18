function [f2,f3]=MakerInSpectrogram(w5,F1,Frame,SigFz1,Sig2,oneStd1,L6)
%w5: I1
%F1: F
%T1: T
%Frame: FrameN
%SigFz1:SigFz
%Sig2: Sig
%L6: X (total frame number)

%Group the signals according to the Std value
%binlength=oneStd 
Min=min(Sig2);
Max=max(Sig2);
Nbin=fix((Max-Min)/(2*oneStd1))+1;
E=zeros(Nbin,L6);
f1=zeros(Nbin,L6);
f2=zeros(Nbin,L6);
f3=zeros(Nbin,L6);

l=1;
for k1=1:Nbin
        r1=k1*2*oneStd1;
        r2=(k1-1)*2*oneStd1;  
        for k2=1:L6
           if (Sig2(k2)<Min+r1)&&(Sig2(k2)>=(Min+r2))
                E(k1,l)=k2; % group order is from low part to high part
                l=l+1;
           end                           
        end
end

%group all frequency bins into fix(11025/500)+1
rr=length(F1);
l3=1;
for k=1:L6
    if isnan(Frame(k))==0
        for k3=1:(rr-1)
            if SigFz1(k)==F1(k3)
               w5(k3,Frame(k))=NaN;
                %use Frame(k) to match the frame number in E
               for k4=1:Nbin
                   for l2=1:(l-1)
                      if Frame(k)==E(k4,l2)
                            f1(k4,l3)=w5(k3,Frame(k));%store the point value
                            f2(k4,l3)=k3;%store the y-axis value (frequency)
                            f3(k4,l3)=Frame(k);%store the x-axis value (time, frame number)
                            l3=l3+1;
                      end
                   end
                end
            elseif SigFz1(k)==F1(k3+1)
                w5((k3+1),Frame(k))=NaN; 
                for k4=1:Nbin
                    for l2=1:(l-1)                   
                       if Frame(k)==E(k4,l2)
                            f1(k4,l3)=w5(k3,Frame(k));%store the point value
                            f2(k4,l3)=k3+1;%store the y-axis value (frequency)
                            f3(k4,l3)=Frame(k);%store the x-axis value (time, frame number)
                            l3=l3+1;
                       end
                    end
                end
            elseif (SigFz1(k)>F1(k3))&&(SigFz1(k)<F1(k3+1))
                w5((k3+1),Frame(k))=NaN;
                for k4=1:Nbin
                    for l2=1:(l-1)                    
                        if Frame(k)==E(k4,l2)
                            f1(k4,l3)=w5(k3,Frame(k));%store the point value
                            f2(k4,l3)=k3;%store the y-axis value (frequency)
                            f3(k4,l3)=Frame(k);%store the x-axis value (time, frame number)
                            l3=l3+1;
                        end
                    end
                end
            end
        end
    end
end
Q12=w5;

%Add colours onto the points in the spectrogram


end

 
 

