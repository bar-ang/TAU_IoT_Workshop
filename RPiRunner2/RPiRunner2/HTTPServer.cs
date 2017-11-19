using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using System.Diagnostics;
using System.IO;
using Windows.Storage.Streams;
using Windows.Storage;
using System.Text.RegularExpressions;

namespace RPiRunner2
{
    /// <summary>
    /// Create a Web Server that will run on the device.
    /// </summary>
    public class HTTPServer
    {
        private readonly int _port;
        public int Port { get { return _port; } }

        public bool Restricted { get => restricted; set => restricted = value; }
        public DateTime Last_login { get => last_login; set => last_login = value; }

        private StreamSocketListener listener;

        public delegate Task DataRecived(string data, DataWriter writer);
        public event DataRecived OnDataRecived;

        public delegate void Error(string message);
        public event Error OnError;

        private bool restricted;
        private DateTime last_login;

        private string username;
        private string password;

        /// <summary>
        /// Initialize a new web server.
        /// </summary>
        /// <param name="page"> the name of the file (HTML Document) to send to each new client who connects to the server</param>
        /// <param name="port">the port to listen to.</param>
        public HTTPServer(int port, string username = "admin", string password="admin")
        {
            _port = port;
            this.restricted = true;
            this.username = username;
            this.password = password;
        }

        public void changeCredentials(string username, string password)
        {
            this.username = username;
            this.password = password;
        }


        /// <summary>
        /// Retrives the HTML Document
        /// </summary>
        /// <returns>A string contains the content of the file</returns>
        public static async Task<string> getHTMLAsync(string page)
        {
            StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile sampleFile = await storageFolder.GetFileAsync(page);
            string text = await FileIO.ReadTextAsync(sampleFile);
            return text;
        }
        public static int passwordValidation(string pass, int minlength = 0, int maxlength = int.MaxValue)
        {
            if (pass.Length > maxlength || pass.Length < minlength)
                return -2;
            Regex regex = new Regex("[A-Za-z0-9_]+");
            if (regex.IsMatch(pass))
                return 0;
            else
                return -1;
        }
        public static string HTMLRewrite(string html, string tag, string id, string content)
        {
            Regex regex = new Regex("\\<" + tag + ".*id=\"" + id + "\".*\\>.*\\<\\/" + tag + "\\>");
            Match m = regex.Match(html);
            string str = m.Value;
            string repstr = str.Replace("><", ">" + content + "<");

            return html.Replace(str, repstr);
        }
        public static string HTMLInputFill(string html, string type, string name, string content)
        {
            Regex regex = new Regex("<input\\s.*type=\"" + type + "\"\\s.*name=\"" + name + "\"[\\s>\\/][^>]*>");
            Match m = regex.Match(html);
            string str = m.Value;
            string repstr = str.Replace("input", "input value=\"" + content + "\"");

            return html.Replace(str, repstr);
        }

        /// <summary>
        /// Start listenng to new Connections
        /// </summary>
        public async Task Start()
        {
            try
            {
                if (listener != null)
                {
                    await listener.CancelIOAsync();
                    listener.Dispose();
                    listener = null;
                }

                listener = new StreamSocketListener();

                listener.ConnectionReceived += Listener_ConnectionReceived;

                await listener.BindServiceNameAsync(Port.ToString());
                Debug.WriteLine("listening");
            }
            catch (Exception e)
            {
                if (OnError != null)
                    OnError(e.Message);
                Debug.WriteLine("error listen");
            }
        }

        private bool busy = false;
        /// <summary>
        /// This event activated for each new connection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.WriteLine("connected to " + args.Socket.Information.RemoteAddress + " my ip: " + args.Socket.Information.LocalAddress);
            //while (busy) { }
            busy = true;
            Debug.WriteLine("the connection with " + args.Socket.Information.RemoteAddress + " is being serviced.");
            var reader = new DataReader(args.Socket.InputStream);
            var writer = new DataWriter(args.Socket.OutputStream);
            string data = "";
            bool error = false;
            while (true)
            {

                uint load = await reader.LoadAsync(1).AsTask();
                if(load < 1)
                {
                    Debug.Write("stream ended");
                    return;
                }

                data += reader.ReadString(load);

                if (data.Length > 3 && data.ToUpper().IndexOf("GET") < 0)
                {
                    Debug.WriteLine("Illegal message (non HTTP)");
                    error = true;
                    break;
                }

                if (data.IndexOf("\r\n\r\n") >= 0 || data.IndexOf("\n\n") >= 0 || data.IndexOf("\r\r") >= 0)
                    break;
            }
            busy = false;
            if (!error)
                await OnDataRecived(data, writer);
            else
                OnError("Disconnected during data transfer.");

        }

        /// <summary>
        /// Sends data back to the currently connected client.
        /// If there is no ongoing connections, this function will take no action.
        /// </summary>
        /// <param name="message">The message to send</param>
        public async Task Send(string message, DataWriter writer)
        {
            if (writer != null)
            {
                writer.WriteString(message);

                try
                {
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                }
                catch (Exception ex)
                {
                    if (OnError != null)
                        OnError(ex.Message);
                }
            }
        }
    }
}