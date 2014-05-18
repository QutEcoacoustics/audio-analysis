function [Q2,Q3]=residuals(x,y)

curve=fit(x',y','exp1');

for index=1:length(x)
    Q2(index)=y(index)-curve(x(index));
end
Q3=curve;
end
