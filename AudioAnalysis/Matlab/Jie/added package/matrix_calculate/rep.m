function [result]=rep(array, count)

matrix = repmat(array, count,1);
result = matrix(:);

end
%# [EOF]
