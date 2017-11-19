﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAzure.MobileServices;

namespace ScaleHubForWindows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string applicationURL = @"https://iotweight.azurewebsites.net";
        const string WELCOME_MSG = "@welcome@";
        const int PORT = 9888;
        const int WEBPORT = 9000;
        List<RaspTableWithDevname> listItems;

        int count = 0;
        public MainWindow()
        {
            this.InitializeComponent();
            listItems = new List<RaspTableWithDevname>();


            lv_scalesnearby.ItemsSource = listItems;
            Task.Run(() => MainPageAsync());
        }


        public async void DistinctIP(List<RaspberryTable> rasps)
        {
            string seen = "";
            for (int i = rasps.Count - 1; i >= 0; i--)
            {
                if (seen.IndexOf(rasps[i].IPAddress) >= 0)
                    rasps.RemoveAt(i);
                else
                    seen += " " + rasps[i].IPAddress;
            }
        }

        public async Task MainPageAsync()
        {
            List<RaspberryTable> rasps = await getListOfRasps();

            //FilterByMask(rasps, "192.168.1.0", 24);

            Dictionary<Task<string>, RaspberryTable> connections = new Dictionary<Task<string>, RaspberryTable>();

            DistinctIP(rasps);

            foreach (RaspberryTable rasp in rasps)
            {
                connections.Add(tryConnect(rasp.IPAddress), rasp);
            }



            string result;
            RaspberryTable rtForConn;
            Task<string> firstReturn;
            Task uiUpdate = null;
            count = 0;
            while (connections.Count > 0)
            {
                firstReturn = await Task.WhenAny(connections.Keys);
                if (firstReturn == null)
                    continue;
                result = firstReturn.Result;
                rtForConn = connections[firstReturn];
                if (rtForConn == null)
                    continue;
                if (firstReturn.Result != null)
                {
                    System.Diagnostics.Debug.WriteLine(result + " (" + rtForConn.QRCode + ") -- " + rtForConn.IPAddress);
                    listItems.Add(RaspTableWithDevname.initFromTable(rtForConn, result));
                    count++;
                    if (uiUpdate != null)
                        await uiUpdate;
                    uiUpdate = this.Dispatcher.InvokeAsync(new Action(() =>
                    {
                        lv_scalesnearby.ItemsSource = new List<RaspTableWithDevname>(listItems);
                        tb_wait.Text = count + " devices found.";
                    })).Task;
                }
                connections.Remove(firstReturn);
                System.Diagnostics.Debug.WriteLine("CONNECTIONS: " + connections.Count);
            }
            if (count == 0)
            {
                await this.Dispatcher.InvokeAsync(new Action(() =>
                {
                    tb_wait.Text = "no devices found in range.";
                }));
            }

            System.Diagnostics.Debug.WriteLine("That's all folks.");
        }


        public async Task<List<RaspberryTable>> getListOfRasps()
        {
            var client = new MobileServiceClient(applicationURL);
            IMobileServiceTable<RaspberryTable> raspberryTableRef = client.GetTable<RaspberryTable>();
            try
            {
                List<RaspberryTable> raspsList = await raspberryTableRef.ToListAsync();
                return raspsList;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return null;
            }
        }

        public uint addrToUint(string addr)
        {
            string[] strbytes = addr.Split('.');
            uint addrbits = 0;
            uint mul = 1;
            for (int i = 0; i < 4; i++)
            {
                addrbits += byte.Parse(strbytes[3 - i]) * mul;
                mul *= 256;
            }
            return addrbits;
        }

        public void FilterByMask(List<RaspberryTable> addrs, string mask, int length)
        {
            uint bmask = addrToUint(mask);
            uint baddr;
            uint setmask;
            string addr;
            for (int i = addrs.Count - 1; i >= 0; i--)
            {
                addr = addrs[i].IPAddress;
                baddr = addrToUint(addr);
                baddr >>= (32 - length);
                setmask = bmask >> (32 - length);

                if (setmask == baddr)
                {
                    //System.Diagnostics.Debug.WriteLine(addr + " is in subnet " + mask);
                }
                else
                {
                    // System.Diagnostics.Debug.WriteLine(addr + " NOT in subnet " + mask);
                    addrs.RemoveAt(i);
                }
            }
        }

        public async Task<string> tryConnect(string ip)
        {
            TCPSender tcps = new TCPSender(PORT);
            bool hasConnection = await tcps.Connect(ip, 3000);
            if (!hasConnection)
                return null;
            await tcps.Send(WELCOME_MSG);
            string[] res = (await tcps.Receive()).Split('=');
            if (res.Length != 2 || !res[0].Equals("TAUIOT@devname"))
                return null;
            else
                return res[1];
        }
        
        /*
        private async void lv_scalesnearby_ItemClick(object sender, ItemClickEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("clickify");

            ListView senderlist = sender as ListView;
            string ip = (e.ClickedItem as RaspTableWithDevname).IPAddress;
            string url = "http://" + ip + ":" + WEBPORT;

            //var dataPackage = new DataPackage();
            //dataPackage.SetText(url);
            //
            Clipboard.SetContent(dataPackage);

            var dialog = new MessageDialog("The address was copied to clipboard:\n" + url, "Copied");
            await dialog.ShowAsync();
        }

    */

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            listItems.Clear();
            await MainPageAsync();
        }

        private void lv_scalesnearby_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("clickify");

            ListView senderlist = sender as ListView;
            string ip = (e.AddedItems[0] as RaspTableWithDevname).IPAddress;
            string url = "http://" + ip + ":" + WEBPORT;
            
            Clipboard.SetText(url);

            MessageBox.Show("The address was copied to clipboard:\n" + url, "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
            
        }
    }
}
