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
        Button _remSingleButton;
        Button _remAllButton;
        private IMobileServiceTable<weighTable> weighTableRef;
        string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;
        DateTime datePicked;
        DeleteJob job;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.PickDate);

            _dateDisplay = FindViewById<TextView>(Resource.Id.date_display);

            _dateSelectButton = FindViewById<Button>(Resource.Id.date_select_button);
            _remSingleButton = FindViewById<Button>(Resource.Id.remove_single_button);
            _remAllButton = FindViewById<Button>(Resource.Id.remove_all_button);

            _dateSelectButton.Click += DateSelect_OnClick;
            _remSingleButton.Click += _remSingleButton_Click;
            _remAllButton.Click += _remAllButton_Click;

            Button OkButton = FindViewById<Button>(Resource.Id.OKbutton);
            OkButton.Visibility = ViewStates.Gone;
            Button CancelButton = FindViewById<Button>(Resource.Id.CancelButton);
            CancelButton.Visibility = ViewStates.Gone;

            OkButton.Click += async (sender, e) =>
            {
                FindViewById<TextView>(Resource.Id.date_display).Text = "Deleting from database. Please Wait...";
                OkButton.Visibility = ViewStates.Gone;
                CancelButton.Visibility = ViewStates.Gone;
                List<weighTable> queryResult;
                switch (job)
                {
                    case DeleteJob.Single:
                        queryResult = await weighTableRef.Where(item => (item.username == ourUserId) && (item.createdAt == datePicked)).ToListAsync();
                        break;
                    case DeleteJob.All:
                        queryResult = await weighTableRef.Where(item => (item.username == ourUserId)).ToListAsync();
                        break;
                    case DeleteJob.PriorToDate:
                        queryResult = await weighTableRef.Where(item => (item.username == ourUserId) && (item.createdAt <= datePicked)).ToListAsync();
                        break;
                    default:
                        queryResult = new List<weighTable>();
                        break;
                }
                await deleteWeighs(queryResult);
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

        private void _remAllButton_Click(object sender, EventArgs e)
        {
            job = DeleteJob.All;
            _dateDisplay.Text = "ALL WEIGHTS WILL BE DELETED.\nThere will be no way to restore them. are you sure you want to continue?";
            _dateSelectButton.Visibility = ViewStates.Gone;
            _remAllButton.Visibility = ViewStates.Gone;
            _remSingleButton.Visibility = ViewStates.Gone;
            FindViewById<Button>(Resource.Id.OKbutton).Visibility = ViewStates.Visible;
            FindViewById<Button>(Resource.Id.CancelButton).Visibility = ViewStates.Visible;
        }

        private void _remSingleButton_Click(object sender, EventArgs e)
        {
            DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime time)
            {
                _dateDisplay.Text = "The weight on day " + time.ToLongDateString() + " will be deleted!";
                datePicked = time;
                _dateSelectButton.Visibility = ViewStates.Gone;
                _remAllButton.Visibility = ViewStates.Gone;
                _remSingleButton.Visibility = ViewStates.Gone;
                FindViewById<Button>(Resource.Id.OKbutton).Visibility = ViewStates.Visible;
                FindViewById<Button>(Resource.Id.CancelButton).Visibility = ViewStates.Visible;
                job = DeleteJob.Single;
            });
            frag.Show(FragmentManager, DatePickerFragment.TAG);
        }

        void DateSelect_OnClick(object sender, EventArgs eventArgs)
        {
            DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime time)
            {
                _dateDisplay.Text = "All weights prior to " + time.ToLongDateString() + " will be deleted !";
                datePicked = time;
                _dateSelectButton.Visibility = ViewStates.Gone;
                _remAllButton.Visibility = ViewStates.Gone;
                _remSingleButton.Visibility = ViewStates.Gone;
                FindViewById<Button>(Resource.Id.OKbutton).Visibility = ViewStates.Visible;
                FindViewById<Button>(Resource.Id.CancelButton).Visibility = ViewStates.Visible;
                job = DeleteJob.PriorToDate;
            });
            frag.Show(FragmentManager, DatePickerFragment.TAG);
        }

        private async Task deleteWeighs(List<weighTable> queryResult)
        {
            try
            {
                MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
                var weighTableRef = client.GetTable<weighTable>();
                var toBeDeleted = queryResult;
                if (toBeDeleted.Count == 0)
                {
                    FindViewById<TextView>(Resource.Id.date_display).Text = "Cannot Delete.\nNo weights were found prior to the specified date";
                }
                else
                {
                    foreach (weighTable weight in toBeDeleted)
                    {
                        await weighTableRef.DeleteAsync(weight);
   
                    }

                    FindViewById<TextView>(Resource.Id.date_display).Text = "Deleted Successfully";
                }
            }
            catch (Exception e)
            {
                CreateAndShowDialog(e, "Error");
            }
        }

        /*
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

        */
        void CreateAndShowDialog(Exception exception, String title)
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
    enum DeleteJob { Single, All, PriorToDate};
}