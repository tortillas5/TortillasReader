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

            int i = 0;

            foreach (var file in Archive.FileEntries.Where(fe => fe.Length != 0).OrderBy(fe => fe.Name))
            {
                // Get image source.
                ImageSource imageSource = ImageHelper.GetImage(file);

                int height = 100;
                double width = 50;

                Image image = new()
                {
                    Height = height,
                    Width = width,
                    Name = "Image_" + i,
                    Source = imageSource
                };

                ImagesCanvas.Children.Add(image);


                // Set images positions on the canvas.
                Canvas.SetTop(image, 0);
                Canvas.SetLeft(image, 0);

                i++;
            }
        }

        private void ImagesCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // TODO GESTION SCROLL
        }
    }
}
