/*
 * To add Offline Sync Support:
 *  1) Add the NuGet package Microsoft.Azure.Mobile.Client.SQLiteStore (and dependencies) to all client projects
 *  2) Uncomment the #define OFFLINE_SYNC_ENABLED
 *
 * For more information, see: http://go.microsoft.com/fwlink/?LinkId=717898
 */
//#define OFFLINE_SYNC_ENABLED

using System;
using System.Net;
using System.Threading.Tasks;
using Android.OS;
using Android.App;
using Android.Views;
using Android.Util;
using Android.Widget;
using Android.Net.Wifi;
using Android.Content;
using Microsoft.WindowsAzure.MobileServices;
using Java.Net;
using Java.IO;

#if OFFLINE_SYNC_ENABLED
using Microsoft.WindowsAzure.MobileServices.Sync;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
#endif

namespace weighJune28
{
    [Activity(MainLauncher = true,
               Icon = "@drawable/ic_launcher", Label = "@string/app_name",
               Theme = "@style/AppTheme")]

    public class ToDoActivity : Activity
    {
        // Client reference.
        public static MobileServiceClient client;
        public static string userid = "";
        const string applicationURL = @"https://weighjune28.azurewebsites.net";
        // Create a new instance field for this activity.
        static ToDoActivity instance = new ToDoActivity();

        // Return the current activity instance.
        public static ToDoActivity CurrentActivity
        {
            get
            {
                return instance;
            }
        }
        // Return the Mobile Services client.
        public MobileServiceClient CurrentClient
        {
            get
            {
                return client;
            }
        }

        public string Currentuserid
        {
            get
            {
                return userid;
            }
            set
            {
                userid = value;
            }
        }



        public static IPAddress GetSubnetMaskFromPrefix(short prefix)
        {
            long mask = (0xffffffffL << (32 - prefix)) & 0xffffffffL;
            mask = IPAddress.HostToNetworkOrder((int)mask);
            return new IPAddress((UInt32)mask);
        }

        public static IPAddress GetNetworkAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }

        public static bool IsInSameSubnet(IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress network1 = GetNetworkAddress(address, subnetMask);
            IPAddress network2 = GetNetworkAddress(address2, subnetMask);

            return network1.Equals(network2);
        }

        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
           
            SetContentView(Resource.Layout.Main);
            CurrentPlatform.Init ();
            client = new MobileServiceClient(applicationURL);
            // Set the current instance of TodoActivity.
            instance = this;

            WifiManager wifiManager = (WifiManager)GetSystemService(Context.WifiService);
            int myIP = (int)wifiManager.ConnectionInfo.IpAddress;

            var d = wifiManager.DhcpInfo;
            short prefix = 0;




            try
            {
                byte[] ipAddress = BitConverter.GetBytes(d.IpAddress);
                InetAddress inetAddress = InetAddress.GetByAddress(ipAddress);
                NetworkInterface networkInterface = NetworkInterface.GetByInetAddress(inetAddress);
                foreach (InterfaceAddress address in networkInterface.InterfaceAddresses)
                {
                    //short netPrefix = address.getNetworkPrefixLength(); 
                    //Log.d(TAG, address.toString());
                    prefix = address.NetworkPrefixLength;

                }
            }
            catch (IOException exception)
            {
                Log.Debug("Exception:", exception.Message);
            }

            IPAddress myIpAddress = new IPAddress(myIP);

            IPAddress networkmask = GetSubnetMaskFromPrefix(prefix);
            IPAddress myNetworkAddress = GetNetworkAddress(myIpAddress, networkmask);



            var RaspberryTableRef = client.GetTable<RaspberryTable>();
            var raspIPList = await (from item in RaspberryTableRef
							       where(IsInSameSubnet(new IPAddress(item.IPNumber), myIpAddress, networkmask) == true)
								   select item.IPNumber)
								   .ToListAsync();
								   
			MEPClient mepClient = MEPClient(MyMac, myIpAddress, raspIPList);
			mepClient.run();



            //Button logInButton = FindViewById<Button>(Resource.Id.LogIn);
            //logInButton.Click += (sender, e) =>
            //{
            //    var intent = new Intent(this, typeof(LogInActivity));
            //    //intent.PutStringArrayListExtra("phone_numbers", phoneNumbers);
            //    //intent.PutExtra("Microsoft.WindowsAzure.MobileServices.MobileServiceClient.client", client);
            //    StartActivity(intent);
            //};

        }



        // Define a authenticated user.
        private MobileServiceUser user;
        private async Task<bool> Authenticate()
        {
            var success = false;
            try
            {
               
                user = await client.LoginAsync(this,
                    MobileServiceAuthenticationProvider.Facebook);
                CreateAndShowDialog(string.Format("you are now logged in - {0}",
                    user.UserId), "Logged in!");
                Currentuserid = user.UserId;

                success = true;
            }
            catch (Exception ex)
            {
                CreateAndShowDialog(ex, "Authentication failed");
            }
            return success;
        }

        [Java.Interop.Export()]
        public async void LoginUser(View view)
        {
            // Load data only after authentication succeeds.
            if (await Authenticate())
            {
                //Hide the button after authentication succeeds.
                FindViewById<Button>(Resource.Id.buttonLoginUser).Visibility = ViewStates.Gone;

                var intent = new Intent(this, typeof(LogInActivity));
                //intent.PutExtra("client", client);
                StartActivity(intent); 
            }
        }

        



        //CurrentPlatform.Init();

        // Create the client instance, using the mobile app backend URL.
        //client = new MobileServiceClient(applicationURL);
        //#if OFFLINE_SYNC_ENABLED
        //            await InitLocalStoreAsync();

        //            // Get the sync table instance to use to store TodoItem rows.
        //            todoTable = client.GetSyncTable<ToDoItem>();
        //#else
        //            todoTable = client.GetTable<ToDoItem>();
        //#endif

        //textNewToDo = FindViewById<EditText>(Resource.Id.textNewToDo);

        // Create an adapter to bind the items with the view
        //adapter = new ToDoItemAdapter(this, Resource.Layout.Row_List_To_Do);
        //var listViewToDo = FindViewById<ListView>(Resource.Id.listViewToDo);
        // listViewToDo.Adapter = adapter;

        // Load the items from the mobile app backend.
        // OnRefreshItemsSelected();
        //

        //#if OFFLINE_SYNC_ENABLED
        //        private async Task InitLocalStoreAsync()
        //        {
        //            var store = new MobileServiceSQLiteStore(localDbFilename);
        //            store.DefineTable<ToDoItem>();

        //            // Uses the default conflict handler, which fails on conflict
        //            // To use a different conflict handler, pass a parameter to InitializeAsync.
        //            // For more details, see http://go.microsoft.com/fwlink/?LinkId=521416
        //            await client.SyncContext.InitializeAsync(store);
        //        }

        //        private async Task SyncAsync(bool pullData = false)
        //        {
        //            try {
        //                await client.SyncContext.PushAsync();

        //                if (pullData) {
        //                    await todoTable.PullAsync("allTodoItems", todoTable.CreateQuery()); // query ID is used for incremental sync
        //                }
        //            }
        //            catch (Java.Net.MalformedURLException) {
        //                CreateAndShowDialog(new Exception("There was an error creating the Mobile Service. Verify the URL"), "Error");
        //            }
        //            catch (Exception e) {
        //                CreateAndShowDialog(e, "Error");
        //            }
        //        }
        //#endif

        //        //Initializes the activity menu
        //        public override bool OnCreateOptionsMenu(IMenu menu)
        //        {
        //            MenuInflater.Inflate(Resource.Menu.activity_main, menu);
        //            return true;
        //        }

        //        //Select an option from the menu
        //        public override bool OnOptionsItemSelected(IMenuItem item)
        //        {
        //            if (item.ItemId == Resource.Id.menu_refresh) {
        //                item.SetEnabled(false);

        //                OnRefreshItemsSelected();

        //                item.SetEnabled(true);
        //            }
        //            return true;
        //        }

        //        // Called when the refresh menu option is selected.
        //        private async void OnRefreshItemsSelected()
        //        {
        //#if OFFLINE_SYNC_ENABLED
        //			// Get changes from the mobile app backend.
        //            await SyncAsync(pullData: true);
        //#endif
        //			// refresh view using local store.
        //            await RefreshItemsFromTableAsync();
        //        }

        //        //Refresh the list with the items in the local store.
        //        private async Task RefreshItemsFromTableAsync()
        //        {
        //            try {
        //                // Get the items that weren't marked as completed and add them in the adapter
        //                var list = await todoTable.Where(item => item.Complete == false).ToListAsync();

        //                adapter.Clear();

        //                foreach (ToDoItem current in list)
        //                    adapter.Add(current);

        //            }
        //            catch (Exception e) {
        //                CreateAndShowDialog(e, "Error");
        //            }
        //        }

        //        public async Task CheckItem(ToDoItem item)
        //        {
        //            if (client == null) {
        //                return;
        //            }

        //            // Set the item as completed and update it in the table
        //            item.Complete = true;
        //            try {
        //				// Update the new item in the local store.
        //                await todoTable.UpdateAsync(item);
        //#if OFFLINE_SYNC_ENABLED
        //                // Send changes to the mobile app backend.
        //				await SyncAsync();
        //#endif

        //                if (item.Complete)
        //                    adapter.Remove(item);

        //            }
        //            catch (Exception e) {
        //                CreateAndShowDialog(e, "Error");
        //            }
        //        }

        //        [Java.Interop.Export()]
        //        public async void AddItem(View view)
        //        {
        //            if (client == null || string.IsNullOrWhiteSpace(textNewToDo.Text)) {
        //                return;
        //            }

        //            // Create a new item
        //            var item = new ToDoItem {
        //                Text = textNewToDo.Text,
        //                Complete = false
        //            };

        //            try {
        //				// Insert the new item into the local store.
        //                await todoTable.InsertAsync(item);
        //#if OFFLINE_SYNC_ENABLED
        //                // Send changes to the mobile app backend.
        //				await SyncAsync();
        //#endif

        //                if (!item.Complete) {
        //                    adapter.Add(item);
        //                }
        //            }
        //            catch (Exception e) {
        //                CreateAndShowDialog(e, "Error");
        //            }

        //            textNewToDo.Text = "";
        //        }

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


