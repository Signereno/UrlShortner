using Owin;
using Signere.no.UrlShortner.Core;

namespace Signere.no.UrlShortner.OwinHost
{
    public static class UrlShortnerMiddlewareExtension
    {
        public static void UseUrlShortner(this IAppBuilder app, IUrlShortnerService urlShortnerService)
        {
            
            app.Use<UrlShortnerMiddleware>(urlShortnerService);
        }
    }
}