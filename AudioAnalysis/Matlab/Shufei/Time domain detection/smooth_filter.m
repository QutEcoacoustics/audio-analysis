function Q=smooth_filter(Y1,L1,n)

Q=zeros(1,L1);
if n==1&&rem(n,2)==0 %note that n here should be an odd number%
    return
end
if rem(n,2)~=0
    Sum4=0;
    Sum5=0;
    for t=0:(L1-n)
        R=Y1((1+t):(n+t));        
        R1=R(1);%store the first sample
        R2=R(n);%store the new sample of next window
        
        if t==0
           for t1=1:n
            Sum4=Sum4+R(t1);
           end
           Q(t+(n-1)/2+1)=Sum4/n;
           Sum5=Sum4-R1;
        end
        if t~=0
           Sum4=Sum5+R2;
           Q(t+(n-1)/2+1)=Sum4/n;
           Sum5=Sum4-R1;
        end
    end
end
end
