using System;
using System.Windows;

namespace TortillasReader
{
    /// <summary>
    /// Logique d'interaction pour ScreenOpacityWindow.xaml
    /// </summary>
    public partial class ScreenOpacityWindow : Window
    {
        public double MainWindowOpacity { get; set; }

        public ScreenOpacityWindow(double opacity)
        {
            InitializeComponent();
            MainWindowOpacity = opacity;
            this.Opacity = opacity;
            OpacitySlider.Value = 1 - opacity;
        }

        /// <summary>
        /// Handle the screen opacity slider.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Opacity = 1 - OpacitySlider.Value;
        }

        private void Ok(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).DialogResult = true;
            Window.GetWindow(this).Close();
        }
    }
}