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
        public int Page { get; set; }

        public PreviewThumbnailsWindow(IArchive Archive)
        {
            InitializeComponent();

            this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            this.Arrange(new Rect(0, 0, this.DesiredSize.Width, this.DesiredSize.Height));

            Page = 0;

            var files = Archive.FileEntries.Where(fe => fe.Length != 0).OrderBy(fe => fe.Name);

            for (int i = 0; i < 10; i++)
            {
                // Get image source.
                ImageSource imageSource = ImageHelper.GetImage(files.ElementAt(i));

                int padding = 50;
                double height = (ImagesCanvas.ActualHeight / 2) - (padding * 4);
                double width = height / 2;

                Image image = new()
                {
                    Height = height,
                    Width = width,
                    Name = "Image_" + i,
                    Source = imageSource
                };

                ImagesCanvas.Children.Add(image);

                // Set images positions on the canvas.
                if (i < 5)
                {
                    Canvas.SetTop(image, padding);
                }
                else
                {
                    Canvas.SetTop(image, height + padding);
                }

                if (i < 5)
                {
                    Canvas.SetLeft(image, (i * width) + padding);
                }
                else
                {
                    Canvas.SetLeft(image, ((i - 5) * width) + padding);
                }
            }
        }

        private void ImagesCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // TODO GESTION SCROLL
        }
    }
}