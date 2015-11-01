﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
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

            switch (context.Request.Method.ToLowerInvariant())
            {
                case "get":
                    await InvokeGet(context);
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
                default:
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Not valid HTTP METHOD/VERB");
                    return;

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
                    await context.Response.WriteAsync(e.Message);
                }
                catch (Exception)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Server error");
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
                await context.Response.WriteAsync(e.Message);
            }
            catch (Exception)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
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

                int statusCode = entity.Expires.HasValue ? 302 : 301;
                if (entity.Url.ToLowerInvariant().Contains("https://"))
                {
                    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000");
                }
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
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync(e.Message);
            }
            catch (ExpiredException e)
            {
                context.Response.StatusCode = 410;
                await context.Response.WriteAsync(e.Message);
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Server error");
            }
        }
    }

    public static class UrlShortnerMiddlewareExtension
    {
        public static void UseUrlShortner(this IAppBuilder app, IUrlShortnerService urlShortnerService)
        {
            
            app.Use<UrlShortnerMiddleware>(urlShortnerService);
        }
    }
}