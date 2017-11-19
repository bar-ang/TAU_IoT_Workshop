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
    [Activity(Label = "Update Height")]
    public class UpdateHeight : Activity
    {
        private IMobileServiceTable<UsersTable> UsersTableRef;
        float enteredHeight = 0;
        string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;
        UsersTable ourUser = null;
        EditText enterHeight;
        TextView heightText;
        Button updateMyHeight;
        Button OKButton;
        Button OKupdateButton;
        Button finishActivity;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            SetContentView(Resource.Layout.heightView);
            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            UsersTableRef = client.GetTable<UsersTable>();
            
            enterHeight = FindViewById<EditText>(Resource.Id.enterHeight);
            enterHeight.Visibility = ViewStates.Gone;
            heightText = FindViewById<TextView>(Resource.Id.heightText);
            //heightText.Visibility = ViewStates.Gone;

            updateMyHeight = FindViewById<Button>(Resource.Id.updateButton);
            updateMyHeight.SetBackgroundColor(Android.Graphics.Color.LightBlue);
            updateMyHeight.Visibility = ViewStates.Gone;

            enterHeight.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) => {

                //https://msdn.microsoft.com/en-us/library/0xh24xh4.aspx
                string value = e.Text.ToString();
                System.Globalization.NumberStyles style;
                System.Globalization.CultureInfo culture;
                float number;
                style = System.Globalization.NumberStyles.Number;
                culture = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB");
                if (Single.TryParse(value, style, culture, out number))
                {
                    Console.WriteLine("Converted '{0}' to {1}.", value, number);
                    string message = "Converted " + value + "to " + number;
                    //CreateAndShowDialog(message, "debug");
                    enteredHeight = number;
                }

                else
                {
                    Console.WriteLine("Unable to convert '{0}'.", value);
                    string message = "Unable to convert " + value;
                    //CreateAndShowDialog(message, "CONVERSION ERROR");
                    enteredHeight = 0;
                }
            };

            updateMyHeight.Click += (sender, e) =>
            {
                heightText.Text = "Please Enter Your Height as a Decimal Number";
                enterHeight.Visibility = ViewStates.Visible;
                updateMyHeight.Visibility = ViewStates.Gone;
                OKupdateButton.Visibility = ViewStates.Visible;
            };


            OKButton = FindViewById<Button>(Resource.Id.OKButton);
            OKButton.Visibility = ViewStates.Gone;
            OKButton.Click += async (sender, e) =>
            {
                
                if (enteredHeight > 0)
                {
                    OKButton.Visibility = ViewStates.Gone;
                    heightText.Visibility = ViewStates.Gone;
                    enterHeight.Visibility = ViewStates.Gone;
                    //enter user's info to UsersTable
                    var userUpdated = new UsersTable
                    {
                        UniqueUsername = ourUserId,
                        height = enteredHeight,
                    };
                    //TODO:  need try-catch here ?
                    await UsersTableRef.InsertAsync(userUpdated);
                    heightText.Text = "Height Inserted to Database Successfully";
                    heightText.Visibility = ViewStates.Visible;
                    finishActivity.Visibility = ViewStates.Visible;

                }
                else
                {
                    CreateAndShowDialog("Please insert a decimal number larger than 0", "Input Error");
                }
            };

            OKupdateButton = FindViewById<Button>(Resource.Id.OKUpdateCaseButton);
            OKupdateButton.Visibility = ViewStates.Gone;
            OKupdateButton.Click += async (sender, e) =>
            {
                if (enteredHeight > 0 && ourUser != null)
                {
                    //TODO:  need try-catch here ?
                    OKupdateButton.Visibility = ViewStates.Gone;
                    ourUser.height = enteredHeight;
                    heightText.Visibility = ViewStates.Gone;
                    enterHeight.Visibility = ViewStates.Gone;
                    await UsersTableRef.UpdateAsync(ourUser);
                    //CreateAndShowDialogAdvanced("", "Updated Successfully ?  Try-Catch");
                    //Finish();
                    heightText.Text = "Height Updated Successfully";
                    heightText.Visibility = ViewStates.Visible;
                    finishActivity.Visibility = ViewStates.Visible;
                }
                else
                {
                    CreateAndShowDialog("Please insert a decimal number larger than 0", "Input Error");
                }
            };

            finishActivity = FindViewById<Button>(Resource.Id.FinishActivity);
            finishActivity.Visibility = ViewStates.Gone;
            finishActivity.Click += (sender, e) =>
            {
                Finish();
            };


            //TODO:  should this be surrounded with try catch ?
            ourUser = await fetchHeightAsync();

        }

        private async Task<UsersTable> fetchHeightAsync()
        {
            try
            {
                var userRecord = await UsersTableRef.Where(item => (item.UniqueUsername == ourUserId)).ToListAsync();
                if (userRecord.Count == 0)
                {
                    heightText.Text = "Please Enter Your Height as a Decimal Number:";
                    enterHeight.Visibility = ViewStates.Visible;
                    OKButton.Visibility = ViewStates.Visible;
                    return null;
                }
                else
                {
                    var user = userRecord[0];
                    float height = user.height;
                    string heightString = Convert.ToString(height);
                    heightText.Text = "Your listed height is: " + heightString;
                    updateMyHeight.Visibility = ViewStates.Visible;
                    return user;
                }
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
                return null;
            }
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

        void CreateAndShowDialogAdvanced(string message, string title)
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