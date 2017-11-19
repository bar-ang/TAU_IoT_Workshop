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
    [Activity(Label = "Display Format")]
    public class GetStatsChooseDisplay : Activity
    {
        List<string> passParameters = new List<string>();
        int isTimePeriodSelected = 0;
        int isDisplayFormatSelected = 0;
        string timePeriod = "not chosen";
        string displayFormat = "not chosen";
        float inputYear = 0;

        RadioGroup rgTimePeriod = null;
        RadioGroup rgDisplayFormat = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);   
        }


        protected override void OnStart()
        {
            base.OnStart();
            SetContentView(Resource.Layout.ChooseDisplay);

            disalbeEnableAllRadioButtons("enable");

            rgTimePeriod = FindViewById<RadioGroup>(Resource.Id.radioGroupTimePeriod);
            rgTimePeriod.ClearCheck();
            rgTimePeriod.CheckedChange += OnCheckedChange;
            rgDisplayFormat = FindViewById<RadioGroup>(Resource.Id.radioGroupDisplayFormat);
            rgDisplayFormat.ClearCheck();
            rgDisplayFormat.CheckedChange += OnCheckedChange;
          
            isTimePeriodSelected = 0;
            isDisplayFormatSelected = 0;
            timePeriod = "not chosen";
            displayFormat = "not chosen";


            var inputTime = FindViewById<EditText>(Resource.Id.chooseTime);
            inputTime.Visibility = ViewStates.Gone;
            FindViewById<TextView>(Resource.Id.displayUserTime).Visibility = ViewStates.Gone;
            FindViewById<Button>(Resource.Id.OKButton).Visibility = ViewStates.Gone;
            var OKButton = FindViewById<Button>(Resource.Id.OKButton);
            OKButton.Visibility = ViewStates.Gone;

            inputTime.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) => {

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
                    inputYear = number;
                }

                else
                {
                    Console.WriteLine("Unable to convert '{0}'.", value);
                    string message = "Unable to convert " + value;
                    inputYear = 0;
                }
            };

            OKButton.Click += (sender, e) =>
            {
                if (inputYear > 0)
                {
                    int days = calculateNumberOfDays(inputYear);
                    timePeriod = days.ToString();
                    startWeighHistoryActivity();

                }
                else
                {
                    CreateAndShowDialog("Please insert a decimal number larger than 0", "Input Error");
                }
            };


        }

        private void OnCheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            switch (e.CheckedId)
            {
                case Resource.Id.radioButtonLastMonth:
                    timePeriod = "LastMonth";
                    isTimePeriodSelected = 1;
                    break;
                case Resource.Id.radioButtonLast3Months:
                    timePeriod = "Last3Months";
                    isTimePeriodSelected = 1;
                    break;
                case Resource.Id.radioButtonLast6Months:
                    timePeriod = "Last6Months";
                    isTimePeriodSelected = 1;
                    break;
                case Resource.Id.radioButtonOther:
                    timePeriod = "Other";
                    isTimePeriodSelected = 1;
                    break;
                case Resource.Id.radioButtonGraph:
                    displayFormat = "Graph";
                    isDisplayFormatSelected = 1;
                    break;
                case Resource.Id.radioButtonList:
                    displayFormat = "List";
                    isDisplayFormatSelected = 1;
                    break;
            }

            if (isTimePeriodSelected + isDisplayFormatSelected == 2)
            {
                if(timePeriod == "Other")
                {
                    disalbeEnableAllRadioButtons("disable");
                    FindViewById<EditText>(Resource.Id.chooseTime).Visibility = ViewStates.Visible;
                    FindViewById<TextView>(Resource.Id.displayUserTime).Visibility = ViewStates.Visible;
                    FindViewById<Button>(Resource.Id.OKButton).Visibility = ViewStates.Visible;
                    return;
                }
                
                startWeighHistoryActivity();
            }
        }

        private void RadioButtonClick(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            Toast.MakeText(this, rb.Text, ToastLength.Short).Show();
        }

        private void CreateAndShowDialog(string message, string title)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }

        private void disalbeEnableAllRadioButtons(string action)
        {
            if(action == "disable")
            {
                FindViewById<RadioGroup>(Resource.Id.radioGroupTimePeriod).Enabled = false;
                FindViewById<RadioGroup>(Resource.Id.radioGroupDisplayFormat).Enabled = false;
                FindViewById<RadioButton>(Resource.Id.radioButtonLastMonth).Enabled = false;
                FindViewById<RadioButton>(Resource.Id.radioButtonLast3Months).Enabled = false;
                FindViewById<RadioButton>(Resource.Id.radioButtonLast6Months).Enabled = false;
                FindViewById<RadioButton>(Resource.Id.radioButtonOther).Enabled = false;
                FindViewById<RadioButton>(Resource.Id.radioButtonGraph).Enabled = false;
                FindViewById<RadioButton>(Resource.Id.radioButtonList).Enabled = false;
            }
            else
            {
                FindViewById<RadioGroup>(Resource.Id.radioGroupTimePeriod).Enabled = true;
                FindViewById<RadioGroup>(Resource.Id.radioGroupDisplayFormat).Enabled = true;
                FindViewById<RadioButton>(Resource.Id.radioButtonLastMonth).Enabled = true;
                FindViewById<RadioButton>(Resource.Id.radioButtonLast3Months).Enabled = true;
                FindViewById<RadioButton>(Resource.Id.radioButtonLast6Months).Enabled = true;
                FindViewById<RadioButton>(Resource.Id.radioButtonOther).Enabled = true;
                FindViewById<RadioButton>(Resource.Id.radioButtonGraph).Enabled = true;
                FindViewById<RadioButton>(Resource.Id.radioButtonList).Enabled = true;
            }
        }

        private int calculateNumberOfDays(float inputYear)
        {
            float daysAsFloat = inputYear * 365;
            int daysRounded = (int)Math.Round(daysAsFloat, 0);
            return daysRounded;
        }

        private void startWeighHistoryActivity()
        {
            //start next activity
            passParameters.Insert(0, timePeriod);
            passParameters.Insert(1, displayFormat);
            isTimePeriodSelected = 0;
            isDisplayFormatSelected = 0;

            var intent = new Intent(this, typeof(WeightHistory));
            intent.PutStringArrayListExtra("passParameters", passParameters);
            StartActivity(intent);
        }
    }
}