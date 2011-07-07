function [mappedX, network] = train_autoencoder(X, layers)
%TRAIN_AUTOENCODER Trains an autoencoder using RBM pretraining
%
%   [mappedX, network] = train_encoder(X, layers)
%
% Trains up an autoencoder with the structure that is specified in layers. 
% The low-dimensional data is returned in mappedX, and the network in
% network. This function is mainly designed to be used for binary [0,1]
% data.

% This file is part of the Matlab Toolbox for Dimensionality Reduction v0.7.2b.
% The toolbox can be obtained from http://homepage.tudelft.nl/19j49
% You are free to use, change, or redistribute this code in any way you
% want for non-commercial purposes. However, it is appreciated if you 
% maintain the name of the original author.
%
% (C) Laurens van der Maaten, 2010
% University California, San Diego / Delft University of Technology


    if length(unique(X)) ~= 2 || any(unique(X) ~= [0 1])
        warning('This function is designed to work on binary data. Running on real-valued data that is scaled between 0 and 1 may work too.');
    end

    % Pretrain the network
    origX = X;
    no_layers = length(layers);
    network = cell(1, no_layers);
    for i=1:no_layers

        % Print progress
        disp(['Training layer ' num2str(i) ' (size ' num2str(size(X, 2)) ' -> ' num2str(layers(i)) ')...']);
        
        if i ~= no_layers
          
            % Train layer using binary units
            network{i} = train_rbm(X, layers(i));
                
            % Transform data using learned weights
            X = 1 ./ (1 + exp(-(bsxfun(@plus, X * network{i}.W, network{i}.bias_upW))));
        else
            
            % Train layer using Gaussian hidden units
            network{i} = train_lin_rbm(X, layers(i));
        end
    end
    
    % Perform backpropagation to minimize reconstruction error
    network = roll_out(network);
    network = backprop(network, origX, origX, 10);
    [foo, mappedX] = run_data_through_autoenc(network, origX);
    