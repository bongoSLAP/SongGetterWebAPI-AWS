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

namespace SongGetterWebAPI_AWS.GetSongController
{
    [Route("api/[controller]")]
    public class GetSongController : ControllerBase
    {
        readonly YoutubeClient youtube = new YoutubeClient();

        private async Task<PathInfo> QueryLib(SongRequest SongRequest)
        {
            System.Diagnostics.Debug.WriteLine("executing query lib");

            var video = youtube.Videos;

            //get video title from metadata
            var metadata = await video.GetAsync(SongRequest.Url);
            var title = metadata.Title;

            //save filename as title and assign path for video download in project App_Data
            PathInfo pathInfo = new PathInfo();
            pathInfo.FilePath = @"C:\Users\jabri_000\source\repos\SongGetterWebAPI-AWS\SongGetterWebAPI-AWS\App_Data";
            pathInfo.FileName = @"" + title + ".mp3";

            //download video
            return await Task.Run(() => Youtube.DownloadVideoAsMp3(youtube, SongRequest.Url, pathInfo));
        }

        public async Task<HttpResponseMessage> GetSongFromLib(string Url)
        {
            System.Diagnostics.Debug.WriteLine("executing GetSongFromLib");

            //instantiate SongRequest class assigning json data sent in GET request from client
            SongRequest songRequest = new SongRequest();
            songRequest.Url = Url;
            songRequest.IsPlaylist = false;

            //create paths, download video(s) from YoutubeExplode library
            var pathInfo = await Task.Run(() => QueryLib(songRequest));

            var filePath = Path.Combine(pathInfo.FilePath, pathInfo.FileName);
            return await Task.Run(() => ResponseHelper.ConstructResponse(pathInfo, filePath, "audio/mpeg"));
        }
    }
}
