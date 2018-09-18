using YouTube2Kodi.Models.YouTube;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace YouTube2Kodi.Models
{
    public class VideoInfoViewModel
    {
        public string Title { get; set; }
        public int? Season { get; set; }
        public int? Episode { get; set; }
        public string AiredDate { get; set; }
        public string SeriesTitle { get; set; }
        public string Thumbnail { get; set; }
        public string Rating { get; set; }
        public string Description { get; set; }
        public string Filename { get; set; }

        public VideoInfoViewModel(VideoInfo video)
        {
            Title = video.Title;
            Season = video.Season;
            Episode = video.Episode;
            AiredDate = video.AiredDate.ToString("yyyy-MM-dd");
            SeriesTitle = video.SeriesTitle;
            Thumbnail = video.Thumbnail;
            Rating = video.Rating.ToString();
            Description = video.Description;
            Filename = video.Filename;
        }
    }
}
