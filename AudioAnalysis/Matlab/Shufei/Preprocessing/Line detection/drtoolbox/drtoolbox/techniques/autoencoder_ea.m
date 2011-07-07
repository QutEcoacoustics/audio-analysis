function [mappedA, mapping] = autoencoder_ea(A, no_dims)
%AUTOENCODER_EA Trains an autoencoder using an evolutionary algorithm
%
%   [mappedX, mapping] = autoencoder_ea(X, no_dims)
%
% Trains an autoencoder using an evolutionary algorithm. The autoencoder is
% trained in such a way, that it reduces the dimensionality of dataset X to
% no_dims (default = 2). The network is a 4-layer feedforward neural network
% with sigmoid functions in the first 3 layers and linear functions in the 
% last layer. 
% The function returns the reduced data in mappedX, and the network layer
% weights in mapping.
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


    % Compute number of nodes in each layer based on input
    layer1_size = ceil(size(A, 2) * 1.2) + 20;
    layer2_size = ceil(size(A, 2) / 2) + 10;
    layer3_size = ceil(size(A, 2) / 4) + 5;
    layer4_size = no_dims;
    if layer3_size <= no_dims,      layer3_size = no_dims + 1;      end
    if layer2_size <= layer3_size,  layer2_size = layer3_size + 1;  end
    if layer1_size <= layer2_size,  layer1_size = layer2_size + 1;  end
    disp(['Performing training of ' num2str(size(A, 2)) '->' num2str(layer1_size) '-' num2str(layer2_size) '-' num2str(layer3_size) '->' num2str(no_dims) ' network...']);
        
    % Make sure data is zero-mean, unit variance
    mapping.mean = mean(A, 1);
    mapping.var  = var(A, 1);
    A = A -  repmat(mapping.mean, [size(A, 1) 1]);
    A = A ./ repmat(mapping.var,  [size(A, 1) 1]);

    % Initialize variables
    no_ind = 200;                       % number of individiuals
    no_gen = 30;                        % number of generations
    sel_perc = .5;                      % percentage of individuals to select
    mut_prob = 0.4;                     % mutation probability
    mut_s = 1.0;                        % variance of mutation amount
    fitnesses = nan(no_ind, 1);         % fitness array
    [n, d] = size(A);                   % number of samples and their dimensionality
    net_size = ((d+1) * layer1_size + (layer1_size+1) * layer2_size + (layer2_size+1) * layer3_size + (layer3_size+1) * layer4_size);     % size of individual
    best_fit = Inf;                     % best fitness until now    
    best_ind = nan(1, net_size);        % best indiviual until now
    weights = rand(no_ind, net_size);   % weight initialization
    disp(['Number of weights to learn: ' num2str(net_size)]);
    
    % Run evolutionary algorithm
    for i=1:no_gen
        
        % Evaluate individuals in population
        for j=1:no_ind
            % Extract layer weigths
            ind_b = 1; ind_e = (d+1) * layer1_size;
                w1 = weights(j, ind_b:ind_e);
                w1 = reshape(w1, [d+1 layer1_size]);
            ind_b = ind_e + 1; ind_e = ind_e + (layer1_size+1) * layer2_size;
                w2 = weights(j, ind_b:ind_e);
                w2 = reshape(w2, [(layer1_size+1) layer2_size]);
            ind_b = ind_e + 1; ind_e = ind_e + (layer2_size+1) * layer3_size;
                w3 = weights(j, ind_b:ind_e);
                w3 = reshape(w3, [(layer2_size+1) layer3_size]);
            ind_b = ind_e + 1; ind_e = ind_e + (layer3_size+1) * layer4_size;
                w4 = weights(j, ind_b:ind_e);
                w4 = reshape(w4, [(layer3_size+1) layer4_size]);
            w5 = w4';
            w6 = w3';
            w7 = w2';
            w8 = w1';
            
            % Compute output of network
            w1probs = [1 ./ (1 + exp(-[A ones(n, 1)] * w1))         ones(n, 1)];
            w2probs = [1 ./ (1 + exp(-w1probs * w2))                ones(n, 1)];
            w3probs = [1 ./ (1 + exp(-w2probs * w3))                ones(n, 1)];
            w4probs = w3probs * w4;
            w5probs = 1 ./ (1 + exp(-w4probs  * w5));   w5probs = w5probs(:,1:end-1);
            w6probs = 1 ./ (1 + exp(-w5probs * w6));    w6probs = w6probs(:,1:end-1);
            w7probs = 1 ./ (1 + exp(-w6probs * w7));    w7probs = w7probs(:,1:end-1);
            dataout =  1 ./ (1 + exp(-w7probs * w8));   dataout = dataout(:,1:end-1);  
            
            % Compute MSE between input and output
            fitnesses(j) = mean(mean((A - dataout) .^ 2));
        end
        
        % Select best individuals (lower fitness is better)
        [fitnesses, ind] = sort(fitnesses);
        weights = repmat(weights(ind(1:floor(sel_perc * no_ind)),:), [floor(1 / sel_perc) 1]);
        if size(weights, 1) < no_ind
            weights = [weights; rand(no_ind, size(weights, 1), net_size)];
        end
        
        % Store best individual until now
        if fitnesses(1) < best_fit
            best_fit = fitnesses(1);
            best_ind = weights(ind(1),:);
        end
        
        % Perform mutations
        weights = weights + ((rand(size(weights)) < mut_prob) .* randn(size(weights)) * mut_s);
        
        % Display on information on current iteration
        disp(['Iteration ' num2str(i) ': Average fitness ' num2str(mean(fitnesses)) ' and best fitness ' num2str(min(fitnesses)) '.']);
    end    
    
    % Use best individual to produce mapped data
    ind_b = 1; ind_e = (d+1) * layer1_size;
        w1 = weights(j, ind_b:ind_e);
        w1 = reshape(w1, [(d+1) layer1_size]);
    ind_b = ind_e + 1; ind_e = ind_e + (layer1_size+1) * layer2_size;
        w2 = weights(j, ind_b:ind_e);
        w2 = reshape(w2, [(layer1_size+1) layer2_size]);
    ind_b = ind_e + 1; ind_e = ind_e + (layer2_size+1) * layer3_size;
        w3 = weights(j, ind_b:ind_e);
        w3 = reshape(w3, [(layer2_size+1) layer3_size]);
    ind_b = ind_e + 1; ind_e = ind_e + (layer3_size+1) * layer4_size;
        w4 = weights(j, ind_b:ind_e);
        w4 = reshape(w4, [(layer3_size+1) layer4_size]);
    w5 = w4';
    w6 = w3';
    w7 = w2';
    w8 = w1';
    
    % Insert data into network (to get mappedA and reconstruction)
    w1probs = [1 ./ (1 + exp(-[A ones(n, 1)] * w1))     ones(n, 1)];
    w2probs = [1 ./ (1 + exp(-w1probs * w2))            ones(n, 1)];
    w3probs = [1 ./ (1 + exp(-w2probs * w3))            ones(n, 1)];
    mappedA = w3probs * w4;
    w5probs = 1 ./ (1 + exp(-w4probs * w5));   w5probs = w5probs(:,1:end-1);
    w6probs = 1 ./ (1 + exp(-w5probs * w6));   w6probs = w6probs(:,1:end-1);
    w7probs = 1 ./ (1 + exp(-w6probs * w7));   w7probs = w7probs(:,1:end-1);
    mapping.recon = 1 ./ (1 + exp(-w7probs * w8));
    mapping.recon = mapping.recon(:,1:end-1);
  
    % Store network
    mapping.w1 = w1;
    mapping.w2 = w2;
    mapping.w3 = w3;
    mapping.w4 = w4;
    
    disp('Done.');
