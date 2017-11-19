using System;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using System.Threading.Tasks;

namespace EventReader
{
    class Program
    {
        private const string EhConnectionString = "Endpoint=sb://appevent.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=5ZC6tOzkeVNUiwmG3ff4PyqpGAnGi98XVFdktcExEIU=";
        private const string EhEntityPath = "messagesr";
        private const string StorageContainerName = "tutorialcontainer";
        private const string StorageAccountName = "taustorage";
        private const string StorageAccountKey = "IlKP0k2Qyxp98d/4/npD07NBgOwUaNw7wEg71NVD4hvC+qoeStUDWtgJIsXKUSw+IR5ELrDELK7ycq0MNcqkHA==";

        private static readonly string StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);

        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Registering EventProcessor...");

            var eventProcessorHost = new EventProcessorHost(
                EhEntityPath,
                PartitionReceiver.DefaultConsumerGroupName,
                EhConnectionString,
                StorageConnectionString,
                StorageContainerName);

            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();

            Console.WriteLine("Receiving. Press ENTER to stop worker.");
            Console.ReadLine();

          

             // Disposes of the Event Processor Host
             await eventProcessorHost.UnregisterEventProcessorAsync();
        }
    }
}
