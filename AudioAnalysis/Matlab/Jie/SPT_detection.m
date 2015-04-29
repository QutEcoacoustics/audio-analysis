% get spectral peak track based on Chen et al's method
% track.arrayp is the predicted track
% track.array is the original track
%-------------------------------------------------------------------------%
function [track_m,track] = SPT_detection(I4, rowIndex, maxGap, maxGapFrame, minDuration,...
                                        maxDuration,binTolerance,minTrackDensity)
 
% M:row; N:column
[M,N] = size(I4);                                        

track = struct('startframe',{},'endframe',{},'topbin',{},'bottombin',{},...,
                'status',{},'density',{},'array',{},'arrayp',{});
            
rowLength = length(rowIndex);
[~,r_1] = find(rowIndex > 0);

c = r_1(1);
            
track{1}.startframe = c;
track{1}.endframe   = c;
track{1}.topbin     = rowIndex(c);
track{1}.bottombin  = rowIndex(c);
track{1}.status     = 1;
track{1}.density    = 1;
track{1}.duration   = 1;
track{1}.array      = rowIndex(c);
track{1}.arrayp     = rowIndex(c);
  
% loop the peaks of each frame
c = c + 1;
while(c <= N)        
    if(rowIndex(c) ~= 0)        
            i_a = length(track); % number of tracks  
            while(i_a >= 1)            
                if(track{i_a}.status ~=0)  % the track is closed
                    % if the length between current column index and the
                    % endframe of one track is larger than the threshold,
                    % the track will be stored or removed
                    if((c - track{i_a}.endframe) > maxGapFrame || track{i_a}.endframe == rowLength)
                       % set the track colsed
                        track{i_a}.status = 0;
                       % calculate the duration,density and avbin
                       % 1. duration
                       track{i_a}.duration = track{i_a}.endframe - track{i_a}.startframe + 1;
                       % 2. density
                       number_a1 = length(find(track{i_a}.array > 0));
                       number_a2 = length(track{i_a}.array);
                       track{i_a}.density = number_a1 / number_a2;
                       
                       % compare them with corresponding threshold
                       if(track{i_a}.duration < minDuration || track{i_a}.density < minTrackDensity || track{i_a}.duration > maxDuration)
                            % prune the track
                            track(i_a) = []; % remove the array 
                       end                                     
                        i_a = i_a - 1;
                    else
                        i_a = i_a - 1;
                    end
                else    
                    i_a = i_a - 1;
                end
            end            
            %---------------------------------------------------------------%
            i_b = length(track); % number of tracks
            DOOR = 1;                        
            while (i_b >= 1)                
                if(track{i_b}.status ~= 0 && DOOR == 1)
                    % predict the next peak
                    if (length(track{i_b}.arrayp) > 1)
                        x = (track{i_b}.startframe:track{i_b}.endframe);
                        % y = rowIndex(track{i_b}.startframe:track{i_b}.endframe);
                        y = track{i_b}.arrayp;
                        % change the length of x and y for linear
                        % regression
                        if length(x) > 10
                            x = x(length(x)-9:length(x));
                            y = y(length(y)-9:length(y));                            
                        end
                        % change 'c' to 'track{i_b}.endframe+1'
                        p = polyfit(x,y,1);
                        yfit = polyval(p,track{i_b}.endframe+1);
                    else
                        yfit = track{i_b}.array;
                    end
                                                         
                    % wheather both the time domian interval and frequency domain
                    % interval satisfy the threshold 
                    if (abs(rowIndex(c) - yfit) <= binTolerance) && (c - track{i_b}.endframe) < maxGap
                        % add peak to track
                        % track{i_b}.array = rowIndex(track{i_b}.startframe:c); 
                        if c ~= track{i_b}.endframe + 1;
                            add_value = zeros(1,c - track{i_b}.endframe - 1);
                            add_valuep = ones(1,c - track{i_b}.endframe - 1) * mean(track{i_b}.array) ;
                            track{i_b}.arrayp = [track{i_b}.arrayp,add_valuep,rowIndex(c)];
                            track{i_b}.array = [track{i_b}.array,add_value,rowIndex(c)];
                        else
                            track{i_b}.arrayp = [track{i_b}.arrayp,rowIndex(c)];
                            track{i_b}.array = [track{i_b}.array,rowIndex(c)];                        
                        end                                                                 
                        % recalculate the endframe
                        track{i_b}.endframe = c;
                        % recalculate the bottom and top bin
                        track{i_b}.topbin = max(rowIndex(c),track{i_b}.topbin);
                        track{i_b}.bottombin = min(rowIndex(c),track{i_b}.bottombin);    
                        % recalculate the duration
                        track{i_b}.duration = track{i_b}.endframe - track{i_b}.startframe + 1;
                        % recalculate the density
                        number_b1 = length(find(track{i_b}.array > 0));
                        number_b2 = length(track{i_b}.array);
                        track{i_b}.density = number_b1 / number_b2;
                        DOOR = 0;
                        break;
                    end                    
                    i_b = i_b - 1;  
                else
                    i_b = i_b - 1;
                end                                
            end
            
            if(DOOR == 1)
                % start a new track
                len_b = length(track);
                track{len_b+1}.startframe = c;
                track{len_b+1}.endframe   = c;
                track{len_b+1}.topbin     = rowIndex(c);
                track{len_b+1}.bottombin  = rowIndex(c);
                track{len_b+1}.array      = rowIndex(c);
                track{len_b+1}.arrayp     = rowIndex(c); 
                track{len_b+1}.status     = 1;
                track{len_b+1}.density    = 1;
                track{len_b+1}.duration   = 1;                                               
            end
        c = c + 1;
    else
        c = c + 1;        
    end        
end
 
%------------------------------------------------------------------------%
% check the duration since tha last track has not been checked by the
% duration threshold
i_d = length(track);
while i_d >= 1

    if(track{i_d}.duration < minDuration || track{i_d}.duration > maxDuration)
        track(i_d) = [];        
    end 
    i_d = i_d - 1;
end

%------------------------------------------------------------------------%
% for multiple tracks, we should select the track with the highest energy to represent the
% advertisement call

% track_d = track{1}.duration;
% i_e = length(track);
% while i_e >= 2
%     temp_d = track{i_e}.duration;
%     if temp_d > track_d
%         i_max = i_e;
%     end
%     
% end
% track_f = track{i_max};

% plot the track on the spectrogram
% change the track into matrix

track_m = zeros(M,N);
track_number = length(track);
for i = 1:track_number   
    track_x = track{i}.startframe:track{i}.endframe;
    track_y = round(track{i}.array);
    track_x(track_x == 0) = 1;
    track_y(track_y == 0) = 1;   
    
    for j = 1:length(track_x(:))
        track_m(track_y(j),track_x(j)) = 1;      
    end  
end

end