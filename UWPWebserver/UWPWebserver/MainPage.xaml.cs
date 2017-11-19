using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPWebserver
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        HTTPServer socket;
        public MainPage()
        {
            socket = new HTTPServer(@"SettingsPage.html", 9000);
            System.Diagnostics.Debug.WriteLine("trying to load server");
            socket.OnError += socket_OnError;
            socket.OnDataRecived += Socket_OnDataRecived;
            socket.Start();
            System.Diagnostics.Debug.WriteLine("Server is running");
            InitializeComponent();
        }
        public async void Socket_OnDataRecived(string data)
        {
            string[] fields;
            if (data.Split(' ').Length > 1)
            {
                string query = data.Split(' ')[1];
                if (query.IndexOf("/?") == 0) {
                    query = query.Substring(2);
                    System.Diagnostics.Debug.WriteLine(query);
                    fields = query.Split('&');
                    foreach(string field in fields)
                    {
                        string prop, val;
                        string[] sep = field.Split('=');
                        prop = sep[0];
                        val = sep[1];
                        System.Diagnostics.Debug.WriteLine(sep[0] + " -> " + sep[1]);
                    }
                }
            }
            string html = await socket.getHTMLAsync();
            string response = wrapWithHTTPHeaders(html);
            socket.Send(response);
        }

        public string wrapWithHTTPHeaders(string msg)
        {
            string response = "";
            response += "HTTP/1.1 200 OK\n";
            response += "Server: TAU_IoT_Workshop\n";
            response += "Content-Type: text/html\n";
            response += "Content-Length: " + msg.Length.ToString() + "\n";
            response += "\n";
            response += msg;
            return response;
        }

        public void socket_OnError(string message)
        {
            System.Diagnostics.Debug.WriteLine("oops");
        }
    }
}
