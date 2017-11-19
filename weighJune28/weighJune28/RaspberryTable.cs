using System;
using Newtonsoft.Json;

namespace weighJune28
{
    public class RaspberryTable
    {
        public string Id { get; set; }

        public DateTime createdAt { get; set; }

        [JsonProperty(PropertyName = "QRCode")]
        public string QRCode { get; set; }

        [JsonProperty(PropertyName = "IPNumber")]
        public uint IPNumber { get; set; }

        [JsonProperty(PropertyName = "IPAddress")]
        public string IPAddress { get; set; }
    }
}