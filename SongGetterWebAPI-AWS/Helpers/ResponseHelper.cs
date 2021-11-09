using SongGetterWebAPI_AWS.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SongGetterWebAPI_AWS.Helpers
{
    public class ResponseHelper
    {
        public static HttpResponseMessage ConstructResponse(PathInfo pathInfo, string filePath, string ContentType)
        {

            //if video could not be downloaded (incorrect url?), return error response message
            if (pathInfo.IsError)
            {
                HttpResponseMessage errorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
                return errorResponse;
            }

            string fullPath = Path.Combine(pathInfo.FilePath, pathInfo.FileName);

            //build response to send to client
            var result = new HttpResponseMessage(HttpStatusCode.OK);

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var memoryStream = new MemoryStream(fileBytes);
            result.Content = new StreamContent(memoryStream);

            var headers = result.Content.Headers;
            headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            headers.ContentDisposition.FileName = pathInfo.FileName;
            headers.ContentType = new MediaTypeHeaderValue(ContentType);
            headers.ContentLength = memoryStream.Length;
            return result;
        }
    }
}