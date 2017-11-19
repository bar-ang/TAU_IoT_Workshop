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

namespace IoTWeight
{
    [Activity(Label = "UpdateProfile")]
    public class UpdateProfile : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.updateProfile);

            // Create your application here
            string userName = Intent.GetStringExtra("userName") ?? "Username not available";
            FindViewById<TextView>(Resource.Id.welcomeText).Text = "Hello " + userName + "!";
            Button deleteButton = FindViewById<Button>(Resource.Id.DeleteWeighs);

            deleteButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(PickDateToDeleteVer2));
                StartActivity(intent);
            };

            
            Button heightButton = FindViewById<Button>(Resource.Id.updateHeight);
   
            heightButton.Click += (sender, e) =>
            {
                var intent = new Intent(this, typeof(UpdateHeight));
                StartActivity(intent);
            };
        }
    }
}