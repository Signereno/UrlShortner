using System;

namespace Signere.no.UrlShortner.Core
{
    public static class Extension
    {

        public static bool Expired(this UrlEntity entity)
        {
            return entity.Expires.HasValue && entity.Expires.Value < DateTime.UtcNow;
        }

        
        
    }
}