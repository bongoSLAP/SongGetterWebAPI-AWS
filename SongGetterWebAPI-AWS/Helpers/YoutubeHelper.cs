using SongGetterWebAPI_AWS.Models;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace SongGetterWebAPI_AWS.Helpers
{
    public class Youtube
    {
        
        public static async Task<PathInfo> DownloadVideoAsMp3(YoutubeClient youtube, string Url, PathInfo pathInfo)
        {
            StreamManifest streamManifest = null;

            try
            {
                //fetch stream manifest using YoutubeExplode library 
                streamManifest = await youtube.Videos.Streams.GetManifestAsync(Url);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                System.Diagnostics.Debug.WriteLine(pathInfo.FileName);
                pathInfo.IsError = true;
                return pathInfo;
            }

            //get audio as highest bitrate

            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();


            if (streamInfo != null)
            {
                System.Diagnostics.Debug.WriteLine("downloading...");
                pathInfo.IsError = false;

                //escape illegal chars in filename
                if (Regex.Match(pathInfo.FileName, @"[:|]").Success)
                {
                    pathInfo.FileName = Regex.Replace(pathInfo.FileName, @"[:|]", "-");
                }

                if (Regex.Match(pathInfo.FileName, @"[/?*\\~]").Success)
                {
                    pathInfo.FileName = Regex.Replace(pathInfo.FileName, @"[/?*\\~]", " ");
                }

                if (pathInfo.FileName.Contains('"'))
                {
                    pathInfo.FileName = pathInfo.FileName.Replace("\"", "'");
                }

                System.Diagnostics.Debug.WriteLine(pathInfo.FileName, "filename");

                //download video as mp3 using path data, log progress
                string fullPath = Path.Combine(pathInfo.FilePath, pathInfo.FileName);

                Progress<double> prog = new Progress<double>(p => System.Diagnostics.Debug.WriteLine($"Progress updated: {p}"));
                await youtube.Videos.Streams.DownloadAsync(streamInfo, fullPath, prog);
            }
            else
            {
                pathInfo.IsError = true;
            }

            return pathInfo;
        }
    }
}