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
        /// <summary>
        /// Full path to the book.
        /// </summary>
        public string CurrentFile { get; set; }

        /// <summary>
        /// Current open archive.
        /// </summary>
        public RarArchive Archive { get; set; }

        /// <summary>
        /// Current page number.
        /// </summary>
        public int CurrentPage { get; set; }

        #region Services

        /// <summary>
        /// Handler used to save datas to the disk.
        /// </summary>
        public JsonHandler JsonHandler { get; set; } = new JsonHandler();

        #endregion Services

        public MainWindow()
        {
            InitializeComponent();

            // Register events handling.
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
            this.SizeChanged += Window_SizeChanged;

            ResumeRead();
        }

        /// <summary>
        /// Events occuring when the app is closing.
        /// </summary>
        /// <param name="e">Event.</param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            // Get and delete last saves.
            var reads = JsonHandler.GetEntities<ResumeReading>();

            foreach (var read in reads)
            {
                JsonHandler.Remove<ResumeReading>(read);
            }

            // Save current book / page number.
            JsonHandler.Add<ResumeReading>(new ResumeReading() { CurrentPage = CurrentPage - 1, LastBook = CurrentFile });
        }

        /// <summary>
        /// Reopen the last read book.
        /// </summary>
        private void ResumeRead()
        {
            var read = JsonHandler.GetEntities<ResumeReading>().FirstOrDefault();

            if (read != null && !string.IsNullOrWhiteSpace(read.LastBook))
            {
                LoadBook(read.LastBook);
                CurrentPage = read.CurrentPage;

                RightPage.Source = GetImage(Archive.Entries[CurrentPage]);
                CurrentPage++;
                LeftPage.Source = GetImage(Archive.Entries[CurrentPage]);

                SetPageNumber();
            }
        }

        /// <summary>
        /// Handle left / right keys.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Handle windows resizing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (RightPage.ActualWidth != 0)
            {
                RightPage.Margin = new Thickness(RightPage.ActualWidth, 35, 0, 0);
                LeftPage.Margin = new Thickness(0, 35, LeftPage.ActualWidth, 0);
            }
        }

        /// <summary>
        /// Handle the click on "charger un fichier".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Cbr files (*.cbr)|*.cbr"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadBook(openFileDialog.FileName);
            }
        }

        /// <summary>
        /// Load a book on the screen.
        /// </summary>
        /// <param name="fileName">Full path to the book to load.</param>
        private void LoadBook(string fileName)
        {
            CurrentPage = 0;
            CurrentFile = fileName;

            Archive = new(CurrentFile);

            RightPage.Source = GetImage(Archive.Entries[CurrentPage]);
            CurrentPage++;
            LeftPage.Source = GetImage(Archive.Entries[CurrentPage]);

            SetPageNumber();

            GoToPageNumber.ItemsSource = Enumerable.Range(1, Archive.Entries.Count - 2);

            MainWindowElement.Title = "Tortillas reader - " + System.IO.Path.GetFileName(fileName);
        }

        private void LoadNextPage()
        {
            if (Archive != null && CurrentPage < Archive.Entries.Count - 2)
            {
                RightPage.Source = GetImage(Archive.Entries[CurrentPage]);
                CurrentPage++;
                LeftPage.Source = GetImage(Archive.Entries[CurrentPage]);

                SetPageNumber();
            }
        }

        private void LoadPreviousPage()
        {
            if (Archive != null && CurrentPage != 1)
            {
                CurrentPage--;
                LeftPage.Source = GetImage(Archive.Entries[CurrentPage]);
                RightPage.Source = GetImage(Archive.Entries[CurrentPage - 1]);

                SetPageNumber();
            }
        }

        /// <summary>
        /// Set the page number on the page counter.
        /// </summary>
        private void SetPageNumber()
        {
            PageNumber.Content = CurrentPage.ToString() + " / " + (Archive.Entries.Count - 2).ToString();
        }

        /// <summary>
        /// Return an image from a compressed archive entry.
        /// </summary>
        /// <param name="rarArchive">Rar archive entry.</param>
        /// <returns>Image.</returns>
        private static ImageSource GetImage(RarArchiveEntry rarArchive)
        {
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = new MemoryStream();
            rarArchive.Extract(bitmapImage.StreamSource);
            bitmapImage.StreamSource.Position = 0;
            bitmapImage.EndInit();

            bitmapImage.StreamSource.Close();
            bitmapImage.StreamSource.Dispose();

            return bitmapImage;
        }

        /// <summary>
        /// Handle the selection of a new page number in the combobox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToPageNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentPage = (int)GoToPageNumber.SelectedItem - 1;

            RightPage.Source = GetImage(Archive.Entries[CurrentPage]);
            CurrentPage++;
            LeftPage.Source = GetImage(Archive.Entries[CurrentPage]);

            SetPageNumber();

            // Need to unfocus the combobox or else the left / right keys won't work.
            LoadFile.Focus();
        }
    }
}
