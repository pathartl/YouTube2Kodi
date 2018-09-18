using System;
using System.Collections.Generic;
using System.Text;

namespace YouTube2Kodi.Models
{
    public class Config
    {
        public IEnumerable<string> Channels { get; set; }
        public string DownloadPath { get; set; }
        public string DestinationPath { get; set; }
        public string ArchiveFilename { get; set; }
        public string SubtitleLanguage { get; set; }
        public IEnumerable<string> SupportedFileExtensions { get; set; }
    }
}