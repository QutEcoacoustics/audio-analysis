namespace System.Windows.Controls
{
    public static class SelectionHelpers
    {
        public static double Top(this FrameworkElement frameworkElement)
        {
            return Canvas.GetTop(frameworkElement);
        }
        public static double Left(this FrameworkElement frameworkElement)
        {
            return Canvas.GetLeft(frameworkElement);
        }
        public static double Bottom(this FrameworkElement frameworkElement)
        {
            return Top(frameworkElement) + frameworkElement.ActualHeight;
        }
        public static double Right(this FrameworkElement frameworkElement)
        {
            return Left(frameworkElement) + frameworkElement.ActualWidth;
        }
        public static Rect Dimensions(this FrameworkElement frameworkElement)
        {
            return new Rect(frameworkElement.Left(), frameworkElement.Top(), frameworkElement.ActualWidth, frameworkElement.ActualHeight);
        }
     
    }
}
