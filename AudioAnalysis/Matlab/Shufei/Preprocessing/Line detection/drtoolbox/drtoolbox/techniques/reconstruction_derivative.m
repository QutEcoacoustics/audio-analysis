function [C, dC] = reconstruction_derivative(V, O, R)
%RECONSTRUCTION_DERIVATIVE Computes reconstruction error and derivative
%
%   [C, dC] = reconstruction_derivative(V, original, reconstruction)
%
% Computes reconstruction error and derivative with respect to the affine 
% transformation V that is applied on the reconstructions.
%

% This file is part of the Matlab Toolbox for Dimensionality Reduction v0.7.2b.
% The toolbox can be obtained from http://homepage.tudelft.nl/19j49
% You are free to use, change, or redistribute this code in any way you
% want for non-commercial purposes. However, it is appreciated if you 
% maintain the name of the original author.
%
% (C) Laurens van der Maaten, 2010
% University California, San Diego / Delft University of Technology


    % Retrieve the variables from the vector V
    alpha = V(1);
    scale = V(2);
    x0 = V(3);
    y0 = V(4);
    n = size(O, 1);

    % Compute reconstruction error
    C1 = O(:,1) - (scale .* cos(alpha) .* R(:,1)) - (scale .* sin(alpha) .* R(:,2)) + (scale .* ((x0 .* cos(alpha)) + (y0 .* sin(alpha))));
    C2 = O(:,2) + (scale .* sin(alpha) .* R(:,1)) - (scale .* cos(alpha) .* R(:,2)) - (scale .* ((x0 .* sin(alpha)) - (y0 .* cos(alpha))));
    C = (1 / n) .* sum((C1 .^ 2) + (C2 .^ 2));
    
    % Compute derivatives (over alpha, scale, x0, and y0)
    dC = repmat(0, [4 1]);
    dC(1) = sum((2 .* C1 .* ((scale .* sin(alpha) .* R(:,1)) - (scale .* cos(alpha) .* R(:,2)) + (scale .* ((y0 .* cos(alpha)) - (x0 .* sin(alpha)))))) + ...
                (2 .* C2 .* ((scale .* cos(alpha) .* R(:,1)) + (scale .* sin(alpha) .* R(:,2)) + (scale .* ((x0 .* cos(alpha)) + (y0 .* sin(alpha))))))) ./ n;
    dC(2) = sum((2 .* C1 .* ((-cos(alpha) .* R(:,1)) - (sin(alpha) .* R(:,2)) + (x0 .* cos(alpha) + y0 .* sin(alpha)))) + ...
                (2 .* C2 .* (( sin(alpha) .* R(:,1)) - (cos(alpha) .* R(:,2)) - (x0 .* sin(alpha) + y0 .* cos(alpha))))) ./ n;
    dC(3) = sum((2 .* C1 .* scale .* cos(alpha)) - (2 .* C2 .* scale .* sin(alpha))) ./ n;
    dC(4) = sum((2 .* C1 .* scale .* sin(alpha)) + (2 .* C2 .* scale .* cos(alpha))) ./ n;
    
