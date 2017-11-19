using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.WindowsAzure.MobileServices;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;


namespace IoTWeight
{
    [Activity(Label = "DeleteWeighs")]
    public class DeleteWeighs : Activity
    {
        private IMobileServiceTable<weighTable> weighTableRef;
        string ourUserId = ToDoActivity.CurrentActivity.Currentuserid;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.deleteWeighs);
            // Create your application here
            MobileServiceClient client = ToDoActivity.CurrentActivity.CurrentClient;
            try
            {
                weighTableRef = client.GetTable<weighTable>();
                DateTime today = DateTime.Now;
                //DateTime earliestDate = today.AddDays(-1);
                DateTime earliestDate = today.AddMinutes(-30);

                //get all the to be deleted weights -  the weights before the specified date
                var toBeDeleted = await weighTableRef.Where(item => (item.username == ourUserId) && (item.createdAt <= earliestDate)).ToListAsync();

                
                if (toBeDeleted.Count == 0)
                {
                    CreateAndShowDialog("No weights were found prior to the specified date", "Cannot Delete");
                }
                else
                {
                    foreach (weighTable weight in toBeDeleted)
                    {
                        await weighTableRef.DeleteAsync(weight);
                    }
                    //TODO:  go back to previous activity
                    CreateAndShowDialog("", "Deleted Successfully");
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