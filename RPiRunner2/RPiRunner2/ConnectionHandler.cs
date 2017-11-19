using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Networking.Connectivity;
using System.Text.RegularExpressions;
using System.Net;
namespace RPiRunner2
{
    class ConnectionHandler
    {
        public const int SOCKET_PORT = 9888;
        public const bool easyDebug = false;

        private TCPListener tcp;

        const string applicationURL = @"https://iotweight.azurewebsites.net";

        private UserHardwareLinker uhl;

        public ConnectionHandler(UserHardwareLinker uhl)
        {
            //Initializing socket listener
            tcp = new TCPListener(SOCKET_PORT, easyDebug);
            tcp.OnDataReceived += socket_onDataReceived;
            tcp.OnError += socket_onError;
            Task SocketListenTask = tcp.ListenAsync();
            this.uhl = uhl;
            SocketListenTask.Wait();
            System.Diagnostics.Debug.WriteLine("socket created");
        }



        /* socket functions */
        public async Task socket_onDataReceived(string message, Windows.Storage.Streams.DataWriter writer)
        {
            System.Diagnostics.Debug.WriteLine("received: " + message);
            if (message.Equals("@welcome@"))
            {
                //if the message is from the ScaleHub we don't use DRP, just send back the scale's name.
                await tcp.Send("TAUIOT@devname=" + PermanentData.Devname, writer);
                return;
            }

            //getting and parsing the messsage
            DRP msg;
            try
            {
                msg = DRP.deserializeDRP(message);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("non-DRP message received.");
                return;
            }

            /* taking care of illegal messages */
            if (msg.DevType == DRPDevType.RBPI)
            {
                DRP response = new DRP(DRPDevType.RBPI, "", PermanentData.Serial, PermanentData.Devname, 0, 0, DRPMessageType.ILLEGAL);
                await tcp.Send(response.ToString(), writer);
                return;
            }


            if (msg.MessageType == DRPMessageType.DATA || msg.MessageType == DRPMessageType.HARDWARE_ERROR || msg.MessageType == DRPMessageType.IN_USE)
            {
                DRP response = new DRP(DRPDevType.RBPI, "", PermanentData.Serial, PermanentData.Devname, 0, 0, DRPMessageType.ILLEGAL);
                await tcp.Send(response.ToString(), writer);
                return;
            }

            /* taking care of SCANNED messages */
            if (msg.MessageType == DRPMessageType.SCANNED)
            {
                if (uhl == null)
                {
                    //if there is no uhl, return HARDWARE_ERROR
                    DRP response = new DRP(DRPDevType.RBPI, msg.UserName, PermanentData.Serial, PermanentData.Devname, 0, 0, DRPMessageType.HARDWARE_ERROR);
                    await tcp.Send(response.ToString(), writer);
                    System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());
                    return;
                }
                TempProfile profile = new TempProfile(msg.UserName, msg.Token, 0);
                if (uhl.currentServedUser() == null)
                {
                    //if no user uses the weight
                    uhl.StartUser(profile);
                    try
                    {
                        double w = uhl.getWeight(UserHardwareLinker.WEIGH_AVG);
                        DRP response = new DRP(DRPDevType.RBPI, msg.UserName, PermanentData.Serial, PermanentData.Devname, (float)w, 0, DRPMessageType.DATA);
                        //sending result to client
                        Task sendTask = tcp.Send(response.ToString(), writer);
                        System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());

                        //sending to cloud
                        Dictionary<string, string> jsend = new Dictionary<string, string>();
                        jsend.Add("username", msg.UserName);
                        jsend.Add("weigh", w.ToString());

                        Task cloudTask = null;
                        if (msg.UserName != null)
                        {
                            string sendToCloud = JsonConvert.SerializeObject(jsend);
                            cloudTask = AzureIoTHub.SendDeviceToCloudMessageAsync(sendToCloud);
                            System.Diagnostics.Debug.WriteLine("Send to cloud with user: " + msg.UserName);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("!!!!!!!!!!Not sent!");
                        }
                        await sendTask;
                        uhl.FinishUser();
                        if (cloudTask != null)
                            await cloudTask;
                        return;
                    }
                    catch
                    {
                        DRP response = new DRP(DRPDevType.RBPI, msg.UserName, PermanentData.Serial, PermanentData.Devname, 0, 0, DRPMessageType.HARDWARE_ERROR);
                        await tcp.Send(response.ToString(), writer);
                        System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());
                        uhl.FinishUser();
                        return;
                    }

                }
                else
                {
                    //if somwone already uses the weight
                    DRP response = new DRP(DRPDevType.RBPI, msg.UserName, PermanentData.Serial, PermanentData.Devname, 0, 0, DRPMessageType.IN_USE);
                    await tcp.Send(response.ToString(), writer);
                    System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());
                }
            }
            else if (msg.MessageType == DRPMessageType.ILLEGAL)
            {
                //never actually happens.
                DRP response = new DRP(DRPDevType.RBPI, "", PermanentData.Serial, PermanentData.Devname, 0, 0, DRPMessageType.ACK);
                await tcp.Send(response.ToString(), writer);
                System.Diagnostics.Debug.WriteLine("message sent: " + response.ToString());
            }
            else
            {
                throw new Exception();
            }

        }

        public void socket_onError(string message)
        {
            System.Diagnostics.Debug.WriteLine("There was an error in socket: " + message);
        }
    }
}
