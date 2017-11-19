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


namespace IoTWeight
{
    [Activity(Label = "GetIPAddress")]
    public class GetIPAddress : Activity
    {
        private IMobileServiceTable<RaspberryTable> raspberryTableRef;
        private IMobileServiceTable<weighTable> weighTableRef;
        uint ipAddress = 0;
        string ipaddress = "";
        float currentWeigh = 0;   //to be returned from Raspberry
        TCPSender tcps;
        string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;  //username

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            // This activity receives from QRActivity a string which is the Raspberry's QRCode,  and returns this Raspberry's IP Address
            SetContentView(Resource.Layout.DisplayWeigh);

            Button weighAgainButton = FindViewById<Button>(Resource.Id.WeighAgainButton);
            weighAgainButton.SetBackgroundColor(Android.Graphics.Color.LightBlue);
            weighAgainButton.Visibility = ViewStates.Gone;

            Button deleteButton = FindViewById<Button>(Resource.Id.DeleteButton);
            deleteButton.SetBackgroundColor(Android.Graphics.Color.SteelBlue);
            deleteButton.Visibility = ViewStates.Gone;

            deleteButton.Click += async (o, e) =>
            {
                //FindViewById<TextView>(Resource.Id.date_display).Text = "Deleting from database. Please Wait.";
                //deleteButton.Visibility = ViewStates.Gone;
                deleteButton.Enabled = false;
                FindViewById<TextView>(Resource.Id.Text1).Text = "Deleting weigh from database. This might take a few seconds.\n Please Wait...";
                FindViewById<TextView>(Resource.Id.currentWeigh).Text = "";

                Console.WriteLine("SLEEPING before deleting !!!!!!!!!!!!!!!!!!!");
                await Task.Delay(5000);

                await deleteWeigh();
                FindViewById<Button>(Resource.Id.WeighAgainButton).Visibility = ViewStates.Visible;

            };

            weighAgainButton.Click += (sender, e) =>
            {
                Finish();
                //var intent = new Intent(this, typeof(LogInActivity)).SetFlags(ActivityFlags.ClearTask);
                //var intent = new Intent(this, typeof(LogInActivity))
                //.SetFlags(ActivityFlags.ReorderToFront);
                //StartActivity(intent);
            };

            string qrCode = Intent.GetStringExtra("qrcode") ?? "QR Code not available";
            Console.WriteLine("In GetIPAddress activity and qrCode = {0}", qrCode);

            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            raspberryTableRef = client.GetTable<RaspberryTable>();
            //TODO BAR:   add to protocol OK message, so that app will notify Rpi that it has recieved the weight
            try
            {
                //some inserts for debugging:
                //var record1 = new RaspberryTable
                //{
                //    QRCode = "Testing 2",
                //    IPAddress = "18.0.0.7",
                //};
                //await raspberryTableRef.InsertAsync(record1);
                //if (8 == 8)
                //    return;

                var ipAddressList = await raspberryTableRef.Where(item => (item.QRCode == qrCode)).ToListAsync();
                //TODO:  handle this case
                if (ipAddressList.Count == 0)
                {
                    ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.ProgressCircle);
                    progress.Visibility = ViewStates.Gone;

                    string sorryMessage = "Sorry!\nNo Raspberries with the QR code: " + qrCode + " were found in the database. \nEither the Raspberry was not registered in the cloud via the installation process  , \nor there was an error in the barcode scan. \nIn the later case, please press the back button and try to scan again";
                    //CreateAndShowDialog("No Raspberries with the QR code: " + qrCode + " were found in the database. \nEither the Raspberry was not registered in the cloud via the installation process  , \nor there was an error in the barcode scan. \nIn the later case, please press the back button and try to scan again", "Sorry:");
                    FindViewById<TextView>(Resource.Id.Text1).Text = sorryMessage;
                }

                else
                {
                    var address = ipAddressList[0];
                    ipaddress = address.IPAddress;
                    tcps = new TCPSender(); //Creating the socket on the default port, which is 9888.
                    await TalkToRaspberry();
                }

            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }

        }


        //signature previously had return type void instread of Task.  also,  call to TalkToRaspberry wasn't awaited.
        private async Task TalkToRaspberry()
        {
            string ip = ipaddress;
            if (!await tcps.Connect(ip))
            {
                //TODO:   display message to user
                System.Diagnostics.Debug.WriteLine("Connection failed.");
                handleGUI_OnFailure("Connection failed.");
                return;
            }

            DRP result = await sendSCANNED(); //TODO: check what happend if there is no answer

            if (result == null)
            {
                //in case there's no answer from the server
                handleGUI_OnFailure("Connection Timeout");
                return;
            }
            FindViewById<TextView>(Resource.Id.tv_servname).Visibility = ViewStates.Visible;
            FindViewById<TextView>(Resource.Id.tv_servname).Text = result.ServName;
            if (result.MessageType == DRPMessageType.DATA)
            {
                currentWeigh = result.Data;
                handleGUI_OnSuccess(result.Data.ToString());
                return;
            }
            else if (result.MessageType == DRPMessageType.IN_USE)
            {
                handleGUI_OnFailure("the scale is in use");
                return;
            }
            else if (result.MessageType == DRPMessageType.ILLEGAL || result.MessageType == DRPMessageType.HARDWARE_ERROR)
            {

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

            FindViewById<Button>(Resource.Id.DeleteButton).Visibility = ViewStates.Visible;

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
        private async Task<DRP> sendSCANNED()
        {

            //TODO: what is the username? what is the serial?
            DRP msg = new DRP(DRPDevType.APP, ourUserId, "111", "don't care", 0, 0, DRPMessageType.SCANNED);

            //sending the message
            await tcps.Send(msg.ToString());
            string ans = await tcps.Receive();

            try
            {
                DRP rec = DRP.deserializeDRP(ans);
                return rec;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("deserializion failed: " + ans);
                return null;
            }

        }

        //Find and delete the last weight in the database in order to delete it:
        private async Task deleteWeigh()
        {
            weighTable toDelete = null;
            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            try
            {
                weighTableRef = client.GetTable<weighTable>();
                DateTime today = DateTime.Now;
                DateTime earliestDate = today.AddMinutes(-5);
                
                var toBeDeletedList = await weighTableRef.Where(item => (item.username == ourUserId) && (item.createdAt >= earliestDate) && (item.weigh == currentWeigh)).ToListAsync();
                if (toBeDeletedList.Count == 0)
                {
                    toDelete = null;
                    FindViewById<TextView>(Resource.Id.Text1).Text = "Cannot Delete.\nWeigh has not yet arrived to the database in the cloud";
                    FindViewById<Button>(Resource.Id.DeleteButton).Text = "Retry to Delete";
                    FindViewById<Button>(Resource.Id.DeleteButton).Enabled = true;
                }
                else if (toBeDeletedList.Count > 1)
                {
                    //Only delete the last weigh:
                    toDelete = toBeDeletedList[toBeDeletedList.Count - 1];
                    await weighTableRef.DeleteAsync(toDelete);
                    FindViewById<TextView>(Resource.Id.Text1).Text = "Deleted Successfully !";
                }
                else
                {
                    toDelete = toBeDeletedList[0];
                    await weighTableRef.DeleteAsync(toDelete);
                    FindViewById<TextView>(Resource.Id.Text1).Text = "Deleted Successfully !";
                }
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
            

        }

    }
}












//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.WindowsAzure.MobileServices;
//using Java.Util;
//using Java.Net;


//namespace IoTWeight
//{
//    [Activity(Label = "GetIPAddress")]
//    public class GetIPAddress : Activity
//    {
//        private IMobileServiceTable<RaspberryTable> raspberryTableRef;
//        uint ipAddress = 0;
//        string ipaddress = "";
//        float currentWeigh = 0;   //to be returned from Raspberry
//        TCPSender tcps;
//        string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;  //username

//        protected override async void OnCreate(Bundle savedInstanceState)
//        {
//            base.OnCreate(savedInstanceState);

//            // Create your application here
//            // This activity receives from QRActivity a string which is the Raspberry's QRCode,  and returns this Raspberry's IP Address
//            SetContentView(Resource.Layout.DisplayWeigh);

//            string qrCode = Intent.GetStringExtra("qrcode") ?? "QR Code not available";
//            Console.WriteLine("In GetIPAddress activity and qrCode = {0}", qrCode);
//            //TODO:  DELETE LATER:
//            //qrCode = "Testing 2";

//            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
//            raspberryTableRef = client.GetTable<RaspberryTable>();
//            //TODO BAR:   add to protocol OK message, so that app will notify Rpi that it has recieved the weight
//            try
//            {
//                //some inserts for debugging:
//                //var record1 = new RaspberryTable
//                //{
//                //    QRCode = "Testing 2",
//                //    IPAddress = "18.0.0.7",
//                //};
//                //await raspberryTableRef.InsertAsync(record1);
//                //if (8 == 8)
//                //    return;

//                var ipAddressList = await raspberryTableRef.Where(item => (item.QRCode == qrCode)).ToListAsync();
//                //TODO:  handle this case
//                if (ipAddressList.Count == 0)
//                {
//                    ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.ProgressCircle);
//                    progress.Visibility = ViewStates.Gone;

//                    string sorryMessage = "Sorry!\nNo Raspberries with the QR code: " + qrCode + " were found in the database. \nEither the Raspberry was not registered in the cloud via the installation process  , \nor there was an error in the barcode scan. \nIn the later case, please press the back button and try to scan again";
//                    //CreateAndShowDialog("No Raspberries with the QR code: " + qrCode + " were found in the database. \nEither the Raspberry was not registered in the cloud via the installation process  , \nor there was an error in the barcode scan. \nIn the later case, please press the back button and try to scan again", "Sorry:");
//                    FindViewById<TextView>(Resource.Id.Text1).Text = sorryMessage;
//                }

//                else
//                {
//                    var address = ipAddressList[0];
//                    ipaddress = address.IPAddress;
//                    tcps = new TCPSender(); //Creating the socket on the default port, which is 9888.
//                    await TalkToRaspberry();  
//                }

//            }
//            catch (Exception e)
//            {
//                CreateAndShowDialog(e, "Error");
//            }

//        }




//        //signature previously had return type void instread of Task.  also,  call to TalkToRaspberry wasn't awaited.
//        private async Task TalkToRaspberry()
//        {
//            string ip = ipaddress;
//            if (!tcps.Connect(ip))
//            {
//                //TODO:   display message to user
//                System.Diagnostics.Debug.WriteLine("Connection failed.");
//                handleGUI_OnFailure("Connection failed.");
//                return;
//            }

//            System.Diagnostics.Debug.WriteLine("before send scanned");
//            DRP result = await sendSCANNED(long.Parse("55665566")); //TODO: replace the string here with the scanning result
//            System.Diagnostics.Debug.WriteLine("after send scanned");
//            if (result == null)
//            {
//                //in case there's no answer from the server
//                handleGUI_OnFailure("Connection Timeout");
//                return;
//            }

//            if (result.MessageType == DRPMessageType.DATA)
//            {
//                handleGUI_OnSuccess(result.Data[0].ToString());
//                return;
//            }
//            else if (result.MessageType == DRPMessageType.IN_USE)
//            {
//                handleGUI_OnFailure("the scale is in use");
//                return;
//            }
//            else if (result.MessageType == DRPMessageType.ILLEGAL || result.MessageType == DRPMessageType.HARDWARE_ERROR)
//            {

//                handleGUI_OnFailure("The scaling could not been done due to error.");
//                return;
//            }
//            //TODO: send ACKs (we'll do it later)
//        }


//        private void handleGUI_OnFailure(string answerFromRPi)
//        {
//            ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.ProgressCircle);
//            progress.Visibility = ViewStates.Gone;
//            FindViewById<TextView>(Resource.Id.Text1).Text = answerFromRPi;
//        }

//        private void handleGUI_OnSuccess(string answerFromRPi)
//        {
//            ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.ProgressCircle);
//            progress.Visibility = ViewStates.Gone;
//            FindViewById<TextView>(Resource.Id.Text1).Text = "Your Current Weight:";
//            FindViewById<TextView>(Resource.Id.currentWeigh).Text = answerFromRPi;
//        }

//        private void CreateAndShowDialog(Exception exception, String title)
//        {
//            CreateAndShowDialog(exception.Message, title);
//        }

//        private void CreateAndShowDialog(string message, string title)
//        {
//            AlertDialog.Builder builder = new AlertDialog.Builder(this);

//            builder.SetMessage(message);
//            builder.SetTitle(title);
//            builder.Create().Show();
//        }

//        //sending a message of type SCANNED to the RBPi and waiting to the result
//        private async Task<DRP> sendSCANNED(long serial, int timeout = 10000)
//        {

//            //DRP msg = new DRP(DRPDevType.APP, "a_monkey", 343434, serial, new List<float>(), 0, DRPMessageType.SCANNED); //TODO: what is the username? what is the serial?
//            DRP msg = new DRP(DRPDevType.APP, ourUserId, 343434, serial, new List<float>(), 0, DRPMessageType.SCANNED);
//            //sending the message
//            tcps.Send(msg.ToString());
//            //await tcps.Send(msg.ToString());

//            //waiting for result
//            Task<string> rec_str_task = tcps.Receive();
//            if (rec_str_task.Wait(timeout))
//            {
//                string rec_str = rec_str_task.Result;
//                DRP rec = DRP.deserializeDRP(rec_str); //TODO: do not assume for DRP, needs to be checked!
//                return rec; //returns the RBPi's response
//            }
//            else
//            {
//                //if there is no response, returns null
//                return null;
//            }
//        }

//        private string findIpFromSerial(string serial)
//        {
//            //TODO: I've hardcoded my RPi ip here. In the final version we need to look for the ip in the DB
//            //return "192.168.1.104";
//            //Ron's laptop ip:
//            return "10.0.0.2";
//        }
//    }
//}












//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.WindowsAzure.MobileServices;
//using Java.Util;
//using Java.Net;


//namespace IoTWeight
//{
//    [Activity(Label = "GetIPAddress")]
//    public class GetIPAddress : Activity
//    {
//        private IMobileServiceTable<RaspberryTable> raspberryTableRef;
//        uint ipAddress = 0;
//        string ipaddress = "";
//        float currentWeigh = 0;   //to be returned from Raspberry
//        TCPSender tcps;
//        string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;  //username

//        protected override async void OnCreate(Bundle savedInstanceState)
//        {
//            base.OnCreate(savedInstanceState);

//            // Create your application here
//            // This activity receives from QRActivity a string which is the Raspberry's QRCode,  and returns this Raspberry's IP Address
//            SetContentView(Resource.Layout.DisplayWeigh);

//            string qrCode = Intent.GetStringExtra("qrcode") ?? "QR Code not available";
//            Console.WriteLine("In GetIPAddress activity and qrCode = {0}", qrCode);
//            //TODO:  DELETE LATER:
//            //qrCode = "Testing 2";

//            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
//            raspberryTableRef = client.GetTable<RaspberryTable>();
//            //TODO BAR:   add to protocol OK message, so that app will notify Rpi that it has recieved the weight
//            try
//            {
//                //some inserts for debugging:
//                //var record1 = new RaspberryTable
//                //{
//                //    QRCode = "Testing 2",
//                //    IPAddress = "18.0.0.7",
//                //};
//                //await raspberryTableRef.InsertAsync(record1);
//                //if (8 == 8)
//                //    return;

//                var ipAddressList = await raspberryTableRef.Where(item => (item.QRCode == qrCode)).ToListAsync();
//                //TODO:  handle this case
//                if (ipAddressList.Count == 0)
//                {
//                    ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.ProgressCircle);
//                    progress.Visibility = ViewStates.Gone;

//                    string sorryMessage = "Sorry!\nNo Raspberries with the QR code: " + qrCode + " were found in the database. \nEither the Raspberry was not registered in the cloud via the installation process  , \nor there was an error in the barcode scan. \nIn the later case, please press the back button and try to scan again";
//                    //CreateAndShowDialog("No Raspberries with the QR code: " + qrCode + " were found in the database. \nEither the Raspberry was not registered in the cloud via the installation process  , \nor there was an error in the barcode scan. \nIn the later case, please press the back button and try to scan again", "Sorry:");
//                    FindViewById<TextView>(Resource.Id.Text1).Text = sorryMessage;
//                }

//                else
//                {
//                    var address = ipAddressList[0];
//                    ipaddress = address.IPAddress;
//                    tcps = new TCPSender(); //Creating the socket on the default port, which is 9888.
//                    await TalkToRaspberry();  
//                }

//            }
//            catch (Exception e)
//            {
//                CreateAndShowDialog(e, "Error");
//            }

//        }




//        //signature previously had return type void instread of Task.  also,  call to TalkToRaspberry wasn't awaited.
//        private async Task TalkToRaspberry()
//        {
//            string ip = ipaddress;
//            if (!tcps.Connect(ip))
//            {
//                //TODO:   display message to user
//                System.Diagnostics.Debug.WriteLine("Connection failed.");
//                handleGUI_OnFailure("Connection failed.");
//                return;
//            }

//            System.Diagnostics.Debug.WriteLine("before send scanned");
//            DRP result = await sendSCANNED(long.Parse("55665566")); //TODO: replace the string here with the scanning result
//            System.Diagnostics.Debug.WriteLine("after send scanned");
//            if (result == null)
//            {
//                //in case there's no answer from the server
//                handleGUI_OnFailure("Connection Timeout");
//                return;
//            }

//            if (result.MessageType == DRPMessageType.DATA)
//            {
//                handleGUI_OnSuccess(result.Data[0].ToString());
//                return;
//            }
//            else if (result.MessageType == DRPMessageType.IN_USE)
//            {
//                handleGUI_OnFailure("the scale is in use");
//                return;
//            }
//            else if (result.MessageType == DRPMessageType.ILLEGAL || result.MessageType == DRPMessageType.HARDWARE_ERROR)
//            {

//                handleGUI_OnFailure("The scaling could not been done due to error.");
//                return;
//            }
//            //TODO: send ACKs (we'll do it later)
//        }


//        private void handleGUI_OnFailure(string answerFromRPi)
//        {
//            ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.ProgressCircle);
//            progress.Visibility = ViewStates.Gone;
//            FindViewById<TextView>(Resource.Id.Text1).Text = answerFromRPi;
//        }

//        private void handleGUI_OnSuccess(string answerFromRPi)
//        {
//            ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.ProgressCircle);
//            progress.Visibility = ViewStates.Gone;
//            FindViewById<TextView>(Resource.Id.Text1).Text = "Your Current Weight:";
//            FindViewById<TextView>(Resource.Id.currentWeigh).Text = answerFromRPi;
//        }

//        private void CreateAndShowDialog(Exception exception, String title)
//        {
//            CreateAndShowDialog(exception.Message, title);
//        }

//        private void CreateAndShowDialog(string message, string title)
//        {
//            AlertDialog.Builder builder = new AlertDialog.Builder(this);

//            builder.SetMessage(message);
//            builder.SetTitle(title);
//            builder.Create().Show();
//        }

//        //sending a message of type SCANNED to the RBPi and waiting to the result
//        private async Task<DRP> sendSCANNED(long serial, int timeout = 10000)
//        {

//            //DRP msg = new DRP(DRPDevType.APP, "a_monkey", 343434, serial, new List<float>(), 0, DRPMessageType.SCANNED); //TODO: what is the username? what is the serial?
//            DRP msg = new DRP(DRPDevType.APP, ourUserId, 343434, serial, new List<float>(), 0, DRPMessageType.SCANNED);
//            //sending the message
//            tcps.Send(msg.ToString());
//            //await tcps.Send(msg.ToString());

//            //waiting for result
//            Task<string> rec_str_task = tcps.Receive();
//            if (rec_str_task.Wait(timeout))
//            {
//                string rec_str = rec_str_task.Result;
//                DRP rec = DRP.deserializeDRP(rec_str); //TODO: do not assume for DRP, needs to be checked!
//                return rec; //returns the RBPi's response
//            }
//            else
//            {
//                //if there is no response, returns null
//                return null;
//            }
//        }

//        private string findIpFromSerial(string serial)
//        {
//            //TODO: I've hardcoded my RPi ip here. In the final version we need to look for the ip in the DB
//            //return "192.168.1.104";
//            //Ron's laptop ip:
//            return "10.0.0.2";
//        }
//    }
//}