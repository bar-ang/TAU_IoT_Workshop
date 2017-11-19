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

using OxyPlot.Xamarin.Android;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace IoTWeight
{
    [Activity(Label = "Weight History")]
    public class WeightHistory : Activity
    {
        List<float> lastWeights = new List<float>();
        List<WeighDatePair> weighDateList = new List<WeighDatePair>();

        string timePeriod = "";
        string displayFormat = "";
        int timePeriodDesired = 0;

        private IMobileServiceTable<weighTable> weighTableRef;
        private IMobileServiceTable<UsersTable> UsersTableRef;

        //[EnableQuery(PageSize = 1000)]
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var passParameters = Intent.Extras.GetStringArrayList("passParameters") ?? new string[0];
            timePeriod = passParameters.ElementAt(0);
            timePeriodDesired = extractTimePeriod();
            displayFormat = passParameters.ElementAt(1);
            PlotView view = null;
            PlotModel plotModel = null;
            LineSeries series1 = null;
            if (displayFormat == "Graph")
            {
                SetContentView(Resource.Layout.Graphs);
                view = FindViewById<PlotView>(Resource.Id.plot_view);
                var startDate = DateTime.Now.AddDays(timePeriodDesired);
                var endDate = DateTime.Now;

                var minValue = DateTimeAxis.ToDouble(startDate);
                var maxValue = DateTimeAxis.ToDouble(endDate);

                plotModel = new PlotModel { Title = "Weigh History" };
                plotModel.Axes.Add(new DateTimeAxis
                {
                    Position = AxisPosition.Bottom,
                    Minimum = minValue,
                    Maximum = maxValue,
                    StringFormat = "M/d",
                    //MinorIntervalType = DateTimeIntervalType.Days,
                    IntervalType = DateTimeIntervalType.Days,
                    IntervalLength = 30,
                    Title = "Day of Weighing",
                });
                series1 = new LineSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 4,
                    MarkerStroke = OxyColors.White
                };
            }

            else
            {
                SetContentView(Resource.Layout.multipleColumnsLayout);
            }


            string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;
            //Console.WriteLine("sid is: {0}", ourUserId);
            //Bar's sid:    sid:f5ccac253e6e9ce70bd96a3b9a0b59d2
            //string BarSID = "sid:f5ccac253e6e9ce70bd96a3b9a0b59d2";
            //ourUserId = BarSID;


            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            weighTableRef = client.GetTable<weighTable>();
            UsersTableRef = client.GetTable<UsersTable>();

            try
            {
                DateTime today = DateTime.Now;
                DateTime earliestDate = today.AddDays(timePeriodDesired);

                //weighTable :      username               weigh       createdAt
                //UsersTable:       UniqueUsername         height       gender      age    


                //var list9 = await weighTableRef.Where(item => (item.username == ourUserId) && (item.createdAt >= earliestDate)).ToListAsync();
                var list9 = await weighTableRef.Take(500).Where(item => (item.username == ourUserId) && (item.createdAt >= earliestDate)).ToListAsync();
                

                if (list9.Count == 0)
                {
                    CreateAndShowDialog("Please Weigh yourself and try again", "No Previous Weighs Found in the requested time period");
                }

                int i = 0;
                DateTime debuggDate;
                string dateSring;
                foreach (weighTable weight in list9)
                {
                    float currW = weight.weigh;
                    string weighInStringFormat = Convert.ToString(currW);
                    lastWeights.Add(currW);
                    //Console.WriteLine("weight = : {0}", currW);
                    DateTime date1 = weight.createdAt;
                    debuggDate = date1;
                    //string dateSring = Convert.ToString(date1);
                    //Console.WriteLine(date1.ToString());
                    //dateSring = Convert.ToString(debuggDate);
                    
                    dateSring = Convert.ToString(date1);
                    if (displayFormat == "List")
                    {
                        weighDateList.Add(new WeighDatePair
                        {
                            myWeight = weighInStringFormat,
                            dateOfWeigh = dateSring
                        });
                    }
                    else
                    {

                            //shift dates to debugg graph
                            debuggDate = date1.AddDays(-i);
                            i = i + 1;
                            series1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(debuggDate), currW)); 
                    }

                }

                float minWeigh = 0;
                float maxWeigh = 0;
                if (displayFormat == "Graph" && list9.Count > 0)
                {
                    //find the min and max weigh of the Time Period checked:
                    minWeigh = lastWeights.Min();
                    minWeigh = minWeigh - 2;
                    maxWeigh = lastWeights.Max();
                    maxWeigh = maxWeigh + 2;
                }


                if (displayFormat == "List")
                {
                    var listView = FindViewById<ListView>(Resource.Id.listView);
                    listView.Adapter = new WeighDatePairAdapter(this, weighDateList);
                }
                else
                {
                    if (list9.Count > 0)
                    {
                        plotModel.Axes.Add(new LinearAxis
                        {
                            Position = AxisPosition.Left,
                            Title = "Weigh",
                            Minimum = minWeigh,
                            Maximum = maxWeigh
                        });
                        plotModel.Series.Add(series1);
                        view.Model = plotModel;
                    }

                }

                //DateTime date2 = new DateTime(2017, 6, 28, 16, 5, 0);
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }

            //this.ListAdapter = new ArrayAdapter<float>(this, Android.Resource.Layout.SimpleListItem1, lastWeights);
        }

        



        protected int extractTimePeriod()
        {
            if (timePeriod == "LastMonth")
                return -30;
            else if (timePeriod == "Last3Months")
                return -90;
            else if (timePeriod == "Last6Months")
                return -180;
            else
            {
                // TryParse returns true if the conversion succeeded
                // and stores the result in j.
                int j;
                if (Int32.TryParse(timePeriod, out j))
                {
                    Console.WriteLine(j);
                    int sub = (-1) * j;
                    return sub;
                }

                else
                {
                    //should never happen
                    Console.WriteLine("String user input days could not be parsed back to an int.");
                    return 0;
                }
               
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



    }
}