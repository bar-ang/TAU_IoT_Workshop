using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
using Windows.Devices.Gpio;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Networking.Connectivity;
using System.Text.RegularExpressions;
using System.Text;
using System.Net;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RPiRunner2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string applicationURL = @"https://iotweight.azurewebsites.net";

        const byte DOUT_PIN = 26;
        const byte SLK_PIN = 19;
    
        private UserHardwareLinker uhl;

        private GpioController gpioController;
        private GpioPin dout, clk;

        //Note: cannot be async as it is the first function to run in the proccess (thus no father proccess to return the control with 'await').
        public MainPage()
        {
            this.InitializeComponent();
            Task.Run(() => MainPageAsync());
        }

        public async Task MainPageAsync()
        {

            //Initializing GPIO 
            gpioController = GpioController.GetDefault();
            if (gpioController != null)
            {
                dout = gpioController.OpenPin(DOUT_PIN);
                clk = gpioController.OpenPin(SLK_PIN);
                uhl = new UserHardwareLinker(clk, dout);
                System.Diagnostics.Debug.WriteLine("Connected to Hardware via GPIO.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("WARNING: Your machine does not support GPIO!");
            }

            //Start listening to messages from clients
            new ConnectionHandler(uhl);

            //Start Web Server
            new WebHandler(uhl);

           
            //loading permanent data from memory
            Task loadTask = PermanentData.LoadFromMemoryAsync();
            await loadTask;

            Task putRecordTask = null;
            PermanentData.CurrIP = GetLocalIp();
            if (!PermanentData.Serial.Equals(PermanentData.NULL_SYMBOL))
                putRecordTask = putRecordInDatabase(PermanentData.CurrIP, PermanentData.Serial);
            if (uhl != null)
                uhl.setParameters(PermanentData.Offset, PermanentData.Scale);

            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

            if (putRecordTask != null)
            {
                await putRecordTask;
            }
        }

        //Activates when the IP is changed, and update it in the cloud
        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            System.Diagnostics.Debug.WriteLine("Net status has changed!");
            try
            {
                string newIP = GetLocalIp();
                if (!newIP.Trim().Equals(""))
                {
                    PermanentData.CurrIP = newIP;
                    await putRecordInDatabase(PermanentData.CurrIP, PermanentData.Serial);
                    System.Diagnostics.Debug.WriteLine("new IP: " + PermanentData.CurrIP);
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Could not get IP address, it's fine if there is no internet connection.");
            }
        }

        //Returns the device's IP Address
        private string GetLocalIp()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp?.NetworkAdapter == null) return null;
            var hostname =
                NetworkInformation.GetHostNames()
                    .SingleOrDefault(
                        hn =>
                            hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                            == icp.NetworkAdapter.NetworkAdapterId && hn.CanonicalName.Split('.').Length == 4);
            // the ip address
            return hostname?.CanonicalName;
        }

        //This function inserts to the SQL database a record with the Raspberry's Ip address and QR code.
        //Also: In case the Raspberry's Ip address was changed,  this function updates the database with the current IP.
        public async Task putRecordInDatabase(string ipAdd, string QR_Code)
        {
            var client = new MobileServiceClient(applicationURL);
            //TODO: there is not async version od GetTable, but we need to make it async somwhow
            IMobileServiceTable<RaspberryTable> raspberryTableRef = client.GetTable<RaspberryTable>();
            try
            {
                List<RaspberryTable> ipAddressList = await raspberryTableRef.Where(item => (item.QRCode == QR_Code)).ToListAsync();
                if (ipAddressList.Count == 0)
                {
                    var record1 = new RaspberryTable
                    {
                        QRCode = QR_Code,
                        IPAddress = ipAdd,
                    };
                    await raspberryTableRef.InsertAsync(record1);
                }
                else    //case where we want to update an old ip address
                {
                    var address = ipAddressList[0];
                    address.IPAddress = ipAdd;
                    await raspberryTableRef.UpdateAsync(address);
                }


                //System.Diagnostics.Debug.WriteLine("i'm after insert");
            }
            catch (Exception e)
            {
                //TODO Ron
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
    }
}
