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

namespace weighJune28
{
    [Activity(Label = "LogInActivity")]
    public class LogInActivity : Activity
    {
        //// Client reference.
        //private MobileServiceClient client;
        //// URL of the mobile app backend.
        //const string applicationURL = @"https://weighjune28.azurewebsites.net";
        //private IMobileServiceTable<weighTable> weighTableRef;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.LogInWelcome);

            // Create your application here

            Button getStatsButton = FindViewById<Button>(Resource.Id.GetStats);
            //CurrentPlatform.Init();
            //// Create the client instance, using the mobile app backend URL.
            //client = new MobileServiceClient(applicationURL);
            //weighTableRef = client.GetTable<weighTable>();

            getStatsButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(GetStatsActivity));
                //intent.PutExtra("client", client);
                StartActivity(intent);
            };

            //button.Click += delegate {
            //    var activity2 = new Intent(this, typeof(Activity2));
            //    activity2.PutExtra("MyData", "Data from Activity1");
            //    StartActivity(activity2);
            //};



            //insert some usernames and weights:
            //var newWeighItem1 = new weighTable
            //{
            //    username = "Reut",
            //    weigh = 188F
            //};
            //var newWeighItem3 = new weighTable
            //{
            //    username = "Ron",
            //    weigh = 243F
            //};

            //try
            //{
            //    // Insert the new item into the local store.
            //    await weighTableRef.InsertAsync(newWeighItem1);
            //    await weighTableRef.InsertAsync(newWeighItem3);

            //}
            //catch (Exception e)
            //{
            //    CreateAndShowDialog(e, "Error");
            //}

        }


        

    }
}