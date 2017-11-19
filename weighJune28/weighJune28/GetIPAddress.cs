using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.WindowsAzure.MobileServices;
using Java.Util;
using Java.Net;


namespace weighJune28
{
    [Activity(Label = "GetIPAddress")]
    public class GetIPAddress : Activity
    {
        private IMobileServiceTable<RaspberryTable> raspberryTableRef;
        string ipaddress = "";
        float currentWeigh = 0;   //to be returned from Raspberry
        string qrCode;
        TCPSender tcps;
        string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;  //username

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            // This activity receives from QRActivity a string which is the Raspberry's QRCode,  and returns this Raspberry's IP Address
            SetContentView(Resource.Layout.DisplayWeigh);

            //qrCode = Intent.GetStringExtra("qrcode") ?? "QR Code not available";
            qrCode = (121212).ToString();
            Console.WriteLine("In GetIPAddress activity and qrCode = {0}", qrCode);
            //TODO:  DELETE LATER:
            //qrCode = "Testing 2";
    
            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            raspberryTableRef = client.GetTable<RaspberryTable>();
            //TODO BAR:   add to protocol OK message, so that app will notify Rpi that it has recieved the weight
            try
            {
                //some inserts for debugging:
                //var record1 = new RaspberryTable
                //{
                //    QRCode = "Testing 2",
                //    IPAddress = "10.0.0.2",
                //};
                //await raspberryTableRef.InsertAsync(record1);
                //if (8 == 8)
                //    return;

                var ipAddressList = await raspberryTableRef.Where(item => (item.QRCode == qrCode) ).ToListAsync();
                //TODO:  handle this case
                if (ipAddressList.Count == 0)
                {
                    CreateAndShowDialog("Sorry:", "No Raspberries with the scanned QR Code were found in the database. The Raspberry must first be registered in the cloud via the installation process ");
                }
                
                else 
                {
                    var address = ipAddressList[0];
                    //ipAddress = address.IPNumber;
                    ipaddress = address.IPAddress;
                    // TODO:   start function that sends protocol messages to Raspberry. Bar's code:
                    tcps = new TCPSender(); //Creating the socket on the default port, which is 9888.
                    TalkToRaspberry();

                    //string message = "Ip address of qr code " + qrCode + ": " + ipAddress;
                    ////CreateAndShowDialog( message, "Debugg: ");
                    //ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.ProgressCircle);
                    //progress.Visibility = ViewStates.Gone;
                    //FindViewById<TextView>(Resource.Id.Text1).Text = "Your Current Weight:";
                    ////TODO :  DELETE LATER:
                    //currentWeigh = 97.4f;
                    //FindViewById<TextView>(Resource.Id.currentWeigh).Text = Convert.ToString(currentWeigh);

                }
               
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }

        }


        private async void TalkToRaspberry()
        {
            string ip = ipaddress;
            if (!tcps.Connect(ip))
            {
                //TODO:   display message to user
                System.Diagnostics.Debug.WriteLine("Connection failed.");
                handleGUI_OnFailure("Connection failed.");
                return;
            }

            DRP result = await sendSCANNED(1122); //TODO: replace the string here with the scanning result
            if (result == null)
            {
                //in case there no answer from the server
                //tv.Text = "Connection Timeout";
                handleGUI_OnFailure("Connection Timeout");
                return;
            }

            if (result.MessageType == DRPMessageType.DATA)
            {
                //TODO: Show the scaling result on the screen
                //tv.Text = result.Data[0].ToString();
                handleGUI_OnSuccess(result.Data[0].ToString());
                return;
            }
            else if (result.MessageType == DRPMessageType.IN_USE)
            {
                //TODO: Show a message for the user that informs him the device is already in use by another user.
                //tv.Text = "the scale is in use";
                handleGUI_OnFailure("the scale is in use");
                return;
            }
            else if (result.MessageType == DRPMessageType.ILLEGAL || result.MessageType == DRPMessageType.HARDWARE_ERROR)
            {
                //TODO: The scaling could not been done due to error.
                //tv.Text = "The scaling could not been done due to error.";
                handleGUI_OnFailure("The scaling could not been done due to error.");
                return;
            }
            //TODO: send ACKs (we'll do it later)
        }


        private void handleGUI_OnFailure(string answerFromRPi)
        {
            ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.ProgressCircle);
            progress.Visibility = ViewStates.Gone;
            FindViewById<TextView>(Resource.Id.Text1).Text = answerFromRPi;
        }

        private void handleGUI_OnSuccess(string answerFromRPi)
        {
            ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.ProgressCircle);
            progress.Visibility = ViewStates.Gone;
            FindViewById<TextView>(Resource.Id.Text1).Text = "Your Current Weight:";
            FindViewById<TextView>(Resource.Id.currentWeigh).Text = answerFromRPi;
        }

        private void CreateAndShowDialog(Exception exception, String title)
        {
            CreateAndShowDialog(exception.Message, title);
        }

        private void CreateAndShowDialog(string message, string title)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }

        //sending a message of type SCANNED to the RBPi and waiting to the result
        private async Task<DRP> sendSCANNED(long serial, int timeout = 10000)
        {

            //DRP msg = new DRP(DRPDevType.APP, "a_monkey", 343434, serial, new List<float>(), 0, DRPMessageType.SCANNED); //TODO: what is the username? what is the serial?
            DRP msg = new DRP(DRPDevType.APP, ourUserId, 343434, serial, new List<float>(), 0, DRPMessageType.SCANNED);
            //sending the message
            tcps.Send(msg.ToString());

            //waiting for result
            Task<string> rec_str_task = tcps.Receive();
            if (rec_str_task.Wait(timeout))
            {
                string rec_str = rec_str_task.Result;
                DRP rec = DRP.deserializeDRP(rec_str); //TODO: do not assume for DRP, needs to be checked!
                return rec; //returns the RBPi's response
            }
            else
            {
                //in there is no response, returns null
                return null;
            }
        }

        private string findIpFromSerial(string serial)
        {
            //TODO: I've hardcoded my RPi ip here. In the final version we need to look for the ip in the DB
            //return "192.168.1.104";
            //Ron's laptop ip:
            return "10.0.0.2";
        }
    }
}