using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace SongGetterWebAPI_AWS.Helpers
{
    public static class MyServer
    {
        public static string MapPath(string path)
        {
            return Path.Combine(System.IO.Directory.GetCurrentDirectory(), path);
        }
    } 
}