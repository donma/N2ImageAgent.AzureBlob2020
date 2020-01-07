using System;

namespace N2ImageAgent.AzureBlob.Models
{
    public class CacheInfo
    {
        public DateTime UTCExpire { get; set; }
        public string Url { get; set; }
    }
}
