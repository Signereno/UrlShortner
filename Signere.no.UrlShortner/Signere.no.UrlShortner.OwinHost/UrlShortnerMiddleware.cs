using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Signere.no.UrlShortner.Core;

namespace Signere.no.UrlShortner.OwinHost
{
    public class UrlShortnerMiddleware : OwinMiddleware
    {
        private readonly IUrlShortnerService _service;

        public UrlShortnerMiddleware(OwinMiddleware next,IUrlShortnerService service) : base(next)
        {
            _service = service;
        }

        public async override Task Invoke(IOwinContext context)
        {
            SetupSecurityHeaderes(context);

            switch (context.Request.Method.ToLowerInvariant())
            {
                case "get":
                    if (context.Request.Path.ToString().ToLowerInvariant().Contains("robots.txt"))
                    {
                        context.Response.StatusCode = 200;
                        await context.Response.WriteAsync(
                            new StringBuilder()
                            .AppendLine("User-Agent: *")
                            .AppendLine("Disallow: /").ToString()
                        );
                    }
                    else
                    {
                        await InvokeGet(context);
                    }
                    
                    return;
                case "post":
                    await InvokePost(context);
                    return;
                case "delete":
                    await InvokeDelete(context);
                    return;
                case "put":
                    await InvokePut(context);
                    return;
                case "options":
                    context.Response.StatusCode = 200;
                    return;
                default:
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Not valid HTTP METHOD/VERB");
                    return;

            }
        }

        private static void SetupSecurityHeaderes(IOwinContext context)
        {
            if (!string.IsNullOrWhiteSpace(AppSettingsReader.PublicKeyPinning))
            {
                context.Response.Headers.Add("Public-Key-Pins", new[] { AppSettingsReader.PublicKeyPinning }); ;
            }

            if (!string.IsNullOrWhiteSpace(AppSettingsReader.HstsHeader))
            {
                context.Response.Headers.Add("Strict-Transport-Security", new[] { AppSettingsReader.HstsHeader });
            }
        }

        private async Task InvokePut(IOwinContext context)
        {
            var id = context.Request.Path.Value.Substring(1);
            var accesstoken = context.Request.Query["accesstoken"];

            if (string.IsNullOrWhiteSpace(id))
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Missing ID");
            }

            if (string.IsNullOrWhiteSpace(accesstoken))
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Missing accesstoken");
            }

            using (var reader = new JsonTextReader(new StreamReader(context.Request.Body, Encoding.UTF8)))
            {
                UrlEntityRequest request = new Newtonsoft.Json.JsonSerializer().Deserialize<UrlEntityRequest>(reader);

                try
                {
                    await _service.Update(id,accesstoken,request);

                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(string.Format("Url with ID: {0} is updated", id)); ;
                }
                catch (UnAuthorizedException e)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync(e.Message);
                }
                catch (NotFoundException e)
                {
                    context.Response.StatusCode = 404;
                    context.Response.ReasonPhrase = e.Message;
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync(File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/html/Notfound.html")));
                }
                catch (Exception e)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Server error" + (AppSettingsReader.Debug ? e.Message + Environment.NewLine + e.InnerException : ""));
                }
            }
        }

        private async Task InvokeDelete(IOwinContext context)
        {
            var id = context.Request.Path.Value.Substring(1);
            var accesstoken = context.Request.Query["accesstoken"];

            if (string.IsNullOrWhiteSpace(id))
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Missing ID");
            }

            if (string.IsNullOrWhiteSpace(accesstoken))
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Missing accesstoken");
            }

            try
            {
                await _service.Delete(id, accesstoken);

                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(string.Format( "Url with ID: {0} is deleted",id));
            }
            catch (UnAuthorizedException e)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync(e.Message);
            }
            catch (NotFoundException e)
            {
                context.Response.StatusCode = 404;
                context.Response.ReasonPhrase = e.Message;
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/html/Notfound.html")));
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error" + (AppSettingsReader.Debug ? e.Message + Environment.NewLine + e.InnerException : ""));
            }
        }

        private async Task InvokePost(IOwinContext context)
        {

            using (var reader = new JsonTextReader(new StreamReader(context.Request.Body, Encoding.UTF8)))
            {
                UrlEntityRequest request = new Newtonsoft.Json.JsonSerializer().Deserialize<UrlEntityRequest>(reader);


                var response = await _service.Create(request);

                context.Response.StatusCode = 201;
                context.Response.ContentType = "application/json";
                
             
                await context.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(response));
            }

        }

        private async Task InvokeGet(IOwinContext context)
        {
            var id = context.Request.Path.Value.Substring(1);

            string logPath = "entityupdatelogandurl/";
            if (!string.IsNullOrWhiteSpace(id) && id.StartsWith(logPath) && id.Substring(logPath.Length).Contains('/'))
            {
                var path = id.Substring(logPath.Length);

                var splitString = path.Split('/');

                if (splitString.Count() != 2)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Not valid url pattern");
                    return;
                }

                id = path.Split('/').First();
                string token = path.Split('/').Last();

                var entity = await _service.GetEntityUpdateLog(id, token);

                List<string> urls = !string.IsNullOrWhiteSpace(entity.UpdateLog)
                    ? entity.UpdateLog.Split(',').ToList()
                    : new List<string>();
                urls.Add(entity.Url);

                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(urls));
                return;
            }
            
            string prefix = null;
            if (id.Contains("/"))
            {
                var splitString = id.Split('/');

                if (splitString.Count() != 2)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Not valid url pattern");
                    return;
                }

                id = splitString.Last();
                prefix = splitString.First();
            }

            if (string.IsNullOrWhiteSpace(id) || id.Length > 20)
            {
                context.Response.StatusCode =302;
                context.Response.Headers.Set("Location", string.Format("{0}html/index.html", AppSettingsReader.Baseurl));
                return;
                //context.Response.StatusCode = 404;
                //await context.Response.WriteAsync("URL id is missing or not matching this services requirements!");
            }

            try
            {
                var entity = await _service.GetEntity(id);

                if (!string.IsNullOrWhiteSpace(prefix) && !prefix.Equals(entity.Prefix))
                {
                    throw new NotFoundException(id);
                }

                int statusCode =entity.PermanentRedirect ? 301:  302;
                //if (entity.Url.ToLowerInvariant().Contains("https://"))
                //{
                    //context.Response.Headers.Append("Strict-Transport-Security", AppSettingsReader.Baseurl.Contains("signere.no") ?
                    //    "max-age=31536000; includeSubdomains; preload":
                    //    "max-age=31536000");
                //}
                if (entity.BlockiFrame)
                {
                    context.Response.Headers.Append("X-Frame-Options","DENY");
                    context.Response.Headers.Append("Content-Security-Policy","frame-ancestors 'none'");
                }

                context.Response.StatusCode = statusCode;
                context.Response.Headers.Set("Location", entity.Url);

            }
            catch (NotFoundException e)
            {
                context.Response.StatusCode = 404;
                context.Response.ReasonPhrase = e.Message;
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/html/Notfound.html")));
            }
            catch (ExpiredException e)
            {
                context.Response.StatusCode = 410;
                await context.Response.WriteAsync(e.Message);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error" + (AppSettingsReader.Debug ? e.Message + Environment.NewLine + e.InnerException :""));
            }
        }
    }
}