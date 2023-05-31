using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aspose.Zip;
using Aspose.Zip.Rar;
using Aspose.Zip.SevenZip;
using Aspose.Zip.Tar;
using Microsoft.Win32;

namespace TortillasReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Will be self instantiated so other classes can update the MainWindow values (like opacity).
        /// </summary>
        public static MainWindow AppWindow;

        /// <summary>
        /// Full path to the book.
        /// </summary>
        public string? CurrentFile { get; set; }

        /// <summary>
        /// Current open archive.
        /// </summary>
        public IArchive? Archive { get; set; }

        /// <summary>
        /// Current page number.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Value indicating if the image is zoomed.
        /// </summary>
        public bool ImageIsZoomed { get; set; }

        /// <summary>
        /// Value indicating if the mouse button is down.
        /// </summary>
        public bool MouseButtonIsDown { get; set; }

        /// <summary>
        /// Speed of the pages scroll.
        /// </summary>
        public int ScrollSpeed { get; set; }

        /// <summary>
        /// Enable the double page mode (for when books have images containing double pages).
        /// </summary>
        public bool DoublePageMode { get; set; }

        /// <summary>
        /// Enable comic mode (when reading american comics).
        /// </summary>
        public bool ComicMode { get; set; }

        #region Borders

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

        #endregion Borders

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
                Filter = "Archives de bandes dessinées|*.cbr;*.cbz;*.cbt;*.cb7"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadBook(openFileDialog.FileName);
                SetPage();
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
                Window windowGoToPage = new GoToPageWindow(Enumerable.Range(1, Archive.FileEntries.Count() - 2), CurrentPage + 1)
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

            windowDimmedMode.ShowDialog();
        }

        /// <summary>
        /// Change the scroll speed of the pages on click on a menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeScrollSpeed(object sender, RoutedEventArgs e)
        {
            var menu = (MenuItem)sender;
            SetScrollSpeed(int.Parse((string)menu.Header));
        }

        /// <summary>
        /// Switch from the single to the double page mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleDoublePageMode(object sender, RoutedEventArgs e)
        {
            var menu = (MenuItem)sender;
            SetDoublePageMode(menu.IsChecked);
            SetPage();
        }

        /// <summary>
        /// Swotch from the manga to comic mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleComicMode(object sender, RoutedEventArgs e)
        {
            var menu = (MenuItem)sender;
            SetComicMode(menu.IsChecked);
            SetPage();
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
                "Mode écran sombre : Assombrit l'écran pour le confort des yeux\n" +
                "Mode double pages : N'affiche qu'une image à la fois pour gérer les comics avec des doubles pages\n"
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

            AppWindow = this;

            // Register events handling.
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown);
            this.SizeChanged += Window_SizeChanged;

            SetDefaultValues();

            ResumeRead();
        }

        /// <summary>
        /// Set the default values of the app.
        /// </summary>
        private void SetDefaultValues()
        {
            // Set the page scrolling speed.
            ScrollSpeed = 1;
            ScrollSpeed1.IsChecked = true;
        }

        /// <summary>
        /// Events occuring when the app is closing.
        /// </summary>
        /// <param name="e">Event.</param>
        protected override void OnClosing(CancelEventArgs e)
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
                ScreenOpacity = this.Opacity,
                DoublePageMode = DoublePageMode,
                ComicMode = ComicMode
            });
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
                SetDoublePageMode(read.DoublePageMode);
                SetComicMode(read.ComicMode);

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
                if (ComicMode)
                {
                    if (e.Key == Key.Left)
                    {
                        SetPreviousPage();
                    }

                    if (e.Key == Key.Right)
                    {
                        SetNextPage();
                    }
                }
                else
                {
                    if (e.Key == Key.Left)
                    {
                        SetNextPage();
                    }

                    if (e.Key == Key.Right)
                    {
                        SetPreviousPage();
                    }
                }
            }
        }

        /// <summary>
        /// Set the next page.
        /// </summary>
        private void SetNextPage()
        {
            if (CurrentPage + 1 < Archive.FileEntries.Count() - 2)
            {
                CurrentPage++;

                if ((int)ScrollSpeed == 2 && CurrentPage + 1 < Archive.FileEntries.Count() - 2)
                {
                    CurrentPage++;
                }

                SetPage();
            }
        }

        /// <summary>
        /// Set the previous page.
        /// </summary>
        private void SetPreviousPage()
        {
            if (CurrentPage - 1 >= 0)
            {
                CurrentPage--;

                if ((int)ScrollSpeed == 2 && CurrentPage - 1 >= 0)
                {
                    CurrentPage--;
                }

                SetPage();
            }
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
        /// Set the double page mode.
        /// </summary>
        /// <param name="doublePageMode">Enabled or disabled.</param>
        private void SetDoublePageMode(bool doublePageMode)
        {
            DoublePageMode = doublePageMode;
            DoublePageModeMenu.IsChecked = doublePageMode;

            if (DoublePageMode)
            {
                SetScrollSpeed(1);

                ScrollSpeed2.IsEnabled = false;
                ScrollSpeed2.IsChecked = false;

                ScrollSpeed1.IsChecked = true;
            }
            else
            {
                ScrollSpeed2.IsEnabled = true;
            }
        }

        /// <summary>
        /// Set the comic mode.
        /// </summary>
        /// <param name="comicMode">Enabled or disabled.</param>
        private void SetComicMode(bool comicMode)
        {
            ComicModeMenu.IsChecked = comicMode;
            ComicMode = comicMode;
        }

        /// <summary>
        /// Handle windows resizing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double imagesWidth = ImagesCanvas.Children.Cast<Image>().Sum(i => i.ActualWidth);

            if (this.ActualWidth < imagesWidth)
            {
                this.Width = imagesWidth;
            }

            if (ImagesCanvas.ActualWidth != 0)
            {
                foreach (Image image in ImagesCanvas.Children)
                {
                    image.Height = ImagesCanvas.ActualHeight;

                    if (DoublePageMode)
                    {
                        double ratio = ImagesCanvas.ActualHeight * 100.0 / image.Source.Height / 100.0;
                        double actualWidth = image.Source.Width * ratio;

                        // Set images positions on the canvas.
                        Canvas.SetTop(image, 0);
                        Canvas.SetLeft(image, (ImagesCanvas.ActualWidth - actualWidth) / 2);
                    }
                    else
                    {
                        if (ComicMode)
                        {
                            // Set images positions on the canvas.
                            if (image.Name == "ImageLeft")
                            {
                                Canvas.SetTop(image, 0);
                                Canvas.SetLeft(image, ImagesCanvas.ActualWidth / 2);
                            }

                            if (image.Name == "ImageRight")
                            {
                                Canvas.SetTop(image, 0);
                                Canvas.SetRight(image, ImagesCanvas.ActualWidth / 2);
                            }
                        }
                        else
                        {
                            // Set images positions on the canvas.
                            if (image.Name == "ImageRight")
                            {
                                Canvas.SetTop(image, 0);
                                Canvas.SetLeft(image, ImagesCanvas.ActualWidth / 2);
                            }

                            if (image.Name == "ImageLeft")
                            {
                                Canvas.SetTop(image, 0);
                                Canvas.SetRight(image, ImagesCanvas.ActualWidth / 2);
                            }
                        }
                    }
                }
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

            if (File.Exists(CurrentFile))
            {
                Archive = GetArchive(CurrentFile);
                MainWindowElement.Title = "Tortillas reader - " + Path.GetFileName(fileName);
            }
            else
            {
                MessageBox.Show("Le fichier qui était en cours de lecture est introuvable :\n" + fileName);
            }
        }

        /// <summary>
        /// Set the images for the current page given.
        /// </summary>
        private void SetPage()
        {
            if (Archive != null && CurrentPage < Archive.FileEntries.Count() - 2 && CurrentPage >= 0)
            {
                ImagesCanvas.Children.Clear();

                if (DoublePageMode)
                {
                    // Get image source.
                    ImageSource rightImageSource = GetImage(Archive.FileEntries.Where(fe => fe.Length != 0).OrderBy(fe => fe.Name).ElementAt(CurrentPage));

                    Image imageRight = new()
                    {
                        Height = ImagesCanvas.ActualHeight,
                        Name = "ImageRight",
                        Source = rightImageSource
                    };

                    ImagesCanvas.Children.Add(imageRight);

                    double ratio = ImagesCanvas.ActualHeight * 100.0 / rightImageSource.Height / 100.0;
                    double actualWidth = rightImageSource.Width * ratio;

                    // Set images positions on the canvas.
                    Canvas.SetTop(imageRight, 0);
                    Canvas.SetLeft(imageRight, (ImagesCanvas.ActualWidth - actualWidth) / 2);
                }
                else
                {
                    // Get images sources.
                    ImageSource rightImageSource = GetImage(Archive.FileEntries.Where(fe => fe.Length != 0).OrderBy(fe => fe.Name).ElementAt(CurrentPage));
                    ImageSource leftImageSource = GetImage(Archive.FileEntries.Where(fe => fe.Length != 0).OrderBy(fe => fe.Name).ElementAt(CurrentPage + 1));

                    Image imageRight = new()
                    {
                        Height = ImagesCanvas.ActualHeight,
                        Name = "ImageRight",
                        Source = rightImageSource
                    };

                    Image imageLeft = new()
                    {
                        Height = ImagesCanvas.ActualHeight,
                        Name = "ImageLeft",
                        Source = leftImageSource
                    };

                    ImagesCanvas.Children.Add(imageRight);
                    ImagesCanvas.Children.Add(imageLeft);

                    if (ComicMode)
                    {
                        // Set images positions on the canvas.
                        Canvas.SetTop(imageLeft, 0);
                        Canvas.SetLeft(imageLeft, ImagesCanvas.ActualWidth / 2);

                        Canvas.SetTop(imageRight, 0);
                        Canvas.SetRight(imageRight, ImagesCanvas.ActualWidth / 2);
                    }
                    else
                    {
                        // Set images positions on the canvas.
                        Canvas.SetTop(imageRight, 0);
                        Canvas.SetLeft(imageRight, ImagesCanvas.ActualWidth / 2);

                        Canvas.SetTop(imageLeft, 0);
                        Canvas.SetRight(imageLeft, ImagesCanvas.ActualWidth / 2);
                    }
                }

                // Reset zoom
                ScaleTransform scaleTransform = new(1, 1);
                ImagesCanvas.RenderTransform = scaleTransform;

                ImageIsZoomed = false;

                // Set the page number.
                PageNumber.Content = (CurrentPage + 1).ToString() + " / " + (Archive.FileEntries.Count() - 2).ToString();
            }
        }

        /// <summary>
        /// Return an image from a compressed archive entry.
        /// </summary>
        /// <param name="archive">Rar archive entry.</param>
        /// <returns>Image.</returns>
        private static ImageSource GetImage(IArchiveFileEntry archive)
        {
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = new MemoryStream();
            archive.Extract(bitmapImage.StreamSource);
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
                Canvas canvas = (Canvas)sender;

                ImageIsZoomed = true;

                // Get cursor position.
                Point point = e.GetPosition(canvas);

                // Zoom on the cursor position
                ScaleTransform scaleTransform = new(2, 2, point.X, point.Y);
                canvas.RenderTransform = scaleTransform;
            }
        }

        /// <summary>
        /// Handle the zoom on an image.
        /// </summary>
        /// <param name="sender"></param>
        private void ZoomOnImage(object sender, MouseButtonEventArgs e)
        {
            // Get the clicked image.
            Canvas image = (Canvas)sender;

            ImageIsZoomed = !ImageIsZoomed;

            // Get cursor position.
            Point point = e.GetPosition(image);

            // Define if we are doing a zoom / unzoom.
            double zoom = ImageIsZoomed ? 2 : 1;

            // Zoom on the cursor position
            ScaleTransform scaleTransform = new(zoom, zoom, point.X, point.Y);
            image.RenderTransform = scaleTransform;
        }

        /// <summary>
        /// Return an IArchive from a file path.
        /// </summary>
        /// <param name="filePath">Path to a comic book file.</param>
        /// <returns>IArchive containing a manga.</returns>
        /// <exception cref="Exception">Format is unknow.</exception>
        public static IArchive GetArchive(string filePath)
        {
            // Needed for IBM437 error.
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return FileTypeDetector.GetFileType(filePath) switch
            {
                ".cbr" => new RarArchive(filePath),
                ".cbz" => new Archive(filePath),
                ".cbt" => new TarArchive(filePath),
                ".cb7" => new SevenZipArchive(filePath),
                _ => throw new Exception("Format du fichier non géré."),
            };
        }
    }
}