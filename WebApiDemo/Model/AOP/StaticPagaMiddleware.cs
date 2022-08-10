using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiDemo.Model.AOP
{
    public class StaticPagaMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string directoryPath;
        private readonly bool deleted;

        public StaticPagaMiddleware(RequestDelegate next, string directoryUrl, bool deleted)
        {
            this._next = next;
            this.directoryPath = directoryUrl;
            this.deleted = deleted;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value!.StartsWith("/weatherforecast"))
            {
                Console.WriteLine($"path:{context.Request.Path.Value}");

                #region  context.Response.Body

                var originalStream = context.Response.Body;
                using (var copyStream = new MemoryStream()) 
                {
                    context.Response.Body = copyStream;
                    await _next(context);

                    copyStream.Position = 0;
                    var reader = new StreamReader(copyStream);
                    var content = await reader.ReadToEndAsync();
                    string url = context.Request.Path.Value;

                    this.SaveHtml(url, content);
                    copyStream.Position = 0;
                    await copyStream.CopyToAsync(originalStream);
                    context.Response.Body = originalStream;

                }

                #endregion
            }
            else {
                await _next(context);
            }
        }

        private void SaveHtml(string url, string html) 
        {
            try
            {
                if (string.IsNullOrEmpty(html)) 
                {
                    return;
                }

                if (!url.EndsWith("weatherforecast")) 
                {
                    return;
                }

                //string directoryPath = "D:\\WangCong\\data";
                if (Directory.Exists(directoryPath) == false) 
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fullPath = Path.Combine(directoryPath, url.Split("/").Last());
                File.WriteAllText(fullPath, html);
            }
            catch (Exception e) 
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}
