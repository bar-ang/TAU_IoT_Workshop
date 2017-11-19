using Android.App;
using Android.Widget;
using Android.OS;

using Microsoft.WindowsAzure.MobileServices;
using System.Text;

using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System.Threading.Tasks;
using System;

namespace BusApp
{
    [Activity(Label = "BusApp", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private static EventHubClient eventHubClient;
        private const string EhConnectionString = "Endpoint=sb://appevent.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=5ZC6tOzkeVNUiwmG3ff4PyqpGAnGi98XVFdktcExEIU=";
        private const string EhEntityPath = "messagesr";
        private const string StorageContainerName = "tutorialcontainer";
        private const string StorageAccountName = "taustorage";
        private const string StorageAccountKey = "IlKP0k2Qyxp98d/4/npD07NBgOwUaNw7wEg71NVD4hvC+qoeStUDWtgJIsXKUSw+IR5ELrDELK7ycq0MNcqkHA==";

        private static readonly string StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Initialization for Azure Mobile Apps
            CurrentPlatform.Init();
            // This MobileServiceClient has been configured to communicate with the Azure Mobile App and
            // Azure Gateway using the application url. You're all set to start working with your Mobile App!
            MobileServiceClient TAUtrycommuncateClient = new MobileServiceClient("https://tautrycommuncate.azurewebsites.net");

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            //startReceiver();

            sender();
        }

        private async void sender()
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EhConnectionString)
            {
                EntityPath = EhEntityPath
            };

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            await SendMessagesToEventHub(10);

            await eventHubClient.CloseAsync();
        }

        private static async Task SendMessagesToEventHub(int numMessagesToSend)
        {
            for (var i = 0; i < numMessagesToSend; i++)
            {
                try
                {
                    DRP msg = new DRP(DRPDevType.RBPI, "UsernameNo" + i, 2, 3, new System.Collections.Generic.List<float>(), 0, DRPMessageType.ILLEGAL);
                    System.Diagnostics.Debug.WriteLine($"Sending message: {msg}");
                    await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(msg.ToString())));
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception: {exception.Message}");
                }

                await Task.Delay(10);
            }

            System.Diagnostics.Debug.WriteLine($"{numMessagesToSend} messages sent.");
        }


        private static async Task startReceiver()
        {
            System.Diagnostics.Debug.WriteLine("Registering EventProcessor...");

            var eventProcessorHost = new EventProcessorHost(
                EhEntityPath,
                PartitionReceiver.DefaultConsumerGroupName,
                EhConnectionString,
                StorageConnectionString,
                StorageContainerName);

            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();

            System.Diagnostics.Debug.WriteLine("Receiving. Press ENTER to stop worker.");

            // Disposes of the Event Processor Host
            //await eventProcessorHost.UnregisterEventProcessorAsync();
        }

    }
}

