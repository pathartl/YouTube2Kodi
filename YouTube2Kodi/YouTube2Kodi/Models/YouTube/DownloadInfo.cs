using System;
using System.Collections.Generic;
using System.Text;

namespace YouTube2Kodi.Models.YouTube
{
    public class DownloadInfo
    {
        public string playlist_uploader { get; set; }
        public string uploader { get; set; }
        public string title { get; set; }
        public string thumbnail { get; set; }
        public string upload_date { get; set; }
        public string description { get; set; }
        public float average_rating { get; set; }
        public int? season_number { get; set; }
        public int? episode_number { get; set; }
        public string _filename { get; set; }
    }
}
