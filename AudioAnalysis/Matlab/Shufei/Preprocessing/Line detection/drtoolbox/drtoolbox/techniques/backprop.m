function network = backprop(network, X, T, max_iter)
%BACKPROP Trains a network on a dataset using backpropagation
%
%   network = backprop(network, X, T, max_iter)
%
% The function trains the specified network using backpropagation on
% dataset X with targets T for max_iter iterations. The dataset X is an NxD
% matrix, whereas the targets matrix T has size NxM. The variable network
% is a cell array that may be obtained from the TRAIN_DEEP_NETWORK
% function. The function returns the trained network in network.
%

% This file is part of the Matlab Toolbox for Dimensionality Reduction v0.7.2b.
% The toolbox can be obtained from http://homepage.tudelft.nl/19j49
% You are free to use, change, or redistribute this code in any way you
% want for non-commercial purposes. However, it is appreciated if you 
% maintain the name of the original author.
%
% (C) Laurens van der Maaten, 2010
% University California, San Diego / Delft University of Technology


    if ~exist('max_iter', 'var') || isempty(max_iter)
        max_iter = 10;
    end

    % Initialize some variables
    n = size(X, 1);
    no_layers = length(network);
    batch_size = round(n / 600);
    
    % Estimate the initial error
    estN = min([n 5000]);
    reconX = run_data_through_autoenc(network, X(1:estN,:));
    C = sum(sum((T(1:estN,:) - reconX) .^ 2)) ./ estN;
    disp(['Initial MSE: ' num2str(C)]);
    
    % Perform the backpropagation
    for iter=1:max_iter
        disp(['Iteration ' num2str(iter) '...']);
        
        % Loop over all batches
        index = randperm(n);
        for batch=1:batch_size:n

            % Select current batch
            tmpX = X(index(batch:min([batch + batch_size - 1 n])),:);
            tmpT = T(index(batch:min([batch + batch_size - 1 n])),:);

            % Convert the weights and store them in the network
            v = [];
            for i=1:length(network)
                v = [v; network{i}.W(:); network{i}.bias_upW(:)];
            end
            
            % Conjugate gradient minimization using 3 linesearches
%             checkgrad('backprop_gradient', v, 1e-5, network, tmpX, tmpT)
            v = minimize(v, 'backprop_gradient', 3, network, tmpX, tmpT);
            
            % Deconvert the weights and store them in the network
            ind = 1;
            for i=1:no_layers
                network{i}.W        = reshape(v(ind:ind - 1 + numel(network{i}.W)),        size(network{i}.W));         ind = ind + numel(network{i}.W);
                network{i}.bias_upW = reshape(v(ind:ind - 1 + numel(network{i}.bias_upW)), size(network{i}.bias_upW));  ind = ind + numel(network{i}.bias_upW);
            end
        end
        
        % Estimate the current error
        reconX = run_data_through_autoenc(network, X(1:estN,:));
        C = sum(sum((T(1:estN,:) - reconX) .^ 2)) ./ estN;
        disp(['MSE: ' num2str(C)]);
    end
    