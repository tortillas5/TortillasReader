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

        public IArchive Archive { get; set; }

        public PreviewThumbnailsWindow(IArchive archive)
        {
            InitializeComponent();

            Page = 10;
            Archive = archive;
        }

        private void ImagesCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // TODO GESTION SCROLL
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var files = Archive.FileEntries.Where(fe => fe.Length != 0).OrderBy(fe => fe.Name);

            int padding = 25;
            double height = (ImagesCanvas.ActualHeight / 2) - padding;
            double width = height / 2;

            for (int i = 0; i < 10; i++)
            {
                // Get image source.
                ImageSource imageSource = ImageHelper.GetImage(files.ElementAt(i + Page));

                Image image = new()
                {
                    Height = height,
                    Width = width,
                    Name = "Image_" + (i + Page),
                    Source = imageSource
                };

                ImagesCanvas.Children.Add(image);

                // Set images positions on the canvas.
                if (i < 5)
                {
                    Canvas.SetTop(image, padding);

                    double left;

                    if (i == 0)
                    {
                        left = (i * width) + padding;
                    }
                    else
                    {
                        left = (i * width) + (padding * i);
                    }

                    Canvas.SetLeft(image, left);
                }
                else
                {
                    Canvas.SetTop(image, height + padding);

                    double left;

                    if (i - 5 == 0)
                    {
                        left = ((i - 5) * width) + padding;
                    }
                    else
                    {
                        left = ((i - 5) * width) + (padding * (i - 5));
                    }

                    Canvas.SetLeft(image, left);
                }
            }
        }
    }
}