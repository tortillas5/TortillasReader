using System.Collections.Generic;
using System.Windows;

namespace TortillasReader
{
    /// <summary>
    /// Logique d'interaction pour GoToPage.xaml
    /// </summary>
    public partial class GoToPageWindow : Window
    {
        public GoToPageWindow(IEnumerable<int> range, int currentPage)
        {
            InitializeComponent();
            PageNumber.ItemsSource = range;
            PageNumber.SelectedValue = currentPage;
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