using System.IO;

namespace TortillasReader
{
    public static class FileTypeDetector
    {
        /// <summary>
        /// Reliably return the type of a file.
        /// </summary>
        /// <param name="filePath">Path of a file.</param>
        /// <returns>Type of a file.</returns>
        public static string GetFileType(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[8]; // Read the first 8 bytes

            // Read the bytes into the buffer
            int bytesRead = stream.Read(buffer, 0, 8);

            // Check for file signatures
            if (IsRar(buffer, bytesRead))
            {
                return ".cbr";
            }
            else if (IsZip(buffer, bytesRead))
            {
                return ".cbz";
            }
            else if (IsTar(buffer, bytesRead))
            {
                return ".cbt";
            }
            else if (Is7z(buffer, bytesRead))
            {
                return ".cb7";
            }

            // If no match is found, return unknown type
            return "Unknown";
        }

        /// <summary>
        /// Check if a file is a 7zip.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bytesRead"></param>
        /// <returns></returns>
        private static bool Is7z(byte[] buffer, int bytesRead)
        {
            // 7z files start with the bytes "7z\xBC\xAF\x27\x1C"
            return bytesRead >= 6 &&
                buffer[0] == 0x37 &&
                buffer[1] == 0x7A &&
                buffer[2] == 0xBC &&
                buffer[3] == 0xAF &&
                buffer[4] == 0x27 &&
                buffer[5] == 0x1C;
        }

        /// <summary>
        /// Check if a file is a rar.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bytesRead"></param>
        /// <returns></returns>
        private static bool IsRar(byte[] buffer, int bytesRead)
        {
            // RAR files start with the bytes "Rar!\x1A\x07\x00"
            return bytesRead >= 7 &&
                buffer[0] == 0x52 &&
                buffer[1] == 0x61 &&
                buffer[2] == 0x72 &&
                buffer[3] == 0x21 &&
                buffer[4] == 0x1A &&
                buffer[5] == 0x07 &&
                buffer[6] == 0x00;
        }

        /// <summary>
        /// Check if a file is a tar.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bytesRead"></param>
        /// <returns></returns>
        private static bool IsTar(byte[] buffer, int bytesRead)
        {
            // TAR files start with various bytes
            return bytesRead >= 262 &&
                (buffer[257] == 0x75 || buffer[257] == 0x76) &&
                (buffer[258] == 0x73 || buffer[258] == 0x74) &&
                (buffer[259] == 0x74 || buffer[259] == 0x61) &&
                (buffer[260] == 0x72 || buffer[260] == 0x20) &&
                buffer[261] == 0x20 &&
                buffer[262] == 0x20;
        }

        /// <summary>
        /// Check if a file is a zip.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bytesRead"></param>
        /// <returns></returns>
        private static bool IsZip(byte[] buffer, int bytesRead)
        {
            // ZIP files start with the bytes "PK\x03\x04"
            return bytesRead >= 4 &&
                buffer[0] == 0x50 &&
                buffer[1] == 0x4B &&
                buffer[2] == 0x03 &&
                buffer[3] == 0x04;
        }
    }
}