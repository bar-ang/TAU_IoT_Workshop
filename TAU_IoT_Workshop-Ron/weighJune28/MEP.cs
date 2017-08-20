using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;

namespace ManualDataSend2
{
    /**
     * Represent a MEP message according to the MEP protocol
     **/
    [JsonObject(MemberSerialization.OptIn)]
    class MEP
    {
        private MEPDevType devType;
        private uint ipAddr;
        private long macAddr;
        private long addressee;
        private MEPCallbackAction callbackAction;
        private DateTime date;

        /* Setters & Getters */
        public MEPDevType DevType { get => devType; set => devType = value; }
        public long MacAddr { get => macAddr; set => macAddr = value; }
        public uint IpAddr { get => ipAddr; set => ipAddr = value; }
        public long Addressee { get => addressee; set => addressee = value; }
        public MEPCallbackAction CallbackAction { get => callbackAction; set => callbackAction = value; }
        public DateTime Date { get => date; set => date = value; }

        /* Setters & Getters for JSON */
        [JsonProperty(PropertyName = "Protocol")]
        private string JProtocol { get => "$MEP"; }
        [JsonProperty(PropertyName = "DevType")]
        private string JDevType { get => stringer(devType); set => devType = parseDevType(value); }
        [JsonProperty(PropertyName = "MacAddr")]
        private string JMacAddr { get => macAddr.ToString("X"); set => macAddr = long.Parse(value, NumberStyles.HexNumber); }
        [JsonProperty(PropertyName = "IpAddr")]
        private string JIpAddr { get => ipAddr.ToString("X"); set => ipAddr = uint.Parse(value, NumberStyles.HexNumber); }
        [JsonProperty(PropertyName = "Addressee")]
        private string JAddressee { get => addressee.ToString("X"); set => addressee = uint.Parse(value, NumberStyles.HexNumber); }
        [JsonProperty(PropertyName = "CallbackAction")]
        private string JCallbackAction { get => stringer(callbackAction); set => callbackAction = parseCallbackAction(value); }
        [JsonProperty(PropertyName = "Date")]
        private string JDate { get => date.ToString(); }

        /** constructors **/

        //Empty constructor is needed for deserialize JSON
        private MEP() { }

        public MEP(MEPDevType devType, long macAddr, uint ipAddr, long addressee, MEPCallbackAction callbackAction)
        {
            this.devType = devType;
            this.IpAddr = ipAddr;
            this.MacAddr = macAddr;
            this.addressee = addressee;
            this.CallbackAction = callbackAction;
            this.Date = DateTime.Now;
        }
        private MEP(MEPDevType devType, long macAddr, uint ipAddr, long addressee, MEPCallbackAction callbackAction, DateTime date)
        {
            this.devType = devType;
            this.IpAddr = ipAddr;
            this.MacAddr = macAddr;
            this.addressee = addressee;
            this.CallbackAction = callbackAction;
            this.Date = date;
        }
        public MEP(MEPDevType devType, MEPCallbackAction callbackAction)
        {
            this.devType = devType;
            this.IpAddr = 0;
            this.MacAddr = 0;
            this.addressee = 0;
            this.CallbackAction = callbackAction;
            this.Date = DateTime.Now;
        }


        /**
         * Deserializing MEP message
         * @param mepMessage the string to deserialize
         * EXAMPLE:
         * {"$MEP":{"DevType":"RBPI","MACAddr":"123456789ABC","IPAddr":"C0A80101","Callback":"RES","Addressee":"123456789ABC","Date": "28/07/2017 19:02"}}
         **/
        public static MEP deserializeMEP(string mepMessage)
        {
            mepMessage = mepMessage.Replace('?', '\"');
            JsonSerializerSettings jss = new JsonSerializerSettings();
            MEP mep = JsonConvert.DeserializeObject<MEP>(mepMessage, jss);
            return mep;
        }

        /**
         * convert enum type to string 
         */
        private static string stringer(MEPDevType type)
        {
            switch (type)
            {
                case MEPDevType.APP:
                    return "APP";
                case MEPDevType.RBPI:
                    return "RBPI";
                default:
                    return "";
            }
        }
        /**
         * convert enum type to string 
         */
        private static string stringer(MEPCallbackAction type)
        {
            switch (type)
            {
                case MEPCallbackAction.Response:
                    return "RES";
                case MEPCallbackAction.Acknowledge:
                    return "ACK";
                case MEPCallbackAction.NoOperation:
                    return "NOP";
                default:
                    return "";
            }
        }
        /**
         * convert string to enum type
         */
        private static MEPDevType parseDevType(string type)
        {
            switch (type)
            {
                case "APP":
                    return MEPDevType.APP;
                case "RBPI":
                    return MEPDevType.RBPI;
                default:
                    return MEPDevType.RBPI;
            }
        }
        /**
         * convert string to enum type
         */
        private static MEPCallbackAction parseCallbackAction(string type)
        {
            switch (type)
            {
                case "RES":
                    return MEPCallbackAction.Response;
                case "ACK":
                    return MEPCallbackAction.Acknowledge;
                case "NOP":
                    return MEPCallbackAction.NoOperation;
                default:
                    return MEPCallbackAction.NoOperation;
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
    }
    enum MEPDevType { RBPI, APP }
    enum MEPCallbackAction { Response, Acknowledge, NoOperation }
}
