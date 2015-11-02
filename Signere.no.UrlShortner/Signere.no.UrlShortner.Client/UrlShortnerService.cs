using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Signere.no.UrlShortner.Core;

namespace Signere.no.UrlShortner.Client
{
    public class UrlShortnerService:IDisposable
    {
        private readonly string _serviceUrl;
        private HttpClient httpClient;

        public UrlShortnerService(string serviceUrl= "https://urlshortner.azurewebsites.net/")
        {
            _serviceUrl = serviceUrl;
            httpClient=new HttpClient();
            httpClient.BaseAddress =new Uri(serviceUrl);
        }

        public async Task<UrlEntityResponse> Create(string url, DateTime? Expires=null, bool BlockiFrame = false)
        {
            var jsonObj = new Signere.no.UrlShortner.Client.SimpleJSON.JSONClass();
            jsonObj.Add("Url",url);

            if(Expires.HasValue)
                jsonObj.Add("Expires", Expires.Value.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
            if(BlockiFrame)
                jsonObj.Add("BlockiFrame", BlockiFrame.ToString().ToLowerInvariant());            

            var response=await httpClient.PostAsync("",new StringContent(jsonObj.ToJSON(0), Encoding.UTF8,"application/json"));

            if(!response.IsSuccessStatusCode)
                throw new ApplicationException("Error creating short url: "+ response.StatusCode + " "+ await response.Content.ReadAsStringAsync());

            var responseJson = SimpleJSON.JSON.Parse(await response.Content.ReadAsStringAsync());

            return new UrlEntityResponse() {AccessToken = responseJson["AccessToken"],Id= responseJson["Id"],ShortUrl = responseJson["ShortUrl"]};

        }

        public async Task Update(string id,string accesstoken,string url=null, DateTime? Expires=null, bool BlockiFrame = false)
        {
            var jsonObj = new Signere.no.UrlShortner.Client.SimpleJSON.JSONClass();
            if(!string.IsNullOrWhiteSpace(url))
                jsonObj.Add("Url", url);
            if (Expires.HasValue)
                jsonObj.Add("Expires", Expires.Value.ToString("s", System.Globalization.CultureInfo.InvariantCulture));
            if (BlockiFrame)
                jsonObj.Add("BlockiFrame", BlockiFrame.ToString().ToLowerInvariant());


            var response = await httpClient.PutAsync(string.Format("{0}?accesstoken={1}", id, accesstoken), new StringContent(jsonObj.ToJSON(0), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
        }

        public async Task Delete(string id, string accesstoken)
        {
            var response = await httpClient.DeleteAsync(string.Format("{0}?accesstoken={1}",id,accesstoken));
            if(!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());
        }

        public void Dispose()
        {
            if(httpClient!=null)
                httpClient.Dispose();
        }
    }
}
