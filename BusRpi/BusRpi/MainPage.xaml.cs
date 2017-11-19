using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;

using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System.Threading.Tasks;
using System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BusRpi
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static EventHubClient eventHubClient;
        private const string EhConnectionString = "Endpoint=sb://appevent.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=5ZC6tOzkeVNUiwmG3ff4PyqpGAnGi98XVFdktcExEIU=";
        private const string EhEntityPath = "messagesr";
        private const string StorageContainerName = "tutorialcontainer";
        private const string StorageAccountName = "taustorage";
        private const string StorageAccountKey = "IlKP0k2Qyxp98d/4/npD07NBgOwUaNw7wEg71NVD4hvC+qoeStUDWtgJIsXKUSw+IR5ELrDELK7ycq0MNcqkHA==";

        private static readonly string StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);

        public MainPage()
        {
            this.InitializeComponent();

            //sendSong();
            startReceiver();
            //listen();
        }
        public async void listen()
        {
            while (true)
            {
                string cloudMessage = await AzureIoTHub.ReceiveCloudToDeviceMessageAsync();
                System.Diagnostics.Debug.WriteLine(cloudMessage);
            }
        }

        public async void sendSong()
        {
            System.Diagnostics.Debug.WriteLine("sending...");

            Dictionary<string, string> jsend = new Dictionary<string, string>();
            DRP drp = new DRP(DRPDevType.RBPI, "yeguslavia", 2, 3, new System.Collections.Generic.List<float>(), 0, DRPMessageType.ILLEGAL);

            await AzureIoTHub.SendDeviceToCloudMessageAsync(drp.ToString());

            System.Diagnostics.Debug.WriteLine("message send");
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
