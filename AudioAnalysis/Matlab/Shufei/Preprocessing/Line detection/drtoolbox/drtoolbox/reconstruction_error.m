function err = reconstruction_error(O, R)
%RECONSTRUCTION_ERROR Computes the reconstruction error up to affine transformations
% 
%   err = reconstruction_error(O, R)
%
% The function computes the reconstruction error of the dataset R (given 
% the original dataset O) up to all linear transformations. The
% function does so by minimizing (O - RW)^2 over the affine transformation W.
% The reconstruction error is computed for every flipping of the data, and 
% the resulting minimum is returned in err.
% 
%

% This file is part of the Matlab Toolbox for Dimensionality Reduction v0.7.2b.
% The toolbox can be obtained from http://homepage.tudelft.nl/19j49
% You are free to use, change, or redistribute this code in any way you
% want for non-commercial purposes. However, it is appreciated if you 
% maintain the name of the original author.
%
% (C) Laurens van der Maaten, 2010
% University California, San Diego / Delft University of Technology



    % Process PRTools datasets
    if strcmp(class(O), 'dataset')
        O = O.data;
    end
    if strcmp(class(R), 'dataset')
        R = R.data;
    end
    if sum(size(O) ~= size(R))
        error('Original and reconstructed data matrices should be of the same size.');
    end
    if size(O, 2) == 1
        O = [O O];
        R = [R R];
    end
    if size(O, 2) > 2
        error('This function was designed for two-dimensional manifolds only.');
    end
    
    % Make sure all features are scaled in [0, 1]
    O = O -  repmat(min(O, [], 1), [size(O, 1) 1]);
    O = O ./ repmat(max(O, [], 1) + 1e-9, [size(O, 1) 1]);
    R = R -  repmat(min(R, [], 1), [size(R, 1) 1]);
    R = R ./ repmat(max(R, [], 1) + 1e-9, [size(R, 1) 1]);
    
    % Perform minimization using conjugate gradient descent (for every
    % combination of flips)
    min_err = Inf;
    min_W = repmat(0, [4 1]);
    min_flip = repmat(0, [1 size(O, 2)]);
    comb = combn([0; 1], size(O, 2));
    for i=1:size(comb, 1)
        
        % Perform necessary flipping
        RR = R;
        for j=find(comb(i,:))
            RR(:,j) = 1 - RR(:,j);
        end
        
        % Minimize cost function
        max_iter = 100;
        randn('seed', cputime);
        V = [0; 1; 0; 0];
        [V, err] = minimize(V, 'reconstruction_derivative', max_iter, O, RR);
        
        % Compute reconstruction error
        err = err(end);
        if err < min_err
            min_err = err;
            min_flip = comb(i,:);
            min_V = V;
        end
    end
    
    % Set final reconstruction error
    err = min_err;
