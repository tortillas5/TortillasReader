using Aspose.Zip;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TortillasReader.Windows
{
    /// <summary>
    /// Logique d'interaction pour PreviewThumbnailsWindow.xaml
    /// </summary>
    public partial class PreviewThumbnailsWindow : Window
    {
        public PreviewThumbnailsWindow(IArchive archive)
        {
            InitializeComponent();

            Archive = archive;
            Page = 0;
        }

        public IArchive Archive { get; set; }
        public int Page { get; set; }

        public int NbParPage { get; set; } = 10;

        public int MoitiePage { get { return NbParPage / 2; } }

        #region Mouse events

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // up
            if (e.Delta > 0)
            {
                if (Page > 0)
                {
                    Page--;
                }
            }
            else
            {
                // down
                Page++;
            }

            ShowThumbnails();
        }

        #endregion Mouse events

        #region Window events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ShowThumbnails();
        }

        #endregion Window events

        private void ShowThumbnails()
        {
            var files = Archive.FileEntries.Where(fe => fe.Length != 0).OrderBy(fe => fe.Name).Skip(Page * NbParPage).Take(NbParPage);

            double padding = 5;
            double width = (ImagesCanvas.ActualWidth / (NbParPage / 2)) - (padding * ((NbParPage / 2) + 1));
            double maxHeight = (ImagesCanvas.ActualHeight / 2) - (padding * 4);
            int i = 0;

            foreach (var file in files)
            {
                // Get image source.
                ImageSource imageSource = ImageHelper.GetImage(file);

                Image image = new()
                {
                    MaxHeight = maxHeight,
                    Width = width,
                    Name = "Image",
                    Source = imageSource
                };

                var imageBorder = new Border
                {
                    MaxHeight = maxHeight,
                    Height = image.Height,
                    Width = image.Width,
                    Background = Brushes.White,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Child = image
                };

                ImagesCanvas.Children.Add(imageBorder);

                // Set images positions on the canvas.
                if (i < MoitiePage)
                {
                    Canvas.SetTop(imageBorder, padding);

                    double left = (i * width) + (padding * (i + 1));

                    Canvas.SetLeft(imageBorder, left);
                }
                else
                {
                    Canvas.SetTop(imageBorder, (padding) + (ImagesCanvas.ActualHeight / 2));

                    double left = ((i - MoitiePage) * width) + (padding * ((i - MoitiePage) + 1));

                    Canvas.SetLeft(imageBorder, left);
                }

                i++;
            }

            PageNumber.Content = (Page + 1) * NbParPage;
        }
    }
}