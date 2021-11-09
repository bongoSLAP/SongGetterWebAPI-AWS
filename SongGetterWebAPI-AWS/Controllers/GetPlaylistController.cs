using Microsoft.AspNetCore.Mvc;
using SongGetterWebAPI_AWS.Models;
using SongGetterWebAPI_AWS.Helpers;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using Microsoft.AspNetCore.Hosting;

namespace SongGetterWebAPI_AWS.GetPlaylistController
{
    [Route("api/[controller]")]
    public class GetPlaylistController : ControllerBase
    {
        readonly YoutubeClient youtube = new YoutubeClient();

        private async Task<PathInfo> QueryLib(SongRequest SongRequest)
        {
            System.Diagnostics.Debug.WriteLine("executing query lib");

            //get playlist
            var playlist = await youtube.Playlists.GetAsync(SongRequest.Url);

            //get playlist title from metadata
            var playlistTitle = playlist.Title;

            //create directory for folder to store audio files in playlist, call folder the title of playlist 
            PathInfo playlistPathInfo = new PathInfo();
            playlistPathInfo.FilePath = Path.Combine(@"C:\Users\jabri_000\source\repos\SongGetterWebAPI-AWS\SongGetterWebAPI-AWS\App_Data", playlistTitle);
            Directory.CreateDirectory(playlistPathInfo.FilePath);

            await foreach (var video in youtube.Playlists.GetVideosAsync(playlist.Id))
            {
                //get title of each video, create path using title as filename
                var videoTitle = video.Title;
                PathInfo videoPathInfo = new PathInfo();
                videoPathInfo.FilePath = playlistPathInfo.FilePath;
                videoPathInfo.FileName = videoTitle + ".mp3";

                //download videos
                await Task.Run(() => Youtube.DownloadVideoAsMp3(youtube, video.Url, videoPathInfo));
            }

            //create zip file in App_Data folder using playlist title
            PathInfo zipPathInfo = new PathInfo();
            zipPathInfo.FilePath = @"C:\Users\jabri_000\source\repos\SongGetterWebAPI-AWS\SongGetterWebAPI-AWS\App_Data\Zips";
            zipPathInfo.FileName = playlistTitle + ".zip";

            //zip file for transmission
            string sourceDirectory = playlistPathInfo.FilePath;
            string targetFile = Path.Combine(zipPathInfo.FilePath, zipPathInfo.FileName);

            if (!System.IO.File.Exists(targetFile))
            {
                await Task.Run(() => ZipFile.CreateFromDirectory(sourceDirectory, targetFile));
            }
            return zipPathInfo;

        }

        public async Task<HttpResponseMessage> GetPlaylistFromLib(string Url)
        {
            System.Diagnostics.Debug.WriteLine("executing GetPlaylistFromLib");

            //instantiate SongRequest class assigning json data sent in GET request from client
            SongRequest songRequest = new SongRequest();
            songRequest.Url = Url;
            songRequest.IsPlaylist = true;

            //create paths, download video(s) from YoutubeExplode library
            var pathInfo = await Task.Run(() => QueryLib(songRequest));

            var filePath = MyServer.MapPath($"~/App_Data/Zips/{ pathInfo.FileName}");
            return await Task.Run(() => ResponseHelper.ConstructResponse(pathInfo, filePath, "application/zip"));
        }
    }
}
