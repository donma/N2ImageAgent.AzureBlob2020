using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace N2ImageAgent.AzureBlob.Models
{
    public class CacheInfo
    {
        public DateTime UTCExpire { get; set; }
        public string Url {get;set;}
}
}
