function Q7=signalExtract(w1,t,g,nosle,o)
%t: oneStd
%g: X
%w1:Nstd
%(-2.58std,+2.58std)--99%
%(-1.96std,+1.96std)--95%
%(-1std, +1std)--68.27%


switch (o)
    case 1
            below=nosle-2.58*t;
            below2=nosle-1.96*t;
            below3=nosle-3*t;
             below4=nosle-1.50*t;
              below5=nosle-0.5*t;
    case 2
            below=nosle+2.58*t;
            below2=nosle+1.96*t;
            below3=nosle+3*t;
            below4=nosle+1.50*t;
            below5=nosle+0.5*t;
end
switch(o)
    case 1
        for q=1:g
           if w1(q)-below2<0
            sig1(q)=w1(q);
           else
            sig1(q)=NaN; %NaN=not a number
           end     
        end
        Q7=sig1;
    case 2
        for q=1:g
           if w1(q)-below2>0
            sig1(q)=w1(q);
           else
            sig1(q)=NaN; %NaN=not a number
           end     
        end
        Q7=sig1;
end
