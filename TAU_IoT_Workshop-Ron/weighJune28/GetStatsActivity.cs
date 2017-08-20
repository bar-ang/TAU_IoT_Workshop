using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    [Activity(Label = "GetStatsActivity")]
    public class GetStatsActivity : Activity
    {
        List<float> lastWeights = new List<float>();
        List<long> ourUserMacAddresses = new List<long>();
        List<WeighDatePair> weighDateList = new List<WeighDatePair>();
        // Client reference.
        //private MobileServiceClient client;
        // URL of the mobile app backend.
        //const string applicationURL = @"https://weighjune28.azurewebsites.net";
        private IMobileServiceTable<weighTable> weighTableRef;
        private IMobileServiceTable<UsersTable> UsersTableRef;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.multipleColumnsLayout);

            // Create your application here
            //CurrentPlatform.Init();
            // Create the client instance, using the mobile app backend URL.
            //client = new MobileServiceClient(applicationURL);
            // Get the MobileServiceClient from the current activity instance.


            //GET MAC ADDRESS
            long macAdd = getMacAddress();
            Console.WriteLine("MAC address is: {0}", macAdd);
            //"02:00:00:00:00:00" is default
            if (macAdd == 2199023255552)
            {
                CreateAndShowDialog("Please exit the application and try again", "Error in Identifying the User");
            }
            string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;
            Console.WriteLine("sid is: {0}", ourUserId);

            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            weighTableRef = client.GetTable<weighTable>();
            UsersTableRef = client.GetTable<UsersTable>();

            try
            {
                DateTime today = DateTime.Now;
                DateTime lastMonth = today.AddDays(-30);

                //some inserts for debugging:
                //macAdd = A0:82:1F:6A:40:5B
                //var newWeighTableRecord = new weighTable
                //{
                //    MACaddress = macAdd,
                //    weigh = 311F,
                //    createdAt = today.AddDays(-9)
                //};
                //var UserTableRecord = new UsersTable
                //{
                //    MACaddress = macAdd,
                //    UniqueUsername = ourUserId
                //};
                //await weighTableRef.InsertAsync(newWeighTableRecord);
                //await UsersTableRef.InsertAsync(UserTableRecord);

                //first check if the user exists in the database:
                var usersList = await UsersTableRef.Where(item => (item.UniqueUsername == ourUserId) && (item.MACaddress == macAdd)).ToListAsync();
                if (usersList.Count == 0)
                {
                    var newUserMacAddTableRecord = new UsersTable
                    {
                        UniqueUsername = ourUserId,
                        MACaddress = macAdd
                    };
                    await UsersTableRef.InsertAsync(newUserMacAddTableRecord);
                }
                
                //SQL Table 1:      MACaddress        weigh       createdAt
                //UsersTable:       UniqueUsername    MACaddress
                var convertToListOfStrings = await UsersTableRef.Where(item => (item.UniqueUsername == ourUserId)).ToListAsync();
                foreach (UsersTable userRecord in convertToListOfStrings)
                {
                    long m = userRecord.MACaddress;
                    ourUserMacAddresses.Add(m);
                }
                //ourUserMacAddresses.Add(4023);  //TODO:  delete.  for debug purpose only
                var list9 = await weighTableRef.Where(item => (ourUserMacAddresses.Contains(item.MACaddress) ) && (item.createdAt > lastMonth) ).ToListAsync();

                foreach (weighTable weight in list9)
                {
                    float currW = weight.weigh;
                    string weighInStringFormat = Convert.ToString(currW);
                    lastWeights.Add(currW);
                    Console.WriteLine("weight = : {0}", currW);
                    DateTime date1 = weight.createdAt;
                    string dateSring = Convert.ToString(date1);
                    Console.WriteLine(date1.ToString());
                    weighDateList.Add(new WeighDatePair
                    {
                        myWeight = weighInStringFormat,
                        dateOfWeigh = dateSring
                    });
                }


                if (list9.Count == 0)
                {
                    CreateAndShowDialog("Please Exit the Application and then Weigh yourself and try again", "No Previous Weighs Found");
                }
                else
                {
                    var listView = FindViewById<ListView>(Resource.Id.listView);
                    listView.Adapter = new WeighDatePairAdapter(this, weighDateList);
                }


                //DateTime date2 = new DateTime(2017, 6, 28, 16, 5, 0);


                //Console.WriteLine("last month from today is : {0}", lastMonth);
                //var list1 = await weighTableRef.Where(item => (item.username == "Reut") && (item.createdAt > date2) ).ToListAsync();
                //var list1 = await weighTableRef.Where(item => (item.username == "Reut") && (item.createdAt > lastMonth)).ToListAsync();



                //var allowedStatus = new[]{ "A", "B", "C" };
                //var filteredOrders = orders.Order.Where(o => allowedStatus.Contains(o.StatusCode));


                //var innerJoinQuery =
                //from users in UsersTableRef
                //join weighsItems in weighTableRef on users.MACaddress equals weighsItems.MACaddress
                //select new { UsersTable = users.MACaddress, weighTable = weighsItems.MACaddress }; //produces flat sequence


                //            var innerJoinQuery =
                //from category in categories
                //join prod in products on category.ID equals prod.CategoryID
                //select new { ProductName = prod.Name, Category = category.Name }; //produces flat sequence






            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }

            //this.ListAdapter = new ArrayAdapter<float>(this, Android.Resource.Layout.SimpleListItem1, lastWeights);
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

        public static long getMacAddress()
        {
            long macadd = 0;
            //long arcadi = 2;
            //Console.WriteLine("arcadi = {0}", arcadi << 40);
            int i = 40;
            string macAddress = string.Empty;

            var all = Collections.List(Java.Net.NetworkInterface.NetworkInterfaces);

            foreach (var interfaces in all)
            {
                if (!(interfaces as Java.Net.NetworkInterface).Name.Contains("wlan0")) continue;

                var macBytes = (interfaces as
                Java.Net.NetworkInterface).GetHardwareAddress();
                if (macBytes == null) continue;

                var sb = new System.Text.StringBuilder();
                foreach (var b in macBytes)
                {
                    string convertedByte = string.Empty;
                    convertedByte = (b & 0xFF).ToString("X2") + ":";

                    if (convertedByte.Length == 1)
                    {
                        convertedByte.Insert(0, "0");
                    }
                    sb.Append(convertedByte);

                    //Console.WriteLine("byte b = {0}", b);
                    long shifted = ((long)b) << i;
                    macadd = macadd + shifted;
                    i = i - 8;
                    //Console.WriteLine("macadd = {0}", macadd);
                }

                macAddress = sb.ToString().Remove(sb.Length - 1);

                return macadd;
            }
            //return "02:00:00:00:00:00";
            long def = 2;
            return (def << 40);
        }

    }
}