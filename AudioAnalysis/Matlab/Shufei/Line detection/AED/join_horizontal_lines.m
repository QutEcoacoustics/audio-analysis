function I2 = join_horizontal_lines(I1)
% Joins horizontal lines that are seperated by horz_thresh pixels or less 
% in a black and white image
%
% Sensor Networks Project
% Birgit Planitz
% 20090311 

horz_thresh = 3;



[M,N] = size(I1);
I2 = I1;
for mm=1:M
    
    nstart = 1;
    nend = nstart + horz_thresh - 1;
    
    for nn = 1:N-horz_thresh+1
        
        if ( (nstart==nn) && (I1(mm,nn)==1) )
            
            if sum(I1(mm,nstart+1:nend)) >= 1
                
                first_one = find(I1(mm,nstart+1:nend) == 1);
                
                if (~isempty(first_one))
                    first_one = first_one(1);
                    I2(mm,nstart+1:nstart+first_one) = 1;
                    nstart = nstart+first_one;
                    nend = nstart + horz_thresh - 1;
                end
                
            end
        end
        if (nstart==nn)
            nstart = nn + 1;
            nend = nstart + horz_thresh - 1;
        end
    end
end
                

