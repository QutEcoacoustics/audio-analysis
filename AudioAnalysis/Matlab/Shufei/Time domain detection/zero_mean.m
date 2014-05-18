function Q1=zero_mean(Xn,L3)
Sum6=0;
Q1=zeros(1,L3);
for h=1:L3
    Sum6=Sum6+Xn(h);
end
Avg2=Sum6/L3;
for h=1:L3
    Q1(h)=Xn(h)-Avg2;
end
end
