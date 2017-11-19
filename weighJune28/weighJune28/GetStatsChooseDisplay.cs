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

namespace weighJune28
{
    [Activity(Label = "Display Format")]
    public class GetStatsChooseDisplay : Activity
    {
        List<string> passParameters = new List<string>();
        int isTimePeriodSelected = 0;
        int isDisplayFormatSelected = 0;
        string timePeriod = "not chosen";
        string displayFormat = "not chosen";

        RadioGroup rgTimePeriod = null;
        RadioGroup rgDisplayFormat = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //SetContentView(Resource.Layout.ChooseDisplay);    
        }


        //protected override void OnStop()
        //{
        //    base.OnStop();
        //    if (rgTimePeriod != null)
        //        rgTimePeriod.ClearCheck();
        //    if (rgDisplayFormat != null)
        //        rgDisplayFormat.ClearCheck();
        //}

        protected override void OnStart()
        {
            //Log.Debug("OnStart", "OnStart called, App is Active");
            base.OnStart();
            SetContentView(Resource.Layout.ChooseDisplay);

            // Create your application here
            RadioButton radioButtonLastMonth = FindViewById<RadioButton>(Resource.Id.radioButtonLastMonth);
            RadioButton radioButtonLast3Months = FindViewById<RadioButton>(Resource.Id.radioButtonLast3Months);
            RadioButton radioButtonLast6Months = FindViewById<RadioButton>(Resource.Id.radioButtonLast6Months);

            RadioButton radioButtonGraph = FindViewById<RadioButton>(Resource.Id.radioButtonGraph);
            RadioButton radioButtonList = FindViewById<RadioButton>(Resource.Id.radioButtonList);

            rgTimePeriod = FindViewById<RadioGroup>(Resource.Id.radioGroupTimePeriod);
            rgTimePeriod.ClearCheck();
            rgTimePeriod.CheckedChange += OnCheckedChange;
            rgDisplayFormat = FindViewById<RadioGroup>(Resource.Id.radioGroupDisplayFormat);
            rgDisplayFormat.ClearCheck();
            rgDisplayFormat.CheckedChange += OnCheckedChange;
            //string message = "time Period = " + timePeriod + ",   DisplayFormat = " + displayFormat;
            //Console.WriteLine(message);
            isTimePeriodSelected = 0;
            isDisplayFormatSelected = 0;
            timePeriod = "not chosen";
            displayFormat = "not chosen";
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
                case Resource.Id.radioButtonGraph:
                    displayFormat = "Graph";
                    isDisplayFormatSelected = 1;
                    break;
                case Resource.Id.radioButtonList:
                    displayFormat = "List";
                    isDisplayFormatSelected = 1;
                    break;
            }

            if(isTimePeriodSelected + isDisplayFormatSelected == 2)
            {
                //start next activity
                passParameters.Insert(0, timePeriod);
                passParameters.Insert(1, displayFormat);
                isTimePeriodSelected = 0;
                isDisplayFormatSelected = 0;

                //rgTimePeriod = FindViewById<RadioGroup>(Resource.Id.radioGroupTimePeriod);
                //rgDisplayFormat = FindViewById<RadioGroup>(Resource.Id.radioGroupDisplayFormat);
                //if (rgTimePeriod != null)
                //    rgTimePeriod.ClearCheck();
                //if (rgDisplayFormat != null)
                //    rgDisplayFormat.ClearCheck();

                //var intent = new Intent(this, typeof(GetStatsActivity));
                var intent = new Intent(this, typeof(WeightHistory));
                intent.PutStringArrayListExtra("passParameters", passParameters);
                StartActivity(intent);

                //string message = "time Period = " + timePeriod + ",   DisplayFormat = " + displayFormat;
                //CreateAndShowDialog(message, "debugging title");


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
    }
}