using System.Threading;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Signere.no.UrlShortner.Core;
using Signere.no.UrlShortner.OwinHost;
using Signere.no.UrlShortner.Service;

[assembly: OwinStartup(typeof(Startup))]
namespace Signere.no.UrlShortner.OwinHost
{
    
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            IUrlShortnerService service=new UrlShortnerService(AppSettingsReader.StorageAccountName,AppSettingsReader.StorageAccountSecret,AppSettingsReader.Baseurl,AppSettingsReader.UseCache);

            var token = new OwinContext(app.Properties).Get<CancellationToken>("host.OnAppDisposing");
            if (token != CancellationToken.None)
            {
                token.Register(service.Dispose);
            }


            app.UseCors(CorsOptions.AllowAll);


            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                EnableDefaultFiles = true,
                DefaultFilesOptions = { DefaultFileNames = { "index.html" } },
                FileSystem = new PhysicalFileSystem(@"./html"),
                RequestPath = new PathString("/html"),
                StaticFileOptions = { OnPrepareResponse = (ContextCallback) =>
                {
                    ContextCallback.OwinContext.Response.Headers.Add("X-Frame-Options",new[] {"DENY"});
                    ContextCallback.OwinContext.Response.Headers.Add("Content-Security-Policy",new string[]
                    {
                        "script-src 'self' https://ajax.googleapis.com https://code.jquery.com https://maxcdn.bootstrapcdn.com; style-src https://maxcdn.bootstrapcdn.com; img-src https://az280641.vo.msecnd.net; font-src https://maxcdn.bootstrapcdn.com;"
                    });
                } }
            };

            app.UseFileServer(options);

            app.UseUrlShortner(service);

        }
    }
}