using System;
using System.Windows;
using System.Windows.Controls;

namespace MQUTeR.Media.AudioPlayer.Code
{
    public class DiscreteSlider : Slider
    {
        bool _busy;

        protected override void OnValueChanged(double oldValue, double newValue)
        {

            if (!_busy)
            {
                _busy = true;

                if (SmallChange != 0)
                {
                    double newDiscreteValue = (int)(Math.Round(newValue / SmallChange)) * SmallChange;

                    if (newDiscreteValue != DiscreteValue)
                    {
                        Value = newDiscreteValue;
                        base.OnValueChanged(DiscreteValue, newDiscreteValue);
                        DiscreteValue = newDiscreteValue;
                    }
                }
                else
                {
                    base.OnValueChanged(oldValue, newValue);
                }

                _busy = false;
            }
        }


        #region DiscreteValue (DependencyProperty)

        /// <summary>
        /// Returns the discrete value of the slider
        /// </summary>
        public double DiscreteValue
        {
            get { return (double)GetValue(DiscreteValueProperty); }
            set { SetValue(DiscreteValueProperty, value); }
        }
        public static readonly DependencyProperty DiscreteValueProperty =
            DependencyProperty.Register("DiscreteValue", typeof(double), typeof(DiscreteSlider),
              new PropertyMetadata(0.0));

        #endregion

    }

}