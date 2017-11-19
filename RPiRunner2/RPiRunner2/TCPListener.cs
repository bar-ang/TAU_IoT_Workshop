using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace RPiRunner2
{
    public class TCPListener
    {
        private const int DEFAULT_PORT = 9000;
        private int port;

        private StreamSocketListener listener;

        public delegate Task DataRecived(string data, DataWriter writer);
        public event DataRecived OnDataReceived;

        public delegate void Error(string message);
        public event Error OnError;

        public int Port { get => port; set => port = value; }

        private bool easyDebug;

        public TCPListener(int port, bool easyDebug = false)
        {
            this.port = port;
            this.easyDebug = easyDebug;
        }
        public TCPListener()
        {
            this.port = DEFAULT_PORT;
            this.easyDebug = false;
        }

        public async Task ListenAsync()
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
                OnError(e.Message);
            }
        }

        private bool busy = false;
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                Debug.WriteLine("connection accepted to client " + args.Socket.Information.RemoteAddress);
                // while (busy) { } //TODO: this may block. need to check this.
                Debug.WriteLine("connection is serviced with " + args.Socket.Information.RemoteAddress);
                busy = true;
                //We assume here that the first 4 bytes of the message will always contain the message's length.
                var reader = new DataReader(args.Socket.InputStream);
                var writer = new DataWriter(args.Socket.OutputStream);

                uint msize = await reader.LoadAsync(sizeof(uint)); //receiving the message length
                if (msize < sizeof(uint))
                {
                    Debug.WriteLine("disconnected during data transfer. :-(");
                    return;
                }
                uint length = reader.ReadUInt32();

                if (easyDebug)
                    length = 5;

                var loadTask = reader.LoadAsync(length).AsTask();
                if (await Task.WhenAny(loadTask, Task.Delay(10000)) != loadTask)
                {
                    Debug.WriteLine("loader did not return (perhaps the size is incorrect?) :-(");
                    return;
                }
                uint loaded = loadTask.Result;
                if (loaded < length)
                {
                    Debug.WriteLine("disconnected during data transfer. :-(");
                    return;
                }

                string msg = reader.ReadString(loaded);

                await OnDataReceived(msg, writer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Listening failed.");
                OnError(ex.Message);
            }
            busy = false;
        }

        public async Task Send(string message, DataWriter writer)
        {
            try
            {
                string send = message;
                if (writer != null)
                {

                    writer.WriteInt32(send.Length); //write message length
                    writer.WriteString(send); //write message

                    await writer.StoreAsync();
                    await writer.FlushAsync();
                }

            }
            catch (Exception ex)
            {
                OnError(ex.Message);
            }
        }

    }
}

