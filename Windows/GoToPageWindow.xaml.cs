using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

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
            PageNumber.Focus();
            this.KeyDown += new KeyEventHandler(GoToPageWindow_KeyDown);
        }

        public int Result
        {
            get { return (int)PageNumber.SelectedValue; }
        }

        private void GoToPageWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Ok(null, null);
            }
        }

        private void Ok(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).DialogResult = true;
            Window.GetWindow(this).Close();
        }
    }
}