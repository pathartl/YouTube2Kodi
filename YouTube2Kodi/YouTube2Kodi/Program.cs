using System;
using YouTube2Kodi.Service;

namespace YouTube2Kodi
{
    class Program
    {
        static void Main(string[] args)
        {
            var youTubeService = new YouTubeService();
            youTubeService.DownloadAllChannels();

            Console.WriteLine("Done");
        }
    }
}
