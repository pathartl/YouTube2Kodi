using YouTube2Kodi.Models.YouTube;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace YouTube2Kodi.Models
{
    public class VideoInfo
    {
        public string Title { get; set; }
        public int? Season { get; set; }
        public int? Episode { get; set; }
        public DateTime AiredDate { get; set; }
        public string SeriesTitle { get; set; }
        public string Thumbnail { get; set; }
        public float Rating { get; set; }
        public string Description { get; set; }
        public string Filename { get; set; }

        public VideoInfo(DownloadInfo download)
        {
            Title = download.title;
            AiredDate = DateTime.ParseExact(download.upload_date, "yyyyMMdd", CultureInfo.InvariantCulture);
            Season = download.season_number.HasValue ? download.season_number.Value : AiredDate.Year;
            Episode = download.episode_number.HasValue ? download.episode_number.Value : 0;
            SeriesTitle = download.playlist_uploader;
            Thumbnail = download.thumbnail;
            Rating = download.average_rating * 2;
            Description = download.description;
            Filename = download._filename;
        }
    }
}
