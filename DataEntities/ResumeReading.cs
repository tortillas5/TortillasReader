namespace TortillasReader
{
    /// <summary>
    /// Class representing the last book read, to reopen the last page/book.
    /// </summary>
    public class ResumeReading : DefaultEntity
    {
        /// <summary>
        /// Position of the page of the last read book.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Position on the disk of the last comic read.
        /// </summary>
        public string LastBook { get; set; }

        /// <summary>
        /// Scroll speed that was set while reading the last book.
        /// </summary>
        public int ScrollSpeed { get; set; }

        /// <summary>
        /// The opacity of the screen that was set by the user.
        /// </summary>
        public double ScreenOpacity { get; set; }
    }
}