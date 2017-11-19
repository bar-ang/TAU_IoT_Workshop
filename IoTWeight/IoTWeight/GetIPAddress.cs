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
        bool isStreamAnalytics = false;     //if Stream Analytics fails,  set this to true
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
            weighAgainButton.Visibility = ViewStates.Gone;
            Button deleteButton = FindViewById<Button>(Resource.Id.DeleteButton);
            deleteButton.Visibility = ViewStates.Gone;

            deleteButton.Click += async (o, e) =>
            {
                //FindViewById<TextView>(Resource.Id.date_display).Text = "Deleting from database. Please Wait.";
                //deleteButton.Visibility = ViewStates.Gone;
                deleteButton.Enabled = false;
                FindViewById<TextView>(Resource.Id.Text1).Text = "Deleting weigh from database. This might take a few seconds.\n Please Wait...";
                FindViewById<TextView>(Resource.Id.currentWeigh).Text = "";

                Console.WriteLine("SLEEPING before deleting in order to allow weight to get to cloud through Stream Analytics !!!!!!!!!!!!!!!!!!!");

                try
                {
                    await Task.Delay(5000);  //do not delete this.  it's necessary so that weight can get to the cloud
                    await deleteWeigh();
                    FindViewById<Button>(Resource.Id.WeighAgainButton).Visibility = ViewStates.Visible;
                }
                catch (Exception ex)
                {
                    CreateAndShowDialog(ex, "Error");
                }

            };

            weighAgainButton.Click += (sender, e) =>
            {
                Finish();
            };

            string qrCode = Intent.GetStringExtra("qrcode") ?? "QR Code not available";
            Console.WriteLine("In GetIPAddress activity and qrCode = {0}", qrCode);

            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            raspberryTableRef = client.GetTable<RaspberryTable>();
            
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
                
                if (ipAddressList.Count == 0)
                {
                    ProgressBar progress = FindViewById<ProgressBar>(Resource.Id.ProgressCircle);
                    progress.Visibility = ViewStates.Gone;

                    string sorryMessage = "Sorry!\nNo Raspberries with the QR code: " + qrCode + " were found in the database. \nEither the Raspberry was not registered in the cloud via the installation process  , \nor there was an error in the barcode scan. \nIn the later case, please press the back button and try to scan again";
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


        private async Task TalkToRaspberry()
        {
            string ip = ipaddress;
            if (!await tcps.Connect(ip))
            {
                System.Diagnostics.Debug.WriteLine("Connection failed.");
                handleGUI_OnFailure("Connection failed.");
                return;
            }

            DRP result = await sendSCANNED();

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

                if (isStreamAnalytics)
                {
                    var newweightablerecord = new weighTable
                    {
                        username = ourUserId,
                        weigh = currentWeigh
                    };
                    try
                    {
                        MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
                        weighTableRef = client.GetTable<weighTable>();
                        await weighTableRef.InsertAsync(newweightablerecord);
                    }
                    catch (Exception e)
                    {
                        CreateAndShowDialog(e, "Error");
                    }
                }

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
            DRP msg = new DRP(DRPDevType.APP, ourUserId, "111", "don't care", 0, 0, DRPMessageType.SCANNED);
            try
            {
                //sending the message
                await tcps.Send(msg.ToString());
                string ans = await tcps.Receive();
                DRP rec = DRP.deserializeDRP(ans);
                return rec;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception caught in SendScanned : " + e.Message);
                return null;
            }

        }

        //Find and delete the last weight in the database in order to delete it:
        private async Task deleteWeigh()
        {
            //IMobileServiceTable<weighTable> weighTableRef;
            weighTable toDelete = null;
            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            try
            {
                weighTableRef = client.GetTable<weighTable>();
                DateTime today = DateTime.Now;
                DateTime earliestDate = today.AddMinutes(-5);

                //var toBeDeletedList = await weighTableRef.Where(item => (item.username == ourUserId) && (item.createdAt >= earliestDate) && (item.weigh == currentWeigh)).ToListAsync();
                //int roundedUp = (int)Math.Ceiling(precise);
                int roundedUp = (int)Math.Ceiling(currentWeigh);
                var toBeDeletedList = await weighTableRef.Where(item => (item.username == ourUserId) && (item.createdAt >= earliestDate) && (item.weigh <= roundedUp) && (item.weigh >= roundedUp-1)).ToListAsync();
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