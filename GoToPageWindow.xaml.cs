using System.Collections.Generic;
using System.Windows;

namespace TortillasReader
{
    /// <summary>
    /// Logique d'interaction pour GoToPage.xaml
    /// </summary>
    public partial class GoToPageWindow : Window
    {
        public GoToPageWindow(IEnumerable<int> range)
        {
            InitializeComponent();
            PageNumber.ItemsSource = range;
            PageNumber.SelectedValue = 1;
        }

        public int Result
        {
            get { return (int)PageNumber.SelectedValue; }
        }

        private void Ok(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).DialogResult = true;
            Window.GetWindow(this).Close();
        }
    }
}