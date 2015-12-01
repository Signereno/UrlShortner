using System;

namespace Signere.no.UrlShortner.Core
{
    public class UrlEntityResponse
    {
        public string ShortUrl { get; set; }

        public string AccessToken { get; set; }

        public string Id { get; set; }
    }

    public class UrlEntity: UrlEntityRequest
    {

    
        public string Id { get; set; }


        public string AccessToken { get; set; }
    }

    public class UrlEntityRequest
    {
        public string AccessToken { get; set; }

        public string Prefix { get; set; }


        public string Url { get; set; }
        public bool BlockiFrame { get; set; }

        public bool PermanentRedirect { get; set; }

        public DateTime? Expires { get; set; }
        


    }
}