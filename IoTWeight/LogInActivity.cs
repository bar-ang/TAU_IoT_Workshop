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

namespace IoTWeight
{
    [Activity(Label = "What would you like to do ?")]
    public class LogInActivity : Activity
    {
        
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.LogInWelcome);

            // Create your application here
            Button getStatsButton = FindViewById<Button>(Resource.Id.GetStats);
            getStatsButton.SetBackgroundColor(Android.Graphics.Color.SteelBlue);
            Button startWeighButton = FindViewById<Button>(Resource.Id.StartWeigh);
            startWeighButton.SetBackgroundColor(Android.Graphics.Color.LightSkyBlue);
            Button BMIButton = FindViewById<Button>(Resource.Id.BMI);
            BMIButton.SetBackgroundColor(Android.Graphics.Color.LightBlue);

            getStatsButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(GetStatsChooseDisplay));
                StartActivity(intent);
            };

            startWeighButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(QRScan));
                //var intent = new Intent(this, typeof(GetIPAddress));
                StartActivity(intent);
            };

            BMIButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(CalculateBMI));
                StartActivity(intent);
            };


            //button.Click += delegate {
            //    var activity2 = new Intent(this, typeof(Activity2));
            //    activity2.PutExtra("MyData", "Data from Activity1");
            //    StartActivity(activity2);
            //};





        }




    }
}