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
using Android.Webkit;

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

            string userName = Intent.GetStringExtra("userName") ?? "Username not available";
            FindViewById<TextView>(Resource.Id.welcomeText).Text = "Hello " + userName + "!";

            Button getStatsButton = FindViewById<Button>(Resource.Id.GetStats);
            getStatsButton.SetBackgroundColor(Android.Graphics.Color.SteelBlue);
            Button startWeighButton = FindViewById<Button>(Resource.Id.StartWeigh);
            startWeighButton.SetBackgroundColor(Android.Graphics.Color.LightSkyBlue);
            Button BMIButton = FindViewById<Button>(Resource.Id.BMI);
            BMIButton.SetBackgroundColor(Android.Graphics.Color.LightBlue);
            Button updateButton = FindViewById<Button>(Resource.Id.update);

            Button LogoutButton = FindViewById<Button>(Resource.Id.ButtonLogout);
            LogoutButton.Visibility = ViewStates.Gone;

            Button debuggButton = FindViewById<Button>(Resource.Id.debuggButton);
            debuggButton.Visibility = ViewStates.Gone;

            debuggButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(InsertWeightsForDebugg));
                StartActivity(intent);
            };

            
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

           
            updateButton.Click += delegate
            {
                var activity2 = new Intent(this, typeof(UpdateProfile));
                activity2.PutExtra("userName", userName);
                StartActivity(activity2);
            };

            LogoutButton.Click += async (sender, e) =>
            {
                MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
                MobileServiceUser user = new MobileServiceUser(ToDoActivity.CurrentActivity.Currentuserid);
                try
                {
                    if (user != null)
                    {
                        CookieManager.Instance.RemoveAllCookie();
                        await client.LogoutAsync();
                        //CreateAndShowDialog(string.Format("You are now logged out - {0}", user.UserId), "Logged out!");
                    }
                    user = null;
                    
                }
                catch (Exception ex)
                {
                    CreateAndShowDialog(ex.Message, "Logout failed");
                }

                var intent = new Intent(this, typeof(ToDoActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            };

        }
        void CreateAndShowDialog(string message, string title)
        {
            var builder = new AlertDialog.Builder(this);
            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.SetNeutralButton("OK", (sender, args) => {
            });
            builder.Create().Show();
        }
    }
}