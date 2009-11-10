using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MQUTeR.Media.AudioPlayer.Code
{
    ///<summary>
    ///</summary>
    public class ScaleConverter : IValueConverter
    {

        #region IValueConverter Members

        /// <exception cref="ArgumentNullException"><c>parameter</c> is null.</exception>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

                if (parameter == null)
                {
                    throw new ArgumentNullException("parameter", "Parameter for this ScaleConverter cannot be null");
                }
                double v = Double.Parse(value.ToString());
                string[] parts = ((string) parameter).Split('|');
                double p = Double.Parse(parts[0]);

                return (v*p).ToString(parts[1] + parts[2]);
        }

        /// <exception cref="ArgumentNullException"><c>parameter</c> is null.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (parameter == null)
                {
                    throw new ArgumentNullException("parameter", "Parameter for this ScaleConverter cannot be null");
                }
                string valueToString = value.ToString().ToLower();
                string[] parts = ((string) parameter).Split('|');
                valueToString = valueToString.Replace(parts[2].ToLower(), "");
                double v = Double.Parse(valueToString);
                double p = Double.Parse(parts[0]);
                return v/p;
            }
            catch (Exception exception)
            {
                //we return exceptions to fire validation - just make sure the binding will really handle them
                //exception.Message = 
                return exception;
            }
        }

        #endregion
    }

    ///<summary>
    ///</summary>
    public class BooleanFillConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool doIt = (bool)value;
            if (doIt)
            {
                string val = parameter.ToString();
                val = val.Replace("#", "");

                byte a = System.Convert.ToByte("ff", 16);
                byte pos = 0;
                if (val.Length == 8)
                {
                    a = System.Convert.ToByte(val.Substring(pos, 2), 16);
                    pos = 2;
                }

                byte r = System.Convert.ToByte(val.Substring(pos, 2), 16);
                pos += 2;

                byte g = System.Convert.ToByte(val.Substring(pos, 2), 16);
                pos += 2;

                byte b = System.Convert.ToByte(val.Substring(pos, 2), 16);

                Color col = Color.FromArgb(a, r, g, b);

                return new SolidColorBrush(col);
            }
            else
            {
                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    ///<summary>
    ///</summary>
    public class ColorConverter : IValueConverter
    {

        #region IValueConverter Members

        //[ValueConversion(typeof(SolidColorBrush), typeof(string))]
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value.ToString();
            val = val.Replace("#", "");

            byte a = System.Convert.ToByte("ff", 16);
            byte pos = 0;
            if (val.Length == 8)
            {
                a = System.Convert.ToByte(val.Substring(pos, 2), 16);
                pos = 2;
            }

            byte r = System.Convert.ToByte(val.Substring(pos, 2), 16);
            pos += 2;

            byte g = System.Convert.ToByte(val.Substring(pos, 2), 16);
            pos += 2;

            byte b = System.Convert.ToByte(val.Substring(pos, 2), 16);

            Color col = Color.FromArgb(a, r, g, b);

            return new SolidColorBrush(col);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush val = value as SolidColorBrush;
            return "#" + val.Color.A.ToString() + val.Color.R.ToString() + val.Color.G.ToString() + val.Color.B.ToString();
        }

        #endregion
    }


    ///<summary>
    ///</summary>
    public class GetMeTheCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IList newVariable = value as IList;
            return newVariable.Count;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    ///<summary>
    ///</summary>
    public class ImANewOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int) value < 0) ? parameter.ToString() : value.ToString();
        }

        /// <exception cref="NotImplementedException"><c>NotImplementedException</c>.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    ///<summary>
    ///</summary>
    public class EmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (String.IsNullOrEmpty(value as string)) ? parameter.ToString() : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as string ?? string.Empty).Replace(parameter as string ?? string.Empty, string.Empty);
        }
    }

    public class RomanNumeralConverter : IValueConverter
    {
        /// <exception cref="ArgumentOutOfRangeException"><c>value</c> is out of range.</exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                int integer = (int) value;

                return (integer < 0) ? NumberToRoman(integer/-1) : integer.ToString();
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException("value", "must be a valid integer");
            }
        }

        /// <exception cref="NotImplementedException"><c>NotImplementedException</c>.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        // Converts an integer value into Roman numerals
        /// <exception cref="ArgumentException">Value must be in the range 0 - 3,999.</exception>
        public string NumberToRoman(int number)
        {
            // Validate
            if (number < 0 || number > 3999)
                throw new ArgumentException("Value must be in the range 0 - 3,999.");

            if (number == 0) return "N";

            // Set up key numerals and numeral pairs
            int[] values = new int[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            string[] numerals = new string[] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

            // Initialise the string builder
            StringBuilder result = new StringBuilder();

            // Loop through each of the values to diminish the number
            for (int i = 0; i < 13; i++)
            {
                // If the number being converted is less than the test value, append
                // the corresponding numeral or numeral pair to the resultant string
                while (number >= values[i])
                {
                    number -= values[i];
                    result.Append(numerals[i]);
                }
            }

            // Done
            return result.ToString();
        }
    }


    public class ColorRangeConverter : DependencyObject, IValueConverter
    {




        #region StartColor (DependencyProperty)

        /// <summary>
        /// The start color for the range
        /// </summary>
        public Color StartColor
        {
            get { return (Color)GetValue(StartColorProperty); }
            set { SetValue(StartColorProperty, value); }
        }
        public static readonly DependencyProperty StartColorProperty =
            DependencyProperty.Register("StartColor", typeof(Color), typeof(ColorRangeConverter),
            new PropertyMetadata(new Color(), new PropertyChangedCallback(OnStartColorChanged)));

        private static void OnStartColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorRangeConverter)d).OnStartColorChanged(e);
        }

        protected virtual void OnStartColorChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion


        #region EndColor (DependencyProperty)

        /// <summary>
        /// The end color for the range
        /// </summary>
        public Color EndColor
        {
            get { return (Color)GetValue(EndColorProperty); }
            set { SetValue(EndColorProperty, value); }
        }
        public static readonly DependencyProperty EndColorProperty =
            DependencyProperty.Register("StartColor", typeof(Color), typeof(ColorRangeConverter),
            new PropertyMetadata(new Color(), new PropertyChangedCallback(OnEndColorChanged)));

        private static void OnEndColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorRangeConverter)d).OnStartColorChanged(e);
        }

        protected virtual void OnEndColorChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion



        #region IValueConverter Members

        /// <exception cref="ArgumentException">Values for ColorRangeConverter can not be null</exception>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentException("Values for ColorRangeConverter can not be null");
            }
            double weight = (double)value;

            Color returnColor = new Color();
            returnColor.A = 255;
            returnColor.B = (byte)(((StartColor.B - EndColor.B) * weight) + EndColor.B);
            returnColor.R = (byte)(((StartColor.R - EndColor.R) * weight) + EndColor.R);
            returnColor.G = (byte)(((StartColor.G - EndColor.G) * weight) + EndColor.G);

            return new SolidColorBrush(returnColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
        #endregion
    }

    public class FontSizeConverter : DependencyObject, IValueConverter
    {




        public int SmallSize
        {
            get { return (int)GetValue(SmallSizeProperty); }
            set { SetValue(SmallSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SmallSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SmallSizeProperty =
            DependencyProperty.Register("SmallSize", typeof(int), typeof(FontSizeConverter), new PropertyMetadata(10));


        public int BigSize
        {
            get { return (int)GetValue(BigSizeProperty); }
            set { SetValue(BigSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BigSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BigSizeProperty =
            DependencyProperty.Register("BigSize", typeof(int), typeof(FontSizeConverter), new PropertyMetadata(16));


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentException("Values for ColorRangeConverter can not be null");
            }
            double weight = (double)value;

            return (int) ((weight*(BigSize - SmallSize)) + SmallSize);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
