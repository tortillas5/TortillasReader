﻿namespace TortillasReader
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
    }
}