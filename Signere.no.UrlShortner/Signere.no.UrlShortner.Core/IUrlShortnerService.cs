using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signere.no.UrlShortner.Core
{
    public interface IUrlShortnerService:IDisposable
    {
        Task<UrlEntityResponse> Create(string url, DateTime? Expires =null, bool BlockiFrame = false);

        Task<UrlEntityResponse> Create(UrlEntityRequest request);

        Task Update(string id, string AccessToken, DateTime? Expires = null, bool BlockiFrame = false);

        Task Update(string id, string AccessToken,string url, DateTime? Expires = null, bool BlockiFrame = false);

        Task Update(string id, string AccessToken, UrlEntityRequest request);

        Task Delete(string id, string AccessToken);

        Task<string> Get(string id);

        Task<UrlEntity> GetEntity(string id);

    }
}
