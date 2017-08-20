using System;
using Newtonsoft.Json;

namespace weighJune28
{
    class RaspberryTable
    {
        public string Id { get; set; }

        public DateTime createdAt { get; set; }

        [JsonProperty(PropertyName = "IPNumber")]
        public long IPNumber { get; set; }


        [JsonProperty(PropertyName = "MacAddress")]
        public long MacAddress { get; set; }

    }
}