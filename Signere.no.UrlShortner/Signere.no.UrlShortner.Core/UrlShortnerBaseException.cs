using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Signere.no.UrlShortner.Core
{
    public abstract class UrlShortnerBaseException:ApplicationException
    {
        public UrlShortnerBaseException(string id):base(id)
        {
            
        }
    }

    public class NotFoundException : UrlShortnerBaseException
    {
        public NotFoundException(string id) : base(string.Format("Url with ID: {0} cannot be found", id))
        {
        }
    }

    public class ExpiredException : UrlShortnerBaseException
    {
        public ExpiredException(string id) : base(string.Format("Url with ID: {0} is expired", id))
        {
        }
    }

    public class UnAuthorizedException : UrlShortnerBaseException
    {
        public UnAuthorizedException(string id) : base(string.Format("Not authorized to update url with id: {0}", id))
        {
        }
    }
}
