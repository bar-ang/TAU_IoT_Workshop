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

namespace UWPWebserver
{
    public class HTTPServer
    {
        private readonly int _port;
        public int Port { get { return _port; } }

        private StreamSocketListener listener;
        private DataWriter _writer;

        public delegate void DataRecived(string data);
        public event DataRecived OnDataRecived;

        public delegate void Error(string message);
        public event Error OnError;

        private string page;

        public HTTPServer(string page, int port)
        {
            _port = port;
            this.page = page;
        }

        public async Task<string> getHTMLAsync()
        {
            StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile sampleFile = await storageFolder.GetFileAsync(page);
            string text = await FileIO.ReadTextAsync(sampleFile);
            return text;
        }

        public async void Start()
        {
            try
            {
                //Fecha a conexão com a porta que está escutando atualmente
                if (listener != null)
                {
                    await listener.CancelIOAsync();
                    listener.Dispose();
                    listener = null;
                }

                //Criar uma nova instancia do listerner
                listener = new StreamSocketListener();

                //Adiciona o evento de conexão recebida ao método Listener_ConnectionReceived
                listener.ConnectionReceived += Listener_ConnectionReceived;
                //Espera fazer o bind da porta
                await listener.BindServiceNameAsync(Port.ToString());
                Debug.WriteLine("listening");
            }
            catch (Exception e)
            {
                //Caso aconteça um erro, dispara o evento de erro
                if (OnError != null)
                    OnError(e.Message);
                Debug.WriteLine("error listen");
            }
        }
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.WriteLine("connected to " + args.Socket.Information.RemoteAddress + " my ip: " + args.Socket.Information.LocalAddress);
            var reader = new DataReader(args.Socket.InputStream);
            _writer = new DataWriter(args.Socket.OutputStream);
            string data = "";
            bool error = false;
            while (true)
            {
                Task<uint> load_task = reader.LoadAsync(1).AsTask();
                bool finished = !load_task.Wait(4000);
                if (!finished)
                {
                    if (load_task.Result == 0)
                    {
                        Debug.WriteLine("disconnected :-(");
                        error = true;
                        break;
                    }
                    data += reader.ReadString(load_task.Result);
                    
                   
                }
                else
                {
                    break;
                }
            }
            if (!error)
                OnDataRecived(data);
            else
                OnError("Disconnected during data transfer.");
        }
        public void configure(string data)
        {
            
        }
        public async void Send(string message)
        {
            if (_writer != null)
            {
                //Envia a string em si
                _writer.WriteString(message);

                try
                {
                    //Faz o Envio da mensagem
                    await _writer.StoreAsync();
                    //Limpa para o proximo envio de mensagem
                    await _writer.FlushAsync();
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