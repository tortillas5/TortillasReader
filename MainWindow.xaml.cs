using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Aspose.Zip;
using Aspose.Zip.Rar;
using Microsoft.Win32;

namespace TortillasReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string CurrentFile { get; set; }

        public RarArchive Archive { get; set; }

        public int CurrentPage { get; set; } = 0;

        public MainWindow()
        {
            InitializeComponent();

            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
            this.SizeChanged += Window_SizeChanged;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                LoadNextPage();
            }

            if (e.Key == Key.Right)
            {
                LoadPreviousPage();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (RightPage.ActualWidth != 0)
            {
                RightPage.Margin = new Thickness(RightPage.ActualWidth, 35, 0, 0);
                LeftPage.Margin = new Thickness(0, 35, LeftPage.ActualWidth, 0);
            }
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Cbr files (*.cbr)|*.cbr"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                CurrentFile = openFileDialog.FileName;

                Archive = new(CurrentFile);

                RightPage.Source = GetImage(Archive.Entries[CurrentPage]);
                CurrentPage++;
                LeftPage.Source = GetImage(Archive.Entries[CurrentPage]);
            }
        }

        private void LoadNextPage()
        {
            RightPage.Source = GetImage(Archive.Entries[CurrentPage]);
            CurrentPage++;
            LeftPage.Source = GetImage(Archive.Entries[CurrentPage]);
        }

        private void LoadPreviousPage()
        {
            if (CurrentPage != 1)
            {
                CurrentPage--;
                LeftPage.Source = GetImage(Archive.Entries[CurrentPage]);
                RightPage.Source = GetImage(Archive.Entries[CurrentPage - 1]);
            }
        }

        private static ImageSource GetImage(RarArchiveEntry rarArchive)
        {
            Stream image = rarArchive.Open();
            
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = image;
            bitmap.EndInit();

            return bitmap;
        }
    }
}
