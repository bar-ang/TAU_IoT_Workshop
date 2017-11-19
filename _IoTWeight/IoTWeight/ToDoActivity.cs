/*
 * To add Offline Sync Support:
 *  1) Add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore (and dependencies) to all client projects
 *  2) Uncomment the #define OFFLINE_SYNC_ENABLED
 *
 * For more information, see: http://go.microsoft.com/fwlink/?LinkId=717898
 */
//#define OFFLINE_SYNC_ENABLED

using System;
using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using IoTWeight;
using Android.Content;
using Java.Util;
//using Java.Net;
using Android.Webkit;
using Newtonsoft.Json.Linq;


namespace IoTWeight
{
    [Activity(MainLauncher = true,
               Icon = "@drawable/ic_launcher", Label = "@string/app_name",
               Theme = "@style/AppTheme")]

    public class ToDoActivity : Activity
    {
        // Client reference.
        public static MobileServiceClient client;
        public static string userid = "";
        const string applicationURL = @"https://iotweight.azurewebsites.net";
        // Create a new instance field for this activity.
        static ToDoActivity instance = new ToDoActivity();

        // Return the current activity instance.
        public static ToDoActivity CurrentActivity
        {
            get
            {
                return instance;
            }
        }
        // Return the Mobile Services client.
        public MobileServiceClient CurrentClient
        {
            get
            {
                return client;
            }
        }

        public string Currentuserid
        {
            get
            {
                return userid;
            }
            set
            {
                userid = value;
            }
        }


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            //SetContentView(Resource.Layout.Activity_To_Do);
            SetContentView(Resource.Layout.Main);
            CurrentPlatform.Init();
            client = new MobileServiceClient(applicationURL);
            // Set the current instance of TodoActivity.
            instance = this;
        }




        private MobileServiceUser user;
        private async Task<string> Authenticate(bool forcePassword = false)
        {
            string myName = "failed";
            try
            {

                if (forcePassword)
                    CookieManager.Instance.RemoveAllCookie();

                user = await client.LoginAsync(this,
                    MobileServiceAuthenticationProvider.Facebook);
                //CreateAndShowDialog(string.Format("you are now logged in - {0}",
                //user.UserId), "Logged in!");
                Console.WriteLine("user's sid = {0}", user.UserId);
                Currentuserid = user.UserId;

                //get username from Facebook:
                var response = await client.InvokeApiAsync<JToken>("getExtraDetails", HttpMethod.Get, null);
                Console.WriteLine("JToken response = {0}" , response);
                //string myName = "Logged in successfully but cannot get user name from Facebook Graph API";
                if (response != null)
                {
                    if (response["facebook"] != null)
                    {
                        JToken userId = response["facebook"]["claims"];
                        if (userId != null)
                        {
                            JToken name = response["facebook"]["claims"]["name"];
                            //JToken name = response["facebook"]["claims"]["wow"];
                            if (name != null)
                            {
                                if ((string)name != null)
                                    myName = (string)name;
                            }
                        }

                    }
                }

                //response = null;
                //JToken abc = response["bla"]["claims"]["woohoo"];
                //myName = (string)abc;
                Console.WriteLine("myName = " + myName);

            }
            catch (Exception ex)
            {
                CreateAndShowDialog(ex, "Authentication failed");
            }
            return myName;
        }


        [Java.Interop.Export()]
        public async void LoginUser(View view)
        {

            Button logInButton = FindViewById<Button>(Resource.Id.buttonLoginUser);
            logInButton.Visibility = ViewStates.Gone;
            Button logInAsDifferentUserButton = FindViewById<Button>(Resource.Id.buttonLoginDiffrentUser);
            logInAsDifferentUserButton.Visibility = ViewStates.Gone;

            string userName = await Authenticate();
            if (userName != "failed")
            {
                var intent = new Intent(this, typeof(LogInActivity));
                intent.PutExtra("userName", userName);
                StartActivity(intent);
            }
            else
            {
                CreateAndShowDialog("Unable to authenticate.", "Sorry");
            }
        }


        [Java.Interop.Export()]
        public async void LoginUserAsDifferentUser(View view)
        {

            Button logInButton = FindViewById<Button>(Resource.Id.buttonLoginUser);
            logInButton.Visibility = ViewStates.Gone;
            Button logInAsDifferentUserButton = FindViewById<Button>(Resource.Id.buttonLoginDiffrentUser);
            logInAsDifferentUserButton.Visibility = ViewStates.Gone;

            string userName = await Authenticate(true);
            if (userName != "failed")
            {
                var intent = new Intent(this, typeof(LogInActivity));
                intent.PutExtra("userName", userName);
                StartActivity(intent);
            }
            else
            {
                CreateAndShowDialog("Unable to authenticate.", "Sorry");
            }
        }




        ////Below are the 2 methods Authenticate and LoginUser without username that worked
        //// Define a authenticated user.
        //private MobileServiceUser user;
        //private async Task<bool> Authenticate()
        //{
        //    var success = false;
        //    try
        //    {

        //        user = await client.LoginAsync(this,
        //            MobileServiceAuthenticationProvider.Facebook);
        //        CreateAndShowDialog(string.Format("you are now logged in - {0}",
        //            user.UserId), "Logged in!");
        //        Currentuserid = user.UserId;

        //        success = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        CreateAndShowDialog(ex, "Authentication failed");
        //    }
        //    return success;
        //}

        //[Java.Interop.Export()]
        //public async void LoginUser(View view)
        //{
        //    // Load data only after authentication succeeds.
        //    if (await Authenticate())
        //    {
        //        //Hide the button after authentication succeeds.
        //        FindViewById<Button>(Resource.Id.buttonLoginUser).Visibility = ViewStates.Gone;

        //        var intent = new Intent(this, typeof(LogInActivity));
        //        //intent.PutExtra("client", client);
        //        StartActivity(intent);
        //    }
        //}


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
    }

}