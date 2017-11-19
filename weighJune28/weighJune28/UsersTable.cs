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

        [JsonProperty(PropertyName = "height")]
        public float height { get; set; }

        [JsonProperty(PropertyName = "age")]
        public int age { get; set; }

        [JsonProperty(PropertyName = "gender")]
        public string gender { get; set; }
    }
}