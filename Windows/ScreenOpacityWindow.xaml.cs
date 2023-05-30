using System.Windows;

namespace TortillasReader
{
    /// <summary>
    /// Logique d'interaction pour ScreenOpacityWindow.xaml
    /// </summary>
    public partial class ScreenOpacityWindow : Window
    {
        public ScreenOpacityWindow(double opacity)
        {
            InitializeComponent();
            OpacitySlider.Value = 1 - opacity;
        }

        /// <summary>
        /// Handle the screen opacity slider.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MainWindow.AppWindow.Opacity = 1 - OpacitySlider.Value;
        }

        /// <summary>
        /// Handle the click on the OK button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Ok(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
    }
}