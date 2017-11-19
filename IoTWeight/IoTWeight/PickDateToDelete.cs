using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace IoTWeight
{
    [Activity(Label = "PickDateToDelete")]
    public class PickDateToDelete : Activity
    {
        TextView _dateDisplay;
        Button _dateSelectButton;
        Button SingleDayButton;
        Button DeleteAllButton;
        private IMobileServiceTable<weighTable> weighTableRef;
        string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;
        DateTime datePicked;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.PickDate);

            _dateDisplay = FindViewById<TextView>(Resource.Id.date_display);
            _dateSelectButton = FindViewById<Button>(Resource.Id.date_select_button);
            _dateSelectButton.Click += DateSelect_OnClick;

            SingleDayButton = FindViewById<Button>(Resource.Id.singleDay);

            DeleteAllButton = FindViewById<Button>(Resource.Id.DeleteAll);

            DeleteAllButton.Click += async (sender, e) =>
            {
                FindViewById<TextView>(Resource.Id.date_display).Text = "Deleting from database. Please Wait...";
                DeleteAllButton.Visibility = ViewStates.Gone;
                SingleDayButton.Visibility = ViewStates.Gone;
                _dateSelectButton.Visibility = ViewStates.Gone;

                await deleteAll();
                //FindViewById<TextView>(Resource.Id.date_display).Text = "Deleted Successfully";   
            };

            Button OkButton = FindViewById<Button>(Resource.Id.OKbutton);
            OkButton.Visibility = ViewStates.Gone;
            Button CancelButton = FindViewById<Button>(Resource.Id.CancelButton);
            CancelButton.Visibility = ViewStates.Gone;

            OkButton.Click += async (sender, e) =>
            {
                FindViewById<TextView>(Resource.Id.date_display).Text = "Deleting from database. Please Wait...";
                OkButton.Visibility = ViewStates.Gone;
                CancelButton.Visibility = ViewStates.Gone;
                await deleteWeighs(datePicked);
                //FindViewById<TextView>(Resource.Id.date_display).Text = "Deleted Successfully";   
            };

            CancelButton.Click += (sender, e) =>
            {
                //OkButton.Visibility = ViewStates.Gone;
                //CancelButton.Visibility = ViewStates.Gone;
                //var intent = new Intent(this, typeof(UpdateProfile))
                //.SetFlags(ActivityFlags.ReorderToFront);
                //StartActivity(intent);
                Finish();
            };

        }

        void DateSelect_OnClick(object sender, EventArgs eventArgs)
        {
            DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime time)
            {
                _dateDisplay.Text = "All weights prior to " + time.ToLongDateString() + "will be deleted !";
                datePicked = time;
                _dateSelectButton.Visibility = ViewStates.Gone;
                FindViewById<Button>(Resource.Id.OKbutton).Visibility = ViewStates.Visible;
                FindViewById<Button>(Resource.Id.CancelButton).Visibility = ViewStates.Visible;
                //await deleteWeighs(time);
            });
            frag.Show(FragmentManager, DatePickerFragment.TAG);
        }

        private async Task deleteWeighs(DateTime time)
        {
            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            try
            {
                weighTableRef = client.GetTable<weighTable>();
                //get all the to be deleted weights -  the weights before the specified date
                var toBeDeleted = await weighTableRef.Where(item => (item.username == ourUserId) && (item.createdAt <= time)).ToListAsync();
                if (toBeDeleted.Count == 0)
                {
                    //CreateAndShowDialog("No weights were found prior to the specified date", "Cannot Delete");
                    FindViewById<TextView>(Resource.Id.date_display).Text = "Cannot Delete.\nNo weights were found prior to the specified date";
                }
                else
                {
                    foreach (weighTable weight in toBeDeleted)
                    {
                        await weighTableRef.DeleteAsync(weight);
                        //Console.WriteLine("SLEEPING !!!!!!!!!!!!!!!!!!!");
                        //await Task.Delay(90000);
                        //object o2 = null;
                        //int i2 = (int)o2;   
                    }
              
                    FindViewById<TextView>(Resource.Id.date_display).Text = "Deleted Successfully";
                }
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }


        private async Task deleteAll()
        {
            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            try
            {
                weighTableRef = client.GetTable<weighTable>();
                var toBeDeleted = await weighTableRef.Where(item => (item.username == ourUserId)).ToListAsync();
                if (toBeDeleted.Count == 0)
                {
                    //CreateAndShowDialog("No weights were found prior to the specified date", "Cannot Delete");
                    FindViewById<TextView>(Resource.Id.date_display).Text = "Cannot Delete.\nNo weights were found in the database";
                }
                else
                {
                    foreach (weighTable weight in toBeDeleted)
                    {
                        await weighTableRef.DeleteAsync(weight);
                        //Console.WriteLine("SLEEPING !!!!!!!!!!!!!!!!!!!");
                        //await Task.Delay(90000);
                        //object o2 = null;
                        //int i2 = (int)o2;   
                    }

                    FindViewById<TextView>(Resource.Id.date_display).Text = "Deleted Successfully";
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
    }
}