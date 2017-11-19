/*
 * NEW CHANGES!!
 * 04.10.17
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;

namespace RPiRunner2
{
    /**
     * Represents a DRP message according to the DRP protocol
     **/
    [JsonObject(MemberSerialization.OptIn)]
    class DRP
    {
        public const string PROTOCOL = "$DRP";
        private DRPDevType devType;
        private string userName;
        private string servID;
        private string servName;
        private float data;
        private ulong token;
        private DRPMessageType messageType;
        private DateTime date;

        /* Setters & Getters */
        public DRPDevType DevType { get => devType; set => devType = value; }
        public string UserName { get => userName; set => userName = value; }
        public string ServID { get => servID; set => servID = value; }
        public string ServName { get => servName; set => servName = value; }
        public ulong Token { get => token; set => token = value; }
        public DRPMessageType MessageType { get => messageType; set => messageType = value; }
        public DateTime Date { get => date; set => date = value; }
        public float Data { get => data; set => data = value; }

        /* Setters & Getters for JSON */
        [JsonProperty(PropertyName = "Protocol")]
        private string JProtocol { get => PROTOCOL; }
        [JsonProperty(PropertyName = "DevType")]
        private string JDevType { get => stringer(devType); set => devType = parseDevType(value); }
        [JsonProperty(PropertyName = "ServID")]
        private string JSourceUD { get => ServID; set => ServID = value; }
        [JsonProperty(PropertyName = "servName")]
        private string JDestID { get => servName; set => servName = value; }
        [JsonProperty(PropertyName = "Username")]
        private string JUserName { get => userName; set => userName = value; }
        [JsonProperty(PropertyName = "Data")]
        private string JData { get => data.ToString(); set => data = float.Parse(value); }
        [JsonProperty(PropertyName = "Token")]
        private string JToken { get => token.ToString("X"); set => token = ulong.Parse(value, NumberStyles.HexNumber); }
        [JsonProperty(PropertyName = "MsgType")]
        private string JMessageType { get => ((int)messageType).ToString(); set => messageType = (DRPMessageType)int.Parse(value); } //TODO Change to number.
        [JsonProperty(PropertyName = "Date")]
        private string JDate { get => date.Ticks.ToString(); set => date = DateTime.MinValue + TimeSpan.FromTicks(long.Parse(value)); }



        /** constructors **/

        //Empty constructor is needed for deserialize JSON
        private DRP() { }

        public DRP(DRPDevType devType, string userName, string servID, string servName, float data, ulong token, DRPMessageType messageType, DateTime date)
        {
            this.devType = devType;
            this.userName = userName;
            this.servID = servID;
            this.servName = servName;
            this.data = data;
            this.token = token;
            this.messageType = messageType;
            this.date = date;
        }
        public DRP(DRPDevType devType, string userName, string servID, string servName, float data, ulong token, DRPMessageType messageType)
        {
            this.devType = devType;
            this.userName = userName;
            this.servID = servID;
            this.servName = servName;
            this.data = data;
            this.token = token;
            this.messageType = messageType;
            this.date = DateTime.Now;
        }

        /**
         * Deserializing DRP message
         * @param mepMessage the string to deserialize
         * EXAMPLE:
         * {"$DRP":{"DevType":"RBPI","MACAddr":"123456789ABC","IPAddr":"C0A80101","Callback":"RES","Addressee":"123456789ABC","Date": "28/07/2017 19:02"}}
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
            if (this.servID != compto.servID)
                return false;
            if (this.servName != compto.ServName)
                return false;
            if (!JsonConvert.SerializeObject(data).Equals(JsonConvert.SerializeObject(compto.data)))
                return false;
            if (this.messageType != compto.messageType)
                return false;
            return true;
        }
    }
    enum DRPDevType { RBPI, APP }
    enum DRPMessageType { SCANNED, DATA, ACK, IN_USE, HARDWARE_ERROR, ILLEGAL  }
}
