function err = compute_recon_err(machine, X)
%COMPUTE_RECON_ERROR Computes reconstruction error of RBM on dataset X
%
%   err = compute_recon_error(machine, X)
%   err = compute_recon_error(network, X)
%
% Computes reconstruction error of the RBM specified in machine or the RBMs
% specified in the netwerk, on the dataset specified in X. The 
% reconstruction error is normalized for the number of datapoints, and is 
% returned in err.
%

% This file is part of the Matlab Toolbox for Dimensionality Reduction v0.7.2b.
% The toolbox can be obtained from http://homepage.tudelft.nl/19j49
% You are free to use, change, or redistribute this code in any way you
% want for non-commercial purposes. However, it is appreciated if you 
% maintain the name of the original author.
%
% (C) Laurens van der Maaten, 2010
% University California, San Diego / Delft University of Technology


    if iscell(machine)
        
        % Run for every layer in the network
        err = zeros(1, length(machine));
        vis = double(X);
        for i=1:length(machine)
            
            % Compute probabilities for hidden nodes
            if i < length(machine) 
                hid = 1 ./ (1 + exp(-(bsxfun(@plus, vis * machine{i}.W, machine{i}.bias_upW))));
            else
                hid = bsxfun(@plus, vis * machine{i}.W, machine{i}.bias_upW);
            end

            % Compute probabilities for visible nodes
            rec = 1 ./ (1 + exp(-(bsxfun(@plus, hid * machine{i}.W', machine{i}.bias_downW))));

            % Compute reconstruction error
            err(i) = sum(sum((vis - rec) .^ 2)) ./ size(X, 1);
            vis = hid;
        end
    else
    
        % Compute probabilities for hidden nodes
        hid = 1 ./ (1 + exp(-(bsxfun(@plus, double(X) * machine.W, machine.bias_upW))));

        % Compute probabilities for visible nodes
        rec = 1 ./ (1 + exp(-(bsxfun(@plus, hid * machine.W', machine.bias_downW))));

        % Compute reconstruction error
        err = sum(sum((X - rec) .^ 2)) ./ size(X, 1);
    end