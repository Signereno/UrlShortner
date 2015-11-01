using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;

namespace Signere.no.UrlShortner.Service
{
 

    public class UrlEntityInternal:TableEntity
    {


        public string Url { get; set; }
        public bool BlockiFrame  { get; set; }

        public DateTime? Expires { get; set; }

        [IgnoreProperty]
        public string Id
        {
            get { return string.Format("{0}{1}", this.PartitionKey, this.RowKey); }
        }

        public string AccessToken { get; set; }
    }
}
