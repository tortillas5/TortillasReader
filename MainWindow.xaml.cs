using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aspose.Zip.Rar;
using Microsoft.Win32;

namespace TortillasReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Full path to the book.
        /// </summary>
        public string? CurrentFile { get; set; }

        /// <summary>
        /// Current open archive.
        /// </summary>
        public RarArchive? Archive { get; set; }

        /// <summary>
        /// Current page number.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Value indicating if left image is zoomed.
        /// </summary>
        public bool LeftImageIsZoomed { get; set; } = false;

        /// <summary>
        /// Value indicating if right image is zoomed.
        /// </summary>
        public bool RightImageIsZoomed { get; set; } = false;

        /// <summary>
        /// Value indicating if the mouse button is down.
        /// </summary>
        public bool MouseButtonIsDown { get; set; } = false;

        /// <summary>
        /// Speed of the pages scroll.
        /// </summary>
        public int ScrollSpeed { get; set; } = 1;

        /// <summary>
        /// Zindex value of the right image.
        /// </summary>
        private int borderRightZIndex;

        /// <summary>
        /// Zindex value of the left image.
        /// </summary>
        private int borderLeftZIndex;

        /// <summary>
        /// Getter / setter of the property <see cref="borderRightZIndex"/>.
        /// </summary>
        public int BorderRightZIndex
        {
            get { return borderRightZIndex; }
            set
            {
                borderRightZIndex = value;
                RaisePropertyChanged(nameof(BorderRightZIndex));
            }
        }

        /// <summary>
        /// Getter / setter of the property <see cref="borderLeftZIndex"/>.
        /// </summary>
        public int BorderLeftZIndex
        {
            get { return borderLeftZIndex; }
            set
            {
                borderLeftZIndex = value;
                RaisePropertyChanged(nameof(BorderLeftZIndex));
            }
        }

        #region Services

        /// <summary>
        /// Handler used to save datas to the disk.
        /// </summary>
        public JsonHandler JsonHandler { get; set; } = new JsonHandler();

        #endregion Services

        #region Notifications

        /// <summary>
        /// Event notifying of properties changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };

        /// <summary>
        /// Notify that a property have changed.
        /// </summary>
        /// <param name="propertyName">Name of the property changed.</param>
        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Notifications

        #region Menus

        /// <summary>
        /// Show the open file dialog.
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
        /// Open the windows to go to a specific page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToPage_Click(object sender, RoutedEventArgs e)
        {
            if (Archive != null)
            {
                Window windowGoToPage = new GoToPageWindow(Enumerable.Range(1, Archive.Entries.Count - 2), CurrentPage + 1)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                var dialogResult = windowGoToPage.ShowDialog();

                if (dialogResult.HasValue && dialogResult.Value)
                {
                    if (windowGoToPage is GoToPageWindow content)
                    {
                        CurrentPage = content.Result - 1;
                        SetPage();
                    }
                }
            }
        }

        /// <summary>
        /// Open the windows to configure the dimmed mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DimmedMode_Click(object sender, RoutedEventArgs e)
        {
            Window windowDimmedMode = new ScreenOpacityWindow(this.Opacity)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var dialogResult = windowDimmedMode.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                if (windowDimmedMode is ScreenOpacityWindow content)
                {
                    this.Opacity = content.Opacity;
                }
            }
        }

        /// <summary>
        /// Show the list of commands of the app.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandList_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Flèches de gauche / droite : Changer de page\n" +
                "Double cliquer : Zoom sur la souris\n" +
                "Cliquer et déplacer pendant le zoom : Se déplacer sur l'image\n" +
                "Charger un fichier : Charge un livre\n" +
                "Aller à la page : Se déplacer à la page sélectionnée\n" +
                "Vitesse de défilement des pages : Déplacer les pages par 1 ou 2 à la fois\n" +
                "Mode écran sombre : Assombrit l'écran pour le confort des yeux\n"
                , "Liste des commandes");
        }

        /// <summary>
        /// Show infos about the app.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Créé par Tortillas - 2023\nurl : https://github.com/tortillas5/TortillasReader", "A propos");
        }

        /// <summary>
        /// Quit the app.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion Menus

        /// <summary>
        /// Initialize a new instance of the class <see cref="MainWindow"/>.
        /// </summary>
        public MainWindow()
        {
            DataContext = this;
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
            JsonHandler.Add<ResumeReading>(new ResumeReading()
            {
                CurrentPage = CurrentPage,
                LastBook = CurrentFile ?? string.Empty,
                ScrollSpeed = ScrollSpeed,
                ScreenOpacity = this.Opacity
            }); ;
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
                SetScrollSpeed(read.ScrollSpeed);
                this.Opacity = read.ScreenOpacity;

                SetPage();
            }
        }

        /// <summary>
        /// Handle left / right keys.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Archive != null)
            {
                if (e.Key == Key.Left && CurrentPage + 1 < Archive.Entries.Count - 2)
                {
                    CurrentPage++;

                    if ((int)ScrollSpeed == 2 && CurrentPage + 1 < Archive.Entries.Count - 2)
                    {
                        CurrentPage++;
                    }

                    SetPage();
                }

                if (e.Key == Key.Right && CurrentPage - 1 >= 0)
                {
                    CurrentPage--;

                    if ((int)ScrollSpeed == 2 && CurrentPage - 1 >= 0)
                    {
                        CurrentPage--;
                    }

                    SetPage();
                }
            }
        }

        /// <summary>
        /// Change the scroll speed of the pages on click on a menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ChangeScrollSpeed(object sender, RoutedEventArgs e)
        {
            var menu = (MenuItem)sender;
            SetScrollSpeed(int.Parse((string)menu.Header));
        }

        /// <summary>
        /// Set the scroll speed.
        /// </summary>
        /// <param name="speed">Speed of the scroll.</param>
        public void SetScrollSpeed(int speed)
        {
            ScrollSpeed = speed;

            switch (ScrollSpeed)
            {
                case 1:
                    ScrollSpeed1.IsChecked = true;
                    ScrollSpeed2.IsChecked = false;
                    break;

                case 2:
                    ScrollSpeed1.IsChecked = false;
                    ScrollSpeed2.IsChecked = true;
                    break;
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
                RightPage.Margin = new Thickness(RightPage.ActualWidth, 18, 0, 0);
                LeftPage.Margin = new Thickness(0, 18, LeftPage.ActualWidth, 0);
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

            SetPage();

            MainWindowElement.Title = "Tortillas reader - " + System.IO.Path.GetFileName(fileName);
        }

        /// <summary>
        /// Set the images for the current page given.
        /// </summary>
        private void SetPage()
        {
            if (Archive != null && CurrentPage < Archive.Entries.Count - 2 && CurrentPage >= 0)
            {
                RightPage.Source = GetImage(Archive.Entries[CurrentPage]);
                LeftPage.Source = GetImage(Archive.Entries[CurrentPage + 1]);

                PageNumber.Content = (CurrentPage + 1).ToString() + " / " + (Archive.Entries.Count - 2).ToString();
            }
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
        /// Handle the mouse click down event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // On double click
                ZoomOnImage(sender, e);
            }
            else
            {
                MouseButtonIsDown = true;
            }
        }

        /// <summary>
        /// Handle the mouse click up event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseButtonIsDown = false;
        }

        /// <summary>
        /// Handle the movement of the mouse.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (MouseButtonIsDown)
            {
                Image image = (Image)sender;

                // Set wich image is zoomed / unzoomed.
                switch (image.Name)
                {
                    case "RightPage":
                        RightImageIsZoomed = true;

                        // Set the right page over the left one.
                        BorderRightZIndex = 100;
                        BorderLeftZIndex = 0;
                        break;

                    case "LeftPage":
                        LeftImageIsZoomed = true;

                        // Set the left page over the right one.
                        BorderLeftZIndex = 100;
                        BorderRightZIndex = 0;
                        break;
                }

                // Get cursor position.
                Point point = e.GetPosition(image);

                // Zoom on the cursor position
                ScaleTransform scaleTransform = new(2, 2, point.X, point.Y);
                image.RenderTransform = scaleTransform;
            }
        }

        /// <summary>
        /// Handle the zoom on an image.
        /// </summary>
        /// <param name="sender"></param>
        private void ZoomOnImage(object sender, MouseButtonEventArgs e)
        {
            // Get the clicked image.
            Image image = (Image)sender;
            bool zoomed = false;

            // Set wich image is zoomed / unzoomed.
            switch (image.Name)
            {
                case "RightPage":
                    RightImageIsZoomed = !RightImageIsZoomed;
                    zoomed = RightImageIsZoomed;

                    // Set the right page over the left one.
                    BorderRightZIndex = 100;
                    BorderLeftZIndex = 0;
                    break;

                case "LeftPage":
                    LeftImageIsZoomed = !LeftImageIsZoomed;
                    zoomed = LeftImageIsZoomed;

                    // Set the left page over the right one.
                    BorderLeftZIndex = 100;
                    BorderRightZIndex = 0;
                    break;
            }

            // Get cursor position.
            Point point = e.GetPosition(image);

            // Define if we are doing a zoom / unzoom.
            double zoom = zoomed ? 2 : 1;

            // Zoom on the cursor position
            ScaleTransform scaleTransform = new(zoom, zoom, point.X, point.Y);
            image.RenderTransform = scaleTransform;
        }
    }
}