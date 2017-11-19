using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Widget;
using Android.OS;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace weighJune28
{
    class TCPSender
    {
        private const int DEFAULT_PORT = 9888;
        private int port;
        private TcpClient tcp;
        private NetworkStream nets;

        public int Port
        {
            get
            {
                return port;
            }

            set
            {
                port = value;
            }
        }

        //public int Port { get => port; set => port = value; }

        public TCPSender(int port)
        {
            this.port = port;
            tcp = new TcpClient();
        }
        public TCPSender()
        {
            this.port = DEFAULT_PORT;
            tcp = new TcpClient();
        }

        public virtual bool Connect(string host)
        {
            try
            {
                tcp.Client.Connect(host, port);
                nets = tcp.GetStream();
                return true;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }
        }

        public virtual void Send(string msg)
        {
            byte[] msgAsBytes = Encoding.ASCII.GetBytes(msg); //convert the message into an array of bytes
            byte[] len = BitConverter.GetBytes(msg.Length); //the message length in little-endian
            byte[] len4 = new byte[sizeof(uint)]; //will contain the length in exactly 4 bytes

            Buffer.BlockCopy(len, 0, len4, sizeof(uint) - len.Length, len.Length); //copy len->len4
            Array.Reverse(len4); //convert to big-endian

            byte[] buf = new byte[msgAsBytes.Length + sizeof(uint)]; //the hole data to send
            Buffer.BlockCopy(len4, 0, buf, 0, sizeof(uint));
            Buffer.BlockCopy(msgAsBytes, 0, buf, sizeof(uint), msgAsBytes.Length);

            nets.WriteAsync(buf, 0, buf.Length); //send the data
            nets.FlushAsync();
        }

        public async virtual System.Threading.Tasks.Task<string> Receive()
        {
            byte[] bmsize = new byte[sizeof(uint)];
            byte[] bmsg;

            int msize;
            string msg;

            nets.Read(bmsize, 0, sizeof(uint)); //reading first 4 bytes (which contains the size of the message)
            Array.Reverse(bmsize); //change from big endian to little endian
            msize = BitConverter.ToInt32(bmsize, 0);


            bmsg = new byte[msize];
            nets.Read(bmsg, 0, msize); //read the message

            msg = Encoding.ASCII.GetString(bmsg); //convert it to string

            return msg;
        }
    }
}