
exports.getOptions = function (model) {
    return {
      // Defines whether the input data model can be accessed by other data models
      // when transform. By default the value is false. If it is set to true, the
      // data model will be stored into Globally Shared Properties.
      isShared: true
    };
  };