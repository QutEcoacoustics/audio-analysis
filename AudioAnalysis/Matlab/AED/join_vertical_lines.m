function I2 = join_vertical_lines(I1)
% Joins vertical lines that are seperated by vert_thresh pixels or less in 
% a black and white image
%
% Sensor Networks Project
% Birgit Planitz
% 20090309 

vert_thresh = 3;



[M,N] = size(I1);
I2 = I1;
for nn=1:N
    
    mstart = 1;
    mend = mstart + vert_thresh - 1;
    
    for mm = 1:M-vert_thresh+1
        
        if ( (mstart==mm) && (I1(mm,nn)==1) )
            
            if sum(I1(mstart+1:mend,nn)) >= 1
                
                first_one = find(I1(mstart+1:mend,nn) == 1);
                
                if (~isempty(first_one))
                    first_one = first_one(1);
                    I2(mstart+1:mstart+first_one,nn) = 1;
                    mstart = mstart+first_one;
                    mend = mstart + vert_thresh - 1;
                end
                
            end
        end
        if (mstart==mm)
            mstart = mm + 1;
            mend = mstart + vert_thresh - 1;
        end
    end
end
                

