// JScript source code

//contains calls to silverlight.js, example below loads Page.xaml
function createSilverlight()
{
	Silverlight.createObjectEx({
		source: "Page.xaml",
		parentElement: document.getElementById("SilverlightControlHost"),
		id: "SilverlightControl",
		properties: {
			width: "100%",
			height: "100%",
			version: "2.0",
			enableHtmlAccess: "true"
		},
		events: {onLoad:onSilverlightLoad}
	});
	   
	// Give the keyboard focus to the Silverlight control by default
    document.body.onload = function() {
      var silverlightControl = document.getElementById('SilverlightControl');
      if (silverlightControl)
      silverlightControl.focus();
    }

}

function onSilverlightLoad(sender, args)
{
    mediaControl = sender.Content.mediaControl;
    if (mediaControl != null)
    {
        mediaControl.Volume = 0.85;
        mediaControl.Finished = mediaControl_Finished;
    }
}

function mediaControl_Finished(sender, args)
{
alert('j');
}