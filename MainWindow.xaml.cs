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

        public BitmapImage LeftImage { get; set; }

        public BitmapImage RightImage { get; set; }

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

                RightPage.Source = GetImage(Archive.Entries[CurrentPage], Side.Right);
                CurrentPage++;
                LeftPage.Source = GetImage(Archive.Entries[CurrentPage], Side.Left);
            }
        }

        private void LoadNextPage()
        {
            if (CurrentPage < Archive.Entries.Count - 2)
            {
                RightPage.Source = GetImage(Archive.Entries[CurrentPage], Side.Right);
                CurrentPage++;
                LeftPage.Source = GetImage(Archive.Entries[CurrentPage], Side.Left);
            }
        }

        private void LoadPreviousPage()
        {
            if (CurrentPage != 1)
            {
                CurrentPage--;
                LeftPage.Source = GetImage(Archive.Entries[CurrentPage], Side.Left);
                RightPage.Source = GetImage(Archive.Entries[CurrentPage - 1], Side.Right);
            }
        }

        private ImageSource GetImage(RarArchiveEntry rarArchive, Side side)
        {
            if (side == Side.Left)
            {
                LeftImage = new BitmapImage();
                LeftImage.BeginInit();
                LeftImage.CacheOption = BitmapCacheOption.OnLoad;
                LeftImage.StreamSource = new MemoryStream();
                rarArchive.Extract(LeftImage.StreamSource);
                LeftImage.StreamSource.Position = 0;
                LeftImage.EndInit();

                LeftImage.StreamSource.Close();
                LeftImage.StreamSource.Dispose();

                return LeftImage;
            }

            if (side == Side.Right)
            {
                RightImage = new BitmapImage();
                RightImage.BeginInit();
                RightImage.CacheOption = BitmapCacheOption.OnLoad;
                RightImage.StreamSource = new MemoryStream();
                rarArchive.Extract(RightImage.StreamSource);
                RightImage.StreamSource.Position = 0;
                RightImage.EndInit();

                RightImage.StreamSource.Close();
                RightImage.StreamSource.Dispose();

                return RightImage;
            }

            throw new Exception("This side is not recognised.");
        }
    }

    public enum Side
    {
        Left = 1,
        Right = 2,
    }
}
