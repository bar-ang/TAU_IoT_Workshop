using System;
using Newtonsoft.Json;

namespace weighJune28
{
    public class UsersTable
    {
        public string Id { get; set; }

        public DateTime createdAt { get; set; }

        [JsonProperty(PropertyName = "UniqueUsername")]
        public string UniqueUsername { get; set; }

        [JsonProperty(PropertyName = "MACaddress")]
        public long MACaddress { get; set; }
    }
}