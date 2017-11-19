//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Android.App;
//using Android.Content;
//using Android.OS;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.WindowsAzure.MobileServices;
//using Java.Util;
//using Java.Net;

//using OxyPlot.Xamarin.Android;
//using OxyPlot;
//using OxyPlot.Axes;
//using OxyPlot.Series;


//namespace weighJune28
//{
//    [Activity(Label = "GetStatsActivity")]
//    public class GetGraphs : Activity
//    {
//        List<float> lastWeights = new List<float>();
//        List<long> ourUserMacAddresses = new List<long>();
//        List<WeighDatePair> weighDateList = new List<WeighDatePair>();
        
//        private IMobileServiceTable<weighTable> weighTableRef;
//        private IMobileServiceTable<UsersTable> UsersTableRef;

//        protected override async void OnCreate(Bundle savedInstanceState)
//        {
//            base.OnCreate(savedInstanceState);
//            SetContentView(Resource.Layout.Graphs);

//            PlotView view = FindViewById<PlotView>(Resource.Id.plot_view);
//            var startDate = DateTime.Now.AddDays(-30);
//            var endDate = DateTime.Now;

//            var minValue = DateTimeAxis.ToDouble(startDate);
//            var maxValue = DateTimeAxis.ToDouble(endDate);

//            var plotModel = new PlotModel { Title = "Weigh History" };
//            //_xAxis = new DateTimeAxis
//            //{
//            //    Position = AxisPosition.Bottom,
//            //    StringFormat = Constants.MarketData.DisplayDateFormat,
//            //    Title = "End of Day",
//            //    IntervalLength = 75,
//            //    MinorIntervalType = DateTimeIntervalType.Days,
//            //    IntervalType = DateTimeIntervalType.Days,
//            //    MajorGridlineStyle = LineStyle.Solid,
//            //    MinorGridlineStyle = LineStyle.None,
//            //};
//            //StringFormat = "M/d",
//            plotModel.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom,
//                Minimum = minValue, Maximum = maxValue,
//                StringFormat = "M/d",
//                //MinorIntervalType = DateTimeIntervalType.Days,
//                IntervalType = DateTimeIntervalType.Days,
//                IntervalLength = 30,
//                Title = "Day of Weighing",
//            });

            
//            //plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left,
//            //    Title = "Weigh"
//            //});

//            var series1 = new LineSeries
//            {
//                MarkerType = MarkerType.Circle,
//                MarkerSize = 4,
//                MarkerStroke = OxyColors.White
//            };

            
//            //GET MAC ADDRESS
//            long macAdd = getMacAddress();
//            Console.WriteLine("MAC address is: {0}", macAdd);
//            //"02:00:00:00:00:00" is default
//            if (macAdd == 2199023255552)
//            {
//                CreateAndShowDialog("Please exit the application and try again", "Error in Identifying the User");
//            }
//            string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;
//            Console.WriteLine("sid is: {0}", ourUserId);

//            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
//            weighTableRef = client.GetTable<weighTable>();
//            UsersTableRef = client.GetTable<UsersTable>();

//            try
//            {
//                DateTime today = DateTime.Now;
//                DateTime lastMonth = today.AddDays(-30);

//                //first check if the user exists in the database:
//                var usersList = await UsersTableRef.Where(item => (item.UniqueUsername == ourUserId) && (item.MACaddress == macAdd)).ToListAsync();
//                if (usersList.Count == 0)
//                {
//                    var newUserMacAddTableRecord = new UsersTable
//                    {
//                        UniqueUsername = ourUserId,
//                        MACaddress = macAdd
//                    };
//                    await UsersTableRef.InsertAsync(newUserMacAddTableRecord);
//                }

//                //SQL Table 1:      MACaddress        weigh       createdAt
//                //UsersTable:       UniqueUsername    MACaddress
//                var convertToListOfStrings = await UsersTableRef.Where(item => (item.UniqueUsername == ourUserId)).ToListAsync();
//                foreach (UsersTable userRecord in convertToListOfStrings)
//                {
//                    long m = userRecord.MACaddress;
//                    ourUserMacAddresses.Add(m);
//                }
//                //ourUserMacAddresses.Add(4023);  //TODO:  delete.  for debug purpose only
//                var list9 = await weighTableRef.Where(item => (ourUserMacAddresses.Contains(item.MACaddress)) && (item.createdAt > lastMonth)).ToListAsync();

//                int i = 0;
//                float minWeigh = 0;
//                float maxWeigh = 0; 
//                foreach (weighTable weight in list9)
//                {
//                    float currW = weight.weigh;
//                    string weighInStringFormat = Convert.ToString(currW);
//                    lastWeights.Add(currW);
//                    Console.WriteLine("weight = : {0}", currW);
//                    DateTime date1 = weight.createdAt;
//                    DateTime debuggDate = date1.AddDays(-i);
//                    i=i+2;
//                    series1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(debuggDate), currW));
//                    string dateSring = Convert.ToString(date1);
//                    Console.WriteLine(date1.ToString());
//                    weighDateList.Add(new WeighDatePair
//                    {
//                        myWeight = weighInStringFormat,
//                        dateOfWeigh = dateSring
//                    });
//                }
//                //find the min and max weigh of the Time Period checked:
//                minWeigh = lastWeights.Min();
//                minWeigh = minWeigh - 2;
//                maxWeigh = lastWeights.Max();
//                maxWeigh = maxWeigh + 2;

//                if (list9.Count == 0)
//                {
//                    CreateAndShowDialog("Please Exit the Application and then Weigh yourself and try again", "No Previous Weighs Found");
//                }
//                else
//                {
//                    plotModel.Axes.Add(new LinearAxis
//                    {
//                        Position = AxisPosition.Left,
//                        Title = "Weigh",
//                        Minimum = minWeigh,
//                        Maximum = maxWeigh
//                    });
//                    plotModel.Series.Add(series1);
//                    view.Model = plotModel;
//                }


//            }
//            catch (Exception e)
//            {
//                CreateAndShowDialog(e, "Error");
//            }

//            //this.ListAdapter = new ArrayAdapter<float>(this, Android.Resource.Layout.SimpleListItem1, lastWeights);
//        }



//        private void CreateAndShowDialog(Exception exception, String title)
//        {
//            CreateAndShowDialog(exception.Message, title);
//        }

//        private void CreateAndShowDialog(string message, string title)
//        {
//            AlertDialog.Builder builder = new AlertDialog.Builder(this);

//            builder.SetMessage(message);
//            builder.SetTitle(title);
//            builder.Create().Show();
//        }

//        public static long getMacAddress()
//        {
//            long macadd = 0;
//            //long arcadi = 2;
//            //Console.WriteLine("arcadi = {0}", arcadi << 40);
//            int i = 40;
//            string macAddress = string.Empty;

//            var all = Collections.List(Java.Net.NetworkInterface.NetworkInterfaces);

//            foreach (var interfaces in all)
//            {
//                if (!(interfaces as Java.Net.NetworkInterface).Name.Contains("wlan0")) continue;

//                var macBytes = (interfaces as
//                Java.Net.NetworkInterface).GetHardwareAddress();
//                if (macBytes == null) continue;

//                var sb = new System.Text.StringBuilder();
//                foreach (var b in macBytes)
//                {
//                    string convertedByte = string.Empty;
//                    convertedByte = (b & 0xFF).ToString("X2") + ":";

//                    if (convertedByte.Length == 1)
//                    {
//                        convertedByte.Insert(0, "0");
//                    }
//                    sb.Append(convertedByte);

//                    //Console.WriteLine("byte b = {0}", b);
//                    long shifted = ((long)b) << i;
//                    macadd = macadd + shifted;
//                    i = i - 8;
//                    //Console.WriteLine("macadd = {0}", macadd);
//                }

//                macAddress = sb.ToString().Remove(sb.Length - 1);

//                return macadd;
//            }
//            //return "02:00:00:00:00:00";
//            long def = 2;
//            return (def << 40);
//        }

//        private PlotModel CreatePlotModel()
//        {
//            //var model = new PlotModel { Title = "DateTimeAxis" };

//            var startDate = DateTime.Now.AddDays(-10);
//            var endDate = DateTime.Now;

//            var minValue = DateTimeAxis.ToDouble(startDate);
//            var maxValue = DateTimeAxis.ToDouble(endDate);

//            var plotModel = new PlotModel { Title = "OxyPlot Demo" };
//            plotModel.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, Minimum = minValue, Maximum = maxValue, StringFormat = "M/d" });

//            //plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom });
//            //plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Maximum = 10, Minimum = 0 });
//            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left});

//            var series1 = new LineSeries
//            {
//                MarkerType = MarkerType.Circle,
//                MarkerSize = 4,
//                MarkerStroke = OxyColors.White
//            };

//            //series1.Points.Add(new DataPoint(0.0, 6.0));
//            //series1.Points.Add(new DataPoint(1.4, 2.1));
//            //series1.Points.Add(new DataPoint(2.0, 4.2));
//            //series1.Points.Add(new DataPoint(3.3, 2.3));
//            //series1.Points.Add(new DataPoint(4.7, 7.4));
//            //series1.Points.Add(new DataPoint(6.0, 6.2));
//            //series1.Points.Add(new DataPoint(8.9, 8.9));

//            int j = 90;
//            for (int i = 0; i < 10; i++)
//            {
//                var myDateTime = DateTime.Now.AddDays(-10+i);
//                series1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(myDateTime), j + i));
//            }
//            //mySeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(myDateTime),myValue))

//            plotModel.Series.Add(series1);

//            return plotModel;

//            //return model;
//        }
        

//    }
//}