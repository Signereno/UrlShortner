using System;
using System.Threading.Tasks;

namespace Signere.no.UrlShortner.Core
{
    public interface IUrlShortnerService:IDisposable
    {
        Task<UrlEntityResponse> Create(string url, DateTime? Expires =null, bool BlockiFrame = false,string accessToken=null);

        Task<UrlEntityResponse> Create(UrlEntityRequest request);

        Task Update(string id, string AccessToken, DateTime? Expires = null, bool BlockiFrame = false);

        Task Update(string id, string AccessToken,string url, DateTime? Expires = null, bool BlockiFrame = false);

        Task Update(string id, string AccessToken, UrlEntityRequest request);

        Task Delete(string id, string AccessToken);

        Task<string> Get(string id);

        Task<UrlEntity> GetEntity(string id);

    }

    public interface IUrlShortnerClient : IDisposable
    {
        Task<UrlEntityResponse> Create(string url, DateTime? Expires = null, bool BlockiFrame = false,string accessToken=null);
        Task Update(string id, string accesstoken, string url = null, DateTime? Expires = null, bool BlockiFrame = false);
        Task Delete(string id, string accesstoken);
        void Dispose();
    }
}
