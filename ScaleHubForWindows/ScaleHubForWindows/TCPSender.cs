using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
//using System.Threading;

namespace ScaleHubForWindows
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

        public virtual async Task<bool> Connect(string host)
        {
            return await Connect(host, 10000);
        }

        public virtual async Task<bool> Connect(string host, int timeout)
        {
            try
            {
                Task connectTask = tcp.ConnectAsync(host, port);
                if (await Task.WhenAny(connectTask, Task.Delay(timeout)) == connectTask)
                {
                    nets = tcp.GetStream();
                    return true;
                }
                else{
                    throw new Exception("Destination Unreachable");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return false;
            }
        }



        public virtual async Task Send(string msg)
        {
            byte[] msgAsBytes = Encoding.ASCII.GetBytes(msg); //convert the message into an array of bytes
            byte[] len = BitConverter.GetBytes(msg.Length); //the message length in little-endian
            byte[] len4 = new byte[sizeof(uint)]; //will contain the length in exactly 4 bytes

            Buffer.BlockCopy(len, 0, len4, sizeof(uint) - len.Length, len.Length); //copy len->len4
            Array.Reverse(len4); //convert to big-endian

            byte[] buf = new byte[msgAsBytes.Length + sizeof(uint)]; //the whole data to send
            Buffer.BlockCopy(len4, 0, buf, 0, sizeof(uint));
            Buffer.BlockCopy(msgAsBytes, 0, buf, sizeof(uint), msgAsBytes.Length);

            await nets.WriteAsync(buf, 0, buf.Length); //send the data
            await nets.FlushAsync();
        }

        public async virtual Task<string> Receive()
        {
            byte[] bmsize = new byte[sizeof(uint)];
            byte[] bmsg;

            int msize;
            string msg;

            //nets.Read(bmsize, 0, sizeof(uint)); //reading first 4 bytes (which contains the size of the message)
            await nets.ReadAsync(bmsize, 0, sizeof(uint)); //reading first 4 bytes (which contains the size of the message)
            Array.Reverse(bmsize); //change from big endian to little endian
            msize = BitConverter.ToInt32(bmsize, 0);


            bmsg = new byte[msize];
            await nets.ReadAsync(bmsg, 0, msize); //read the message

            msg = Encoding.ASCII.GetString(bmsg); //convert it to string

            return msg;
        }
    }
}
