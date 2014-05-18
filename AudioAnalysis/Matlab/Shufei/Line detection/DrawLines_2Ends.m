function DrawLines_2Ends(lineseg, varargin)
%Draw line segments, parameterized as (x1, x2, y1, y2), on graph
%
%  DrawLines_2Ends(lineseg, varargin)
%  A simple function for drawing line segments on graph. Made as an
%  auxiliary tool for function '[...] = Hough_Grd(...)'.
%
%  INPUT: (lineseg, properties)
%  lineseg:     Parameters (x1, x2, y1, y2) of line segments to draw.
%               Is a Ns-by-4 matrix with each row contains the parameters
%               (x1, x2, y1, y2) that define the two ends of a line
%               segment. The output 'lineseg' from the function
%               '[...] = Hough_Grd(...)' can be put here directly.
%  properties:  (Optional)
%               A string of line drawing properties. Will be transferred
%               to function 'plot' without modification for line drawing.
%
%  OUTPUT: None
%
%  BUG REPORT:
%  Please send your bug reports, comments and suggestions to
%  pengtao@glue.umd.edu . Thanks.

%  Author:  Tao Peng
%           Department of Mechanical Engineering
%           University of Maryland, College Park, Maryland 20742, USA
%           pengtao@glue.umd.edu
%  Version: alpha       Revision: Dec. 02, 2005


hold on;
for k = 1 : size(lineseg, 1),
    % The image origin defined in function '[...] = Hough_Grd(...)' is
    % different from what is defined in Matlab, off by (0.5, 0.5).
    if nargin > 1,
        plot(lineseg(k,1:2)+0.5, lineseg(k,3:4)+0.5, varargin{1});
    else
        plot(lineseg(k,1:2)+0.5, lineseg(k,3:4)+0.5, 'LineWidth', 2);
    end
end
hold off;
