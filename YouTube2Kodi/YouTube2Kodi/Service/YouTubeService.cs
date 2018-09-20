using HandlebarsDotNet;
using Newtonsoft.Json;
using NYoutubeDL;
using NYoutubeDL.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YouTube2Kodi.Models;
using YouTube2Kodi.Models.YouTube;

namespace YouTube2Kodi.Service
{
    public class YouTubeService
    {
        private Config Config { get; set; }
        private YoutubeDL YouTubeDL { get; set; }

        public YouTubeService()
        {
            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
        }

        public void InitYouTubeDL()
        {
            YouTubeDL = new YoutubeDL();

            YouTubeDL.Options.FilesystemOptions.Output = Config.DownloadPath + "%(title)s.%(ext)s";
            YouTubeDL.Options.SubtitleOptions.SubFormat = Enums.SubtitleFormat.srt;
            YouTubeDL.Options.DownloadOptions.PlaylistReverse = true;
            YouTubeDL.Options.FilesystemOptions.WriteInfoJson = true;
            YouTubeDL.Options.SubtitleOptions.SubLang = Config.SubtitleLanguage;
            YouTubeDL.Options.PostProcessingOptions.ConvertSubs = Enums.SubtitleFormat.srt;
            YouTubeDL.Options.PostProcessingOptions.EmbedSubs = true;
            YouTubeDL.Options.VideoSelectionOptions.DownloadArchive = Config.ArchiveFilename;
            YouTubeDL.Options.VideoFormatOptions.MergeOutputFormat = Enums.VideoFormat.mkv;
            YouTubeDL.Options.FilesystemOptions.RestrictFilenames = true;

            YouTubeDL.StandardOutputEvent += (sender, output) => Console.WriteLine(output);
            YouTubeDL.StandardErrorEvent += (sender, errorOutput) => Console.WriteLine(errorOutput);
        }

        public void DownloadAllChannels()
        {
            foreach (var channel in Config.Channels)
            {
                DownloadChannel(channel);
            }
        }

        public void DownloadChannel(string channelUrl)
        {
            InitYouTubeDL();
            YouTubeDL.VideoUrl = channelUrl;
            YouTubeDL.Download();

            var files = Directory.EnumerateFiles(Config.DownloadPath, "*.*", SearchOption.AllDirectories)
                .Where(f => f.EndsWith(".json"));

            var infoList = new List<VideoInfo>();

            foreach (var file in files)
            {
                var data = File.ReadAllText(file);

                var downloadInfo = JsonConvert.DeserializeObject<DownloadInfo>(data);
                infoList.Add(new VideoInfo(downloadInfo));
            }

            foreach (var info in infoList)
            {
                var fileInfo = new FileInfo(info.Filename);
                var destinationDirectory = String.Format("{0}/{1}/Season {2}", Config.DestinationPath, info.SeriesTitle, info.Season);

                Directory.CreateDirectory(destinationDirectory);

                if (info.Episode == 0)
                {
                    var existingSeasonEpisodes = Directory.EnumerateFiles(destinationDirectory, "*.*", SearchOption.AllDirectories)
                        .Where(f => Config.SupportedFileExtensions.Any(x => f.EndsWith("." + x)));

                    info.Episode = existingSeasonEpisodes.Count() + 1;
                }

                var videoDestination = String.Format(
                    "{0}/S{1}E{2}{3}",
                    destinationDirectory,
                    info.Season.Value.ToString("D2"),
                    info.Episode.Value.ToString("D3"),
                    fileInfo.Extension
                );

                var nfoDestination = String.Format(
                    "{0}/S{1}E{2}{3}",
                    destinationDirectory,
                    info.Season.Value.ToString("D2"),
                    info.Episode.Value.ToString("D3"),
                    ".nfo"
                );

                var template = Handlebars.Compile(File.ReadAllText("Template.nfo"));
                var compiledTemplate = template(new VideoInfoViewModel(info));

                try
                {
                    File.Move(info.Filename, videoDestination);
                    File.WriteAllText(nfoDestination, compiledTemplate);

                    string[] leftoverFiles = Directory.GetFiles(Config.DownloadPath, Path.GetFileNameWithoutExtension(fileInfo.Name) + ".*");

                    foreach (var leftoverFile in leftoverFiles)
                    {
                        File.Delete(leftoverFile);
                    }
                } catch (Exception)
                {

                }
            }
        }
    }
}
