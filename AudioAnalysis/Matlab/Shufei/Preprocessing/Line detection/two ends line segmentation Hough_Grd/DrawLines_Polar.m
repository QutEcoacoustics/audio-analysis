function DrawLines_Polar(imgsize, lineprm, varargin)
%Draw lines, which are parameterized as (pho, theta), on graph
%
%  DrawLines_Polar(imgsize, lineprm, properties)
%  A simple function for drawing parameterized lines on graph. Made as
%  an auxiliary tool for function '[...] = Hough_Grd(...)'.
%  The parameterization of line complies with the definition in Hough
%  Transform, i.e. 'pho' is the perpendicular distance from the line
%  to the origin of the image and 'theta' is the sweep angle from axis X
%  (i.e. horizontal axis of the image) to the direction that is
%  perpendicular to the given line.
%
%  INPUT: (imgsize, lineprm, properties)
%  imgsize:     Size of the graph, usually the size of the original image
%               from where the lines were extracted, in the format of
%               [vertical dimension, horizontal dimension].
%               The lines are drawn throughout the graph, which is the
%               reason that the size information is required.
%  lineprm:     Parameters (pho, theta) of the lines to draw. Is a N-by-2
%               matrix with each row contains the parameters (pho, theta) 
%               for a line. The output 'lineprm' from the function
%               '[...] = Hough_Grd(...)' can be put here directly.
%               The definitions of 'pho' and 'theta' are as the following:
%               'pho' is the perpendicular distance from the line to the
%               origin of the image. 'pho' can be negative since 'theta'
%               is constrained to [0, pi]. The unit of 'pho' is in pixels.
%               'theta' is the sweep angle from axis X (i.e. horizontal
%               axis of the image) to the direction that is perpendicular
%               to the line. The range of 'theta' is [0, pi].
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
line = zeros(2,2);
for k = 1 : size(lineprm, 1),
    if lineprm(k,2) > pi/4 && lineprm(k,2) < 3*pi/4,
        line(1,1) = 0;
        line(1,2) = lineprm(k,1) / sin(lineprm(k,2));
        line(2,1) = imgsize(2);
        line(2,2) = line(1,2) - line(2,1) / tan(lineprm(k,2));
    else
        line(1,2) = 0;
        line(1,1) = lineprm(k,1) / cos(lineprm(k,2));
        line(2,2) = imgsize(1);
        line(2,1) = line(1,1) - line(2,2) * tan(lineprm(k,2));
    end
    % The image origin defined in function '[...] = Hough_Grd(...)' is
    % different from what is defined in Matlab, off by (0.5, 0.5).
    line = line + 0.5;
    % Draw lines using 'plot'
    if nargin > 2,
        plot(line(:,1), line(:,2), varargin{1});
    else
        plot(line(:,1), line(:,2));
    end
end
hold off;
