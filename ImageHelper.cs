using Aspose.Zip;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TortillasReader
{
    public class ImageHelper
    {
        /// <summary>
        /// Return an image from a compressed archive entry.
        /// </summary>
        /// <param name="archive">An archive entry.</param>
        /// <returns>Image.</returns>
        public static ImageSource GetImage(IArchiveFileEntry archive)
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
    }
}