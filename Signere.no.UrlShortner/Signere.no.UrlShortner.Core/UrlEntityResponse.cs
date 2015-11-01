using System;

namespace Signere.no.UrlShortner.Core
{
    public class UrlEntityResponse
    {
        public string ShortUrl { get; set; }

        public string AccessToken { get; set; }

        public string Id { get; set; }
    }

    public class UrlEntity
    {


        public string Url { get; set; }
        public bool AllowIframe { get; set; }

        public DateTime? Expires { get; set; }

    
        public string Id { get; set; }


        public string AccessToken { get; set; }
    }
}