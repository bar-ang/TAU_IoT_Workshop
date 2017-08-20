using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.WindowsAzure.MobileServices;
using weighJune28;

namespace ManualDataSend2
{
    class MEPClient
    {

        private uint myIp;
        private long myMac;
        private long raspberryMAC;

        //list of nearby Raspberry IPs
        private List<long> nearbyRasps;

        public MEPClient(uint myIp, long myMac, List<long> nearbyRasps)
        {
            this.myIp = myIp;
            this.myMac = myMac;
            this.nearbyRasps = new List<long>(nearbyRasps);
        }

        public uint MyIp { get => myIp; set => myIp = value; }
        public long MyMac { get => myMac; set => myMac = value; }
        public long RaspberryMAC { get => raspberryMAC; set => raspberryMAC = value; }
        public List<long> NearbyRasps { get => new List<long>(nearbyRasps); }


        //sends a MEP to all nearby Raspberries 
        private void sendMepsToNearbyRasps()
        {
            foreach (long rasp in nearbyRasps)
            {
                MEP MEPmessage = new MEP(MEPDevType.APP, MyMac, MyIp, rasp, MEPCallbackAction.Response);
                sendMEP(MEPmessage);
            }

        }

        private const int TIMEOUT = 5000;

        public void run()
        {
            sendMepsToNearbyRasps();

            bool waitingForMessages = true;
            while (waitingForMessages)
            {
                Task<MEP> mep_task = receiveMEP();
                if (!mep_task.Wait(TIMEOUT))
                {
                    Debug.WriteLine("No more messaged received");
                    waitingForMessages = false;
                    break;
                }
                MEP msg = mep_task.Result;
                raspberryMAC = msg.MacAddr;

                if(msg.CallbackAction == MEPCallbackAction.Acknowledge)
                {
                    MEP response = new MEP(MEPDevType.APP, myMac, myIp, raspberryMAC, MEPCallbackAction.NoOperation);
                    sendMEP(response);
                }

            }
        }



        /**
         * Receive MEP messages from the cloud
         */
        private async Task<MEP> receiveMEP()
        {
            string res = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
            return MEP.deserializeMEP(res);
        }


        /**
         * Send MEP Message to the cloud
         */
        private async void sendMEP(MEP message)
        {
            message.timeStamp();
            await AzureIoTHub.SendDeviceToCloudMessageAsync(message.ToString());
        }
    }

}