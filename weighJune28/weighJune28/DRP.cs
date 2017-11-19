using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;

namespace weighJune28
{
    /**
     * Represents a DRP message according to the DRP protocol
     **/
    [JsonObject(MemberSerialization.OptIn)]
    class DRP
    {
        //private const string pROTOCOL = "$DRP";
        public const string PROTOCOL = "$DRP";
        private DRPDevType devType;
        private string userName;
        private long sourceID;
        private long destID;
        private IList<float> data;
        private ulong token;
        private DRPMessageType messageType;
        private DateTime date;

        //public static string PROTOCOL
        //{
        //    get
        //    {
        //        return pROTOCOL;
        //    }
        //}

        internal DRPDevType DevType
        {
            get
            {
                return devType;
            }

            set
            {
                devType = value;
            }
        }

        public string UserName
        {
            get
            {
                return userName;
            }

            set
            {
                userName = value;
            }
        }

        public long SourceID
        {
            get
            {
                return sourceID;
            }

            set
            {
                sourceID = value;
            }
        }

        public long DestID
        {
            get
            {
                return destID;
            }

            set
            {
                destID = value;
            }
        }

        public IList<float> Data
        {
            get
            {
                return data;
            }
        }

        public ulong Token
        {
            get
            {
                return token;
            }

            set
            {
                token = value;
            }
        }

        internal DRPMessageType MessageType
        {
            get
            {
                return messageType;
            }

            set
            {
                messageType = value;
            }
        }

        public DateTime Date
        {
            get
            {
                return date;
            }

            set
            {
                date = value;
            }
        }



        ///* Setters & Getters */
        //public DRPDevType DevType { get => devType; set => devType = value; }
        //public string UserName { get => userName; set => userName = value; }
        //public long DestID { get => destID; set => destID = value; }
        //public long SourceID { get => sourceID; set => sourceID = value; }
        //public ulong Token { get => token; set => token = value; }
        //public DRPMessageType MessageType { get => messageType; set => messageType = value; }
        //public DateTime Date { get => date; set => date = value; }
        //public IList<float> Data { get => data; }

        /* Setters & Getters for JSON */
        //[JsonProperty(PropertyName = "Protocol")]
        //private string JProtocol
        //{
        //    get => PROTOCOL; }

        //[JsonProperty(PropertyName = "height")]
        //public float height { get; set; }




        [JsonProperty(PropertyName = "Protocol")]
        public string JProtocol
        {
            get
            {
                return PROTOCOL;
            }
        }



        [JsonProperty(PropertyName = "DevType")]
        public string JDevType
        {
            get
            {
                return stringer(devType);
            }

            set
            {
                devType = parseDevType(value);
            }
        }




        //    [JsonProperty(PropertyName = "DevType")]
        //    private string JDevType
        //{
        //    get =>stringer(devType); set => devType = parseDevType(value); }
        //    [JsonProperty(PropertyName = "sourceID")]
        //    private string JSourceUD
        //{
        //    get => sourceID.ToString("X"); set => sourceID = long.Parse(value, NumberStyles.HexNumber); }
        [JsonProperty(PropertyName = "sourceID")]
        public string JSourceUD
        {
            get
            {
                return sourceID.ToString("X");
            }

            set
            {
                sourceID = long.Parse(value, NumberStyles.HexNumber);
            }
        }



        //    [JsonProperty(PropertyName = "destID")]
        //    private string JDestID
        //{
        //    get => destID.ToString("X"); set => destID = long.Parse(value, NumberStyles.HexNumber); }
        [JsonProperty(PropertyName = "destID")]
        public string JDestID
        {
            get
            {
                return destID.ToString("X");
            }

            set
            {
                destID = long.Parse(value, NumberStyles.HexNumber);
            }
        }






        //    [JsonProperty(PropertyName = "Username")]
        //    private string JUserName
        //{
        //    get => userName; set => userName = value; }
        [JsonProperty(PropertyName = "Username")]
        public string JUserName
        {
            get
            {
                return userName;
            }

            set
            {
                userName = value;
            }
        }




        //    [JsonProperty(PropertyName = "Data")]
        //    private string JData
        //{
        //    get => JsonConvert.SerializeObject(data); set => data = JsonConvert.DeserializeObject<IList<float>>(value); }

        [JsonProperty(PropertyName = "Data")]
        public string JData
        {
            get
            {
                return JsonConvert.SerializeObject(data);
            }

            set
            {
                data = JsonConvert.DeserializeObject<IList<float>>(value);
            }
        }



        //    [JsonProperty(PropertyName = "Token")]
        //    private string JToken
        //{
        //    get => token.ToString("X"); set => token = ulong.Parse(value, NumberStyles.HexNumber); }
        [JsonProperty(PropertyName = "Token")]
        public string JToken
        {
            get
            {
                return token.ToString("X");
            }

            set
            {
                token = ulong.Parse(value, NumberStyles.HexNumber);
            }
        }




        //    [JsonProperty(PropertyName = "MsgType")]
        //    private string JMessageType
        //{
        //    get => ((int)messageType).ToString(); set => messageType = (DRPMessageType)int.Parse(value); } //TODO Change to number.
        [JsonProperty(PropertyName = "MsgType")]
        public string JMessageType
        {
            get
            {
                return ((int)messageType).ToString(); ;
            }

            set
            {
                messageType = (DRPMessageType)int.Parse(value);    //TODO Change to number
            }
        }



        //    [JsonProperty(PropertyName = "Date")]
        //    private string JDate
        //{
        //    get => date.Ticks.ToString(); set => date = DateTime.MinValue + TimeSpan.FromTicks(long.Parse(value)); }
        [JsonProperty(PropertyName = "Date")]
        public string JDate
        {
            get
            {
                return date.Ticks.ToString(); ;
            }

            set
            {
                date = DateTime.MinValue + TimeSpan.FromTicks(long.Parse(value));
            }
        }


        /** constructors **/

        //Empty constructor is needed for deserialize JSON
        private DRP() { }

        public DRP(DRPDevType devType, string userName, long sourceID, long destID, IList<float> data, ulong token, DRPMessageType messageType, DateTime date)
        {
            this.devType = devType;
            this.userName = userName;
            this.sourceID = sourceID;
            this.destID = destID;
            this.data = data;
            this.token = token;
            this.messageType = messageType;
            this.date = date;
        }
        public DRP(DRPDevType devType, string userName, long sourceID, long destID, IList<float> data, ulong token, DRPMessageType messageType)
        {
            this.devType = devType;
            this.userName = userName;
            this.sourceID = sourceID;
            this.destID = destID;
            this.data = data;
            this.token = token;
            this.messageType = messageType;
            this.date = DateTime.Now;
        }

        /**
         * Deserializing DRP message
         * @param mepMessage the string to deserialize
         **/

        public static DRP deserializeDRP(string drpMessage)
        {
            drpMessage = drpMessage.Replace('?', '\"');
            DRP drp = JsonConvert.DeserializeObject<DRP>(drpMessage);
            return drp;
        }


        /**
         * convert enum type to string 
         */
        private static string stringer(DRPDevType type)
        {
            switch (type)
            {
                case DRPDevType.APP:
                    return "APP";
                case DRPDevType.RBPI:
                    return "RBPI";
                default:
                    return "";
            }
        }

        /**
         * convert string to enum type
         */
        private static DRPDevType parseDevType(string type)
        {
            switch (type)
            {
                case "APP":
                    return DRPDevType.APP;
                case "RBPI":
                    return DRPDevType.RBPI;
                default:
                    return DRPDevType.RBPI;
            }
        }



        /**
        * Sign the current date and time onto the message
        **/
        public void timeStamp()
        {
            this.Date = DateTime.Now;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is DRP))
                return false;
            DRP compto = obj as DRP;
            if (this.devType != compto.devType)
                return false;
            if (this.sourceID != compto.sourceID)
                return false;
            if (this.destID != compto.destID)
                return false;
            if (!JsonConvert.SerializeObject(data).Equals(JsonConvert.SerializeObject(compto.data)))
                return false;
            if (this.messageType != compto.messageType)
                return false;
            return true;
        }
    }
    enum DRPDevType { RBPI, APP }
    enum DRPMessageType { SCANNED, DATA, ACK, IN_USE, HARDWARE_ERROR, ILLEGAL }
}
