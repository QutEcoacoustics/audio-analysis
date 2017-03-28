function [W, H] = NMF_decompose(amp)
%randomise the amplitude matrix
rng('default');
randomAmp = reshape(amp(randperm(numel(amp))), size(amp));

%initialise residuals
diffResidual = 0;
diffRandomResidual = 0;

%non-negative matrix deconvolution
r = 1;
while diffResidual >= diffRandomResidual
    rng('default');
    [W,H,D1] = nnmf(amp, r);
    residual(r) = D1;
    [~,~,D2] = nnmf(randomAmp, r);
    randomResidual(r) = D2;
    if r > 1
        diffResidual = residual(end - 1) - residual(end);
        diffRandomResidual = randomResidual(end - 1) - randomResidual(end);
    end
    r = r + 1;
end

% approx = W * H;