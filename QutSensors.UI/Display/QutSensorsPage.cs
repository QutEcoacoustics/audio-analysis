// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QutSensorsPage.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the QutSensorsPage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace QutSensors.UI.Display
{
    using System;
    using System.Web.UI;

    /// <summary>
    /// Qut Sensors page base class.
    /// </summary>
    public class QutSensorsPage : Page
    {
        protected override void OnPreInit(EventArgs e)
        {
            GetToolbarButtons();
            base.OnPreInit(e);
        }

        protected virtual void GetToolbarButtons()
        {
        }

        protected void EnsureControlsCreated()
        {
            var m = Master;
        }
    }
}
