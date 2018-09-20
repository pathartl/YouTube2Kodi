using HandlebarsDotNet;
using Newtonsoft.Json;
using NYoutubeDL;
using NYoutubeDL.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YouTube2Kodi.Models;
using YouTube2Kodi.Models.YouTube;

namespace YouTube2Kodi.Service
{
    public class YouTubeService
    {
        private Config Config { get; set; }
        private YoutubeDL YouTubeDL { get; set; }
        private List<ArchiveItem> ArchiveItems { get; set; }

        BlockingCollection<string> DownloadQueue = new BlockingCollection<string>();

        public YouTubeService()
        {
            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            ArchiveItems = new List<ArchiveItem>();
        }

        public void InitYouTubeDL()
        {
            YouTubeDL = new YoutubeDL();

            YouTubeDL.Options.VideoSelectionOptions.DownloadArchive = Config.ArchiveFilename;

            YouTubeDL.StandardOutputEvent += (sender, output) => Console.WriteLine(output);
            YouTubeDL.StandardErrorEvent += (sender, errorOutput) => Console.WriteLine(errorOutput);
        }

        public void LoadArchiveItems()
        {
            int counter = 0;
            string line;

            // Read the file and add it line by line
            StreamReader file = new StreamReader(Config.ArchiveFilename);
            while ((line = file.ReadLine()) != null)
            {
                var splitLine = line.Split(' ');

                var item = new ArchiveItem();
                item.ServiceName = splitLine[0];
                item.VideoId = splitLine[1];

                ArchiveItems.Add(item);

                counter++;
            }

            file.Close();
        }

        public void DownloadAllChannels()
        {
            LoadArchiveItems();

            foreach (var channel in Config.Channels)
            {
                DownloadChannel(channel);
            }
        }

        private void DownloadVideoWorker(int workerId)
        {
            foreach (var videoUrl in DownloadQueue.GetConsumingEnumerable())
            {
                var videoDownloader = new YoutubeDL();

                videoDownloader.Options.FilesystemOptions.Output = Config.DownloadPath + "%(title)s.%(ext)s";
                videoDownloader.Options.SubtitleOptions.SubFormat = Enums.SubtitleFormat.srt;
                videoDownloader.Options.DownloadOptions.PlaylistReverse = true;
                videoDownloader.Options.FilesystemOptions.WriteInfoJson = true;
                videoDownloader.Options.SubtitleOptions.SubLang = Config.SubtitleLanguage;
                videoDownloader.Options.PostProcessingOptions.ConvertSubs = Enums.SubtitleFormat.srt;
                videoDownloader.Options.PostProcessingOptions.EmbedSubs = true;
                videoDownloader.Options.VideoSelectionOptions.DownloadArchive = Config.ArchiveFilename;
                videoDownloader.Options.VideoFormatOptions.MergeOutputFormat = Enums.VideoFormat.mkv;
                videoDownloader.Options.FilesystemOptions.RestrictFilenames = true;

                videoDownloader.StandardOutputEvent += (sender, output) => Console.WriteLine(output);
                videoDownloader.StandardErrorEvent += (sender, errorOutput) => Console.WriteLine(errorOutput);

                videoDownloader.VideoUrl = videoUrl;

                videoDownloader.Download();
            }
        }

        public void DownloadChannel(string channelUrl)
        {
            InitYouTubeDL();
            YouTubeDL.VideoUrl = channelUrl;

            var playlistDownloadInfo = (NYoutubeDL.Models.PlaylistDownloadInfo)YouTubeDL.GetDownloadInfo();

            DownloadQueue = new BlockingCollection<string>();

            Task[] downloadWorkers = new Task[Config.Threads];

            for (int i = 0; i < Config.Threads; i++)
            {
                int workerId = i;
                Task task = new Task(() => DownloadVideoWorker(workerId));
                downloadWorkers[i] = task;

                task.Start();
            } 

            foreach (var video in playlistDownloadInfo.Videos)
            {
                if (!ArchiveItems.Any(ai => ai.VideoId == video.Url))
                {
                    DownloadQueue.Add(video.Url);
                }
            }

            DownloadQueue.CompleteAdding();
            Task.WaitAll(downloadWorkers);

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
