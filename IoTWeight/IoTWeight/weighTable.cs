using System;
using Newtonsoft.Json;

namespace IoTWeight
{
    public class weighTable
    {
        public string Id { get; set; }

        public DateTime createdAt { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string username { get; set; }

        [JsonProperty(PropertyName = "weigh")]
        public float weigh { get; set; }

       
        //public DateTime DateUtc { get; set; }

        //[Newtonsoft.Json.JsonIgnore]
        //public string DateDisplay { get { return DateUtc.ToLocalTime().ToString("d"); } }

        //[Newtonsoft.Json.JsonIgnore]
        //public string TimeDisplay { get { return DateUtc.ToLocalTime().ToString("t"); } }
    }
}