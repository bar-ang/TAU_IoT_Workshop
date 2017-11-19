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
using System.Threading.Tasks;
using Java.Util;
using Java.Net;

namespace IoTWeight
{
    [Activity(Label = "Calculate your BMI")]
    public class CalculateBMI : Activity
    {
        private IMobileServiceTable<UsersTable> UsersTableRef;
        private IMobileServiceTable<weighTable> weighTableRef;
        float enteredHeight = 0;
        string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;
       
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your application here
            SetContentView(Resource.Layout.BMIview);
            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            weighTableRef = client.GetTable<weighTable>();
            UsersTableRef = client.GetTable<UsersTable>();

   
            var enterHeight = FindViewById<EditText>(Resource.Id.enterHeight);
            var heightText = FindViewById<TextView>(Resource.Id.heightText);
            var BMI_text = FindViewById<TextView>(Resource.Id.BMI_TextView);
            BMI_text.Visibility = ViewStates.Gone;
            FindViewById<TextView>(Resource.Id.Category_TextView).Visibility = ViewStates.Gone;
  
            heightText.Visibility = ViewStates.Gone;
            enterHeight.Visibility = ViewStates.Gone;
            var wikiLinkButton = FindViewById<Button>(Resource.Id.BMIwikiLink);
            wikiLinkButton.Visibility = ViewStates.Gone;
           
            wikiLinkButton.Click += delegate {
                var uri = Android.Net.Uri.Parse("https://en.wikipedia.org/wiki/Body_mass_index#Categories");
                var intent = new Intent(Intent.ActionView, uri);
                StartActivity(intent);
            };
           

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
                    enteredHeight = number;
                }

                else
                {
                    Console.WriteLine("Unable to convert '{0}'.", value);
                    string message = "Unable to convert " + value;
                    enteredHeight = 0;
                }
            };

            var OKButton = FindViewById<Button>(Resource.Id.OKButton);
            OKButton.Visibility = ViewStates.Gone;
            OKButton.Click += async (sender, e) =>
            {
                if (enteredHeight > 0)
                {
                    //enter user's info to UsersTable and then calculate BMI
                    var userUpdated = new UsersTable
                    {
                        UniqueUsername = ourUserId,
                        height = enteredHeight,
                    };
                    try
                    {
                        await UsersTableRef.InsertAsync(userUpdated);
                        OKButton.Visibility = ViewStates.Gone;
                        heightText.Visibility = ViewStates.Gone;
                        enterHeight.Visibility = ViewStates.Gone;
                        await CalculateUserBMI();
                    }
                    catch (Exception ex)
                    {
                        CreateAndShowDialog(ex, "Error");
                    }
                }
                else
                {
                    CreateAndShowDialog("Please insert a decimal number larger than 0", "Input Error");
                }
            };



            try
            {
                var userRecord = await UsersTableRef.Where(item => (item.UniqueUsername == ourUserId)).ToListAsync();
                if (userRecord.Count == 0)
                {
                    heightText.Visibility = ViewStates.Visible;
                    enterHeight.Visibility = ViewStates.Visible;
                    OKButton.Visibility = ViewStates.Visible;
                }
                //user already entered height:
                else
                {
                   
                    var user = userRecord[0];
                    float height = user.height;
                    //check if there are weighs of this user in the weighTable:
                    var weighRecords = await weighTableRef.Where(item => (item.username == ourUserId)).ToListAsync();
                    if (weighRecords.Count == 0)
                    {
                        CreateAndShowDialog( "No previous weighs were found. Please weigh yourself and try again", "Cannot calculate BMI");
                    }
                    else
                    {
                        //calculate BMI from user's most recent weighing:
                        var mostRecendWeigh = weighRecords[weighRecords.Count - 1];
                        float weight = mostRecendWeigh.weigh;
                        //BMI formula from wikipedia:   BMI = weight in kg / (height in meters)^2
                        //https://en.wikipedia.org/wiki/Body_mass_index
                        double heigtSquared = Math.Pow(height, 2);
                        double BMI = weight / heigtSquared;
                        string BMIInStringFormat = String.Format("{0:0.00}", BMI);
                        string BMI_message = "Your most recent weight = " + weight + "\nYour BMI = " + BMIInStringFormat;
                        var BMItext = FindViewById<TextView>(Resource.Id.BMI_TextView);
                        BMItext.Visibility = ViewStates.Visible;
                        BMItext.Text = BMI_message;
                        CalculateBMICategory(BMI);
                    }
                }

            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
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

       
        private async Task CalculateUserBMI()
        {
            try
            {
                var weighRecords = await weighTableRef.Where(item => (item.username == ourUserId)).ToListAsync();
                if (weighRecords.Count == 0)
                {
                    CreateAndShowDialog("Cannot calculate BMI", "No previous weighs were found. Please weigh yourself and try again");
                    
                }
                else
                {
                    //calculate BMI from user's most recent weighing:
                    var mostRecendWeigh = weighRecords[weighRecords.Count - 1];
                    float weight = mostRecendWeigh.weigh;
                    //BMI formula from wikipedia:   BMI = weight in kg / (height in meters)^2
                    //https://en.wikipedia.org/wiki/Body_mass_index
                    double heigtSquared = Math.Pow(enteredHeight, 2);
                    double BMI = weight / heigtSquared;
                    string BMIInStringFormat = String.Format("{0:0.00}", BMI);
                    string BMI_message = "Your most recent weight = " + weight + "\nYour BMI = " + BMIInStringFormat;
                    var BMItext = FindViewById<TextView>(Resource.Id.BMI_TextView);
                    BMItext.Visibility = ViewStates.Visible;
                    BMItext.Text = BMI_message;
                    CalculateBMICategory(BMI);   

                }
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }


        private void CalculateBMICategory(double BMI)
        {
            string BMI_Category = "";
            if (BMI < 15)
            {
                BMI_Category = "Very Severely Underweight";
            }
            else if (BMI <= 16)
            {
                BMI_Category = "Severely Underweight";
            }
            else if (BMI <= 18.5)
            {
                BMI_Category = "Underweight";

            }
            else if (BMI <= 25)
            {
                BMI_Category = "Normal";

            }
            else if (BMI <= 30)
            {
                BMI_Category = "Overweight";

            }
            else if (BMI <= 35)
            {
                BMI_Category = "Moderately Obese";

            }
            else if (BMI <= 40)
            {
                BMI_Category = "Severely Obese";
            }

            else
            {
                BMI_Category = "Very Severely Obese";

            }
           
            var categoryView = FindViewById<TextView>(Resource.Id.Category_TextView);
            categoryView.Visibility = ViewStates.Visible;
            if (BMI_Category == "Normal")
                categoryView.Text = "Your BMI Category is:  " + BMI_Category;
            else
                categoryView.Text = "Your BMI Category is:  " + BMI_Category + "\nNormal category is 18.5 to 25";
            FindViewById<Button>(Resource.Id.BMIwikiLink).Visibility = ViewStates.Visible;

            
        }
    }
}