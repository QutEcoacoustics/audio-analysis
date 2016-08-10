HTMLWidgets.widget({

  name: 'dviewer',

  type: 'output',

  factory: function(el, width, height) {

    // TODO: define shared variables for this instance

    return {

      renderValue: function(x) {
          data = JSON.parse(x.data);
          console.log(x)
          oi.init(el.id, JSON.parse(x.data), x.settings.group);

      },

      resize: function(width, height) {

        // TODO: code to re-render the widget with a new size

      }

    };
  }
});
