using Aspose.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace TortillasReader.Windows
{
    /// <summary>
    /// Logique d'interaction pour PreviewThumbnailsWindow.xaml
    /// </summary>
    public partial class PreviewThumbnailsWindow : Window
    {
        public PreviewThumbnailsWindow(IArchive Archive)
        {
            InitializeComponent();

            foreach (var file in Archive.FileEntries)
            {
                // Get image source.
                ImageSource imageSource = ImageHelper.GetImage(file);

                Image image = new()
                {
                    Height = ImagesCanvas.ActualHeight,
                    Name = file.Name,
                    Source = imageSource
                };

                ImagesCanvas.Children.Add(image);

                double ratio = ImagesCanvas.ActualHeight * 100.0 / imageSource.Height / 100.0;
                double actualWidth = imageSource.Width * ratio;

                // Set images positions on the canvas.
                Canvas.SetTop(image, 0);
                Canvas.SetLeft(image, (ImagesCanvas.ActualWidth - actualWidth) / 2);
            }
        }

        private void ImagesCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // TODO GESTION SCROLL
        }
    }
}
