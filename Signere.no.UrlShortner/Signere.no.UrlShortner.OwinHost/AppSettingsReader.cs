using System;
using System.Configuration;

namespace Signere.no.UrlShortner.OwinHost
{
    public static class AppSettingsReader
    {

        public static string StorageAccountName { get { return GetSetting("storageAccountName");} }

        public static string StorageAccountSecret { get { return GetSetting("storageAccountSecret"); } }

        public static string Baseurl
        {
            get
            {
                string url = GetSetting("baseurl");
                if (!url.EndsWith("/"))
                    url +="/";
                return url;
            }
        }

        public static bool UseCache { get { return Boolean.Parse( GetSetting("useCache")); } }

        public static bool Debug { get { return Boolean.Parse(GetSetting("debug")); } }

        private static string GetSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}