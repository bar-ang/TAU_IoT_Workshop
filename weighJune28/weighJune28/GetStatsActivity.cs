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
//    public class GetStatsActivity : Activity
//    {
//        List<float> lastWeights = new List<float>();
//        List<long> ourUserMacAddresses = new List<long>();
//        List<WeighDatePair> weighDateList = new List<WeighDatePair>();

//        string timePeriod = "";
//        string displayFormat = "";
//        int timePeriodDesired = 0;

//        private IMobileServiceTable<weighTable> weighTableRef;
//        private IMobileServiceTable<UsersTable> UsersTableRef;

//        protected override async void OnCreate(Bundle savedInstanceState)
//        {
//            base.OnCreate(savedInstanceState);
//            var passParameters = Intent.Extras.GetStringArrayList("passParameters") ?? new string[0];
//            timePeriod = passParameters.ElementAt(0);
//            timePeriodDesired = extractTimePeriod();
//            displayFormat = passParameters.ElementAt(1);
//            PlotView view = null;
//            PlotModel plotModel = null;
//            LineSeries series1 = null;
//            if (displayFormat == "Graph")
//            {
//                SetContentView(Resource.Layout.Graphs);
//                view = FindViewById<PlotView>(Resource.Id.plot_view);
//                var startDate = DateTime.Now.AddDays(timePeriodDesired);
//                var endDate = DateTime.Now;

//                var minValue = DateTimeAxis.ToDouble(startDate);
//                var maxValue = DateTimeAxis.ToDouble(endDate);

//                plotModel = new PlotModel { Title = "Weigh History" };
//                //TODO:    different views in case month, 3 months, 6 months - DateTime axis IntervalLength is different
//                plotModel.Axes.Add(new DateTimeAxis
//                {
//                    Position = AxisPosition.Bottom,
//                    Minimum = minValue,
//                    Maximum = maxValue,
//                    StringFormat = "M/d",
//                    //MinorIntervalType = DateTimeIntervalType.Days,
//                    IntervalType = DateTimeIntervalType.Days,
//                    IntervalLength = 30,
//                    Title = "Day of Weighing",
//                });
//                 series1 = new LineSeries
//                {
//                    MarkerType = MarkerType.Circle,
//                    MarkerSize = 4,
//                    MarkerStroke = OxyColors.White
//                };
//            }

//            else
//            {
//                SetContentView(Resource.Layout.multipleColumnsLayout);
//            }


//            //GET MAC ADDRESS
//            long macAdd = getMacAddress();
//            //Console.WriteLine("MAC address is: {0}", macAdd);
//            //"02:00:00:00:00:00" is default
//            if (macAdd == 2199023255552)
//            {
//                CreateAndShowDialog("Please exit the application and try again", "Error in Identifying the User");
//            }
//            string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;
//            //Console.WriteLine("sid is: {0}", ourUserId);

//            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
//            weighTableRef = client.GetTable<weighTable>();
//            UsersTableRef = client.GetTable<UsersTable>();

//            try
//            {
//                DateTime today = DateTime.Now;
//                //DateTime lastMonth = today.AddDays(-30);
//                DateTime earliestDate = today.AddDays(timePeriodDesired);

//                //some inserts for debugging:
//                //macAdd = A0:82:1F:6A:40:5B
//                //var newweightablerecord = new weighTable
//                //{
//                //    MACaddress = macAdd,
//                //    weigh = 91f,
//                //    //createdat = today.adddays(-9)
//                //};
//                //var newweightablerecord1 = new weighTable
//                //{
//                //    MACaddress = macAdd,
//                //    weigh = 90f,

//                //};
//                //var newweightablerecord2 = new weighTable
//                //{
//                //    MACaddress = macAdd,
//                //    weigh = 90.4f,

//                //};
//                //var newweightablerecord3 = new weighTable
//                //{
//                //    MACaddress = macAdd,
//                //    weigh = 89.8f,

//                //};
//                //var newweightablerecord4 = new weighTable
//                //{
//                //    MACaddress = macAdd,
//                //    weigh = 89.2f,

//                //};
//                //var newweightablerecord5 = new weighTable
//                //{
//                //    MACaddress = macAdd,
//                //    weigh = 88.3f,

//                //};
//                //var newweightablerecord6 = new weighTable
//                //{
//                //    MACaddress = macAdd,
//                //    weigh = 87.5f,

//                //};
//                //var newweightablerecord7 = new weighTable
//                //{
//                //    MACaddress = macAdd,
//                //    weigh = 87.2f,

//                //};
//                //var newweightablerecord8 = new weighTable
//                //{
//                //    MACaddress = macAdd,
//                //    weigh = 86.8f,

//                //};
//                //var newweightablerecord9 = new weighTable
//                //{
//                //    MACaddress = macAdd,
//                //    weigh = 86.2f,

//                //};
//                //var newweightablerecord10 = new weighTable
//                //{
//                //    MACaddress = macAdd,
//                //    weigh = 85.5f,

//                //};
//                //var newweightablerecord11 = new weighTable
//                //{
//                //    MACaddress = macAdd,
//                //    weigh = 84.7f,

//                //};
//                //var UserTableRecord = new UsersTable
//                //{
//                //    MACaddress = macAdd,
//                //    UniqueUsername = ourUserId
//                //};
//                //await weighTableRef.InsertAsync(newweightablerecord);
//                //await weighTableRef.InsertAsync(newweightablerecord1);
//                //await weighTableRef.InsertAsync(newweightablerecord2);
//                //await weighTableRef.InsertAsync(newweightablerecord3);
//                //await weighTableRef.InsertAsync(newweightablerecord4);
//                //await weighTableRef.InsertAsync(newweightablerecord5);
//                //await weighTableRef.InsertAsync(newweightablerecord6);
//                //await weighTableRef.InsertAsync(newweightablerecord7);
//                //await weighTableRef.InsertAsync(newweightablerecord8);
//                //await weighTableRef.InsertAsync(newweightablerecord9);
//                //await weighTableRef.InsertAsync(newweightablerecord10);
//                //await weighTableRef.InsertAsync(newweightablerecord11);

//                //await UsersTableRef.InsertAsync(UserTableRecord);

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
//                var list9 = await weighTableRef.Where(item => (ourUserMacAddresses.Contains(item.MACaddress) ) && (item.createdAt > earliestDate) ).ToListAsync();

//                int i = 0;
//                DateTime debuggDate;
//                string dateSring;
//                foreach (weighTable weight in list9)
//                {
//                    float currW = weight.weigh;
//                    string weighInStringFormat = Convert.ToString(currW);
//                    lastWeights.Add(currW);
//                    //Console.WriteLine("weight = : {0}", currW);
//                    DateTime date1 = weight.createdAt;
//                    debuggDate = date1;
//                    //string dateSring = Convert.ToString(date1);
//                    //Console.WriteLine(date1.ToString());
//                    //dateSring = Convert.ToString(debuggDate);
//                    //if (currW < 93)
//                    //{
//                    //    debuggDate = date1.AddDays(-i);
//                    //    i = i + 10;
//                    //}
//                    //else
//                    //{
//                    //    debuggDate = date1.AddDays(-i);
//                    //    i = i + 5;
//                    //}
//                    dateSring = Convert.ToString(date1);
//                    if (displayFormat == "List")
//                    {
//                        weighDateList.Add(new WeighDatePair
//                        {
//                            myWeight = weighInStringFormat,
//                            dateOfWeigh = dateSring
//                        });
//                    }
//                    else
//                    {
//                        //debug:  don't include 311, 805 results:
//                        if (currW > 300)
//                            continue;
//                        else
//                        {
//                            if (currW < 93)
//                            {
//                                debuggDate = date1.AddDays(-i);
//                                i = i + 10;
//                            }
//                            else
//                            {
//                                debuggDate = date1.AddDays(-i);
//                                i = i + 5;
//                            }
//                            series1.Points.Add(new DataPoint(DateTimeAxis.ToDouble(debuggDate), currW));
//                        }
                        
//                    }
                    
//                }

//                float minWeigh = 0;
//                float maxWeigh = 0;
//                if (displayFormat == "Graph")
//                {
//                    //find the min and max weigh of the Time Period checked:
//                    //for debug:  get rid of 805, 311 results:
//                    if (lastWeights.Contains(805))
//                        lastWeights.Remove(805);
//                    if (lastWeights.Contains(311))
//                        lastWeights.Remove(311);
//                    if (lastWeights.Contains(311))
//                        lastWeights.Remove(311);
//                    minWeigh = lastWeights.Min();
//                    minWeigh = minWeigh - 2;
//                    maxWeigh = lastWeights.Max();
//                    maxWeigh = maxWeigh + 2;
//                }


//                if (list9.Count == 0)
//                {
//                    CreateAndShowDialog("Please Exit the Application , Weigh yourself and try again", "No Previous Weighs Found in the requested time period");
//                }
//                else
//                {
//                    if (displayFormat == "List")
//                    {
//                        var listView = FindViewById<ListView>(Resource.Id.listView);
//                        listView.Adapter = new WeighDatePairAdapter(this, weighDateList);
//                    }
//                    else
//                    {
//                        plotModel.Axes.Add(new LinearAxis
//                        {
//                            Position = AxisPosition.Left,
//                            Title = "Weigh",
//                            Minimum = minWeigh,
//                            Maximum = maxWeigh
//                        });
//                        plotModel.Series.Add(series1);
//                        view.Model = plotModel;
//                    }
//                }


//                //DateTime date2 = new DateTime(2017, 6, 28, 16, 5, 0);
//            }
//            catch (Exception e)
//            {
//                CreateAndShowDialog(e, "Error");
//            }

//            //this.ListAdapter = new ArrayAdapter<float>(this, Android.Resource.Layout.SimpleListItem1, lastWeights);
//        }


//        protected int extractTimePeriod()
//        {
//            if (timePeriod == "LastMonth")
//                return -30;
//            else if (timePeriod == "Last3Months")
//                return -90;
//            else
//                return -180;
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

//    }
//}