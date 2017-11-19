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
    public class PickDateToDeleteVer2 : Activity
    {
        TextView _dateDisplay;
        Button _dateSelectButton;
        Button _remSingleButton;
        Button _remAllButton;
        private IMobileServiceTable<weighTable> weighTableRef;
        string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;
        //Bar's sid:    sid:f5ccac253e6e9ce70bd96a3b9a0b59d2
        //string ourUserId = "sid:f5ccac253e6e9ce70bd96a3b9a0b59d2";
        //ourUserId = BarSID; 
        DateTime datePicked;
        DeleteJob job;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.PickDateViewVer2);

            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            weighTableRef = client.GetTable<weighTable>();

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
                try
                {
                    switch (job)
                    {
                        case DeleteJob.Single:
                            queryResult = await weighTableRef.Take(500).Where(item => (item.username == ourUserId) && (item.createdAt <= datePicked) && (item.createdAt >= datePicked.AddDays(-1) ) ).ToListAsync();
                            //var list9 = await weighTableRef.Take(500).Where(item => (item.username == ourUserId) && (item.createdAt >= earliestDate)).ToListAsync();
                            Console.WriteLine("datePicked = " + datePicked + ", datePicked-1 = " + datePicked.AddDays(-1), "");
                            break;
                        case DeleteJob.All:
                            queryResult = await weighTableRef.Take(500).Where(item => (item.username == ourUserId)).ToListAsync();
                            break;
                        case DeleteJob.PriorToDate:
                            queryResult = await weighTableRef.Take(500).Where(item => (item.username == ourUserId) && (item.createdAt <= datePicked)).ToListAsync();
                            break;
                        default:
                            queryResult = new List<weighTable>();
                            break;
                    }
                    await deleteWeighs(queryResult);
                    //FindViewById<TextView>(Resource.Id.date_display).Text = "Deleted Successfully";  
                }
                catch (Exception ex)
                {
                    CreateAndShowDialog(ex, "Error");
                }
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
                _dateDisplay.Text = "All weighs on day " + time.ToLongDateString() + " will be deleted!";
                datePicked = time.AddDays(1);

                //CreateAndShowDialog("Date Picked = " + time, "");

                _dateSelectButton.Visibility = ViewStates.Gone;
                _remAllButton.Visibility = ViewStates.Gone;
                _remSingleButton.Visibility = ViewStates.Gone;
                job = DeleteJob.Single;
                FindViewById<Button>(Resource.Id.OKbutton).Visibility = ViewStates.Visible;
                FindViewById<Button>(Resource.Id.CancelButton).Visibility = ViewStates.Visible;
                
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
                job = DeleteJob.PriorToDate;
                FindViewById<Button>(Resource.Id.OKbutton).Visibility = ViewStates.Visible;
                FindViewById<Button>(Resource.Id.CancelButton).Visibility = ViewStates.Visible;
                
            });
            frag.Show(FragmentManager, DatePickerFragment.TAG);
        }

        private async Task deleteWeighs(List<weighTable> queryResult)
        {
            try
            {
                var toBeDeleted = queryResult;
                if (toBeDeleted.Count == 0)
                {
                    FindViewById<TextView>(Resource.Id.date_display).Text = "Cannot Delete.\nNo weights were found in the requested time period";
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
    enum DeleteJob { Single, All, PriorToDate };
}
