using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace OverloadTester
{
    class Program
    {
        private static int TIMES = 20; //The number of connection requests to send
        private static string IP = "192.168.43.109"; //the IP of the raspberry
        static void Main(string[] args)
        {
            Task.Run(() => MainAsync(args));
            Console.WriteLine("Finished.");
            Console.Read();
        }

        static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Tester Started.");
            //Test I - send a single message
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Test I Started.");
            await testSendOne();

            //Test II - send several messages, but wait for each one to return an answer before sending the next one
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Test II Started.");
            await testSendSerial();

            //Test III - Sending several messages simultaneously and receiving (hopefully) all the answers
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Test III Started.");
            await testSendParallel();

            //Test IV - Like test III, but every message will be resend until received a proper answer
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Test IV Started.");
            await testSendAndResend();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("DONE!");




        }

        //sending one message, and waiting for answer
        public static async Task<DRP> sr1(TCPSender tcps, string ip, DRP msg, int delay=0)
        {
            await tcps.Connect(ip);
            await Task.Delay(delay);
            await tcps.Send(msg.ToString());
            DRP res = DRP.deserializeDRP(await tcps.Receive());

            return res;
        }
        public static async Task<DRP> sr2(TCPSender tcps, string ip, DRP msg, int delay = 0)
        {
            await tcps.Connect(ip);
            await Task.Delay(delay);
            await tcps.Send(msg.ToString());
            DRP res = DRP.deserializeDRP(await tcps.Receive());
            if (res.MessageType != DRPMessageType.IN_USE)
                return res;
            else
                return await sr2(new TCPSender(), ip, msg, delay);
        }

        //sending one connection request
        public static async Task testSendOne()
        {
            TCPSender tcps = new TCPSender();
            DRP msg = new DRP(DRPDevType.APP, "testX", "", "", 0, 0, DRPMessageType.SCANNED);
            DRP results;
            
            results = await sr1(tcps, IP, msg,5000);
            Console.WriteLine("recv: " + results);

            Console.WriteLine("One finished.");
        }

        //sending, waiting for result, then send again
        public static async Task testSendSerial()
        {
            DRP[] msg = new DRP[TIMES];
            DRP[] results = new DRP[TIMES];
            TCPSender[] senders = new TCPSender[TIMES];

            //sending, waiting for result, then send again
            for (int i = 0; i < TIMES; i++)
            {
                msg[i] = new DRP(DRPDevType.APP, "testX", "", "", 0, 0, DRPMessageType.SCANNED);
                senders[i] = new TCPSender();
                results[i] = await sr1(senders[i] ,IP, msg[i],1000);
                Console.WriteLine("msg " + i + " sent");
            }

            for (int i = 0; i < TIMES; i++)
            {
                Console.WriteLine("recv: " + results[i] ?? "didn't return");
            }
            Console.WriteLine("Serial finished.");
        }

        //sending several messages, without waiting for result
        public static async Task testSendParallel()
        {
            DRP[] msg = new DRP[TIMES];
            DRP[] results = new DRP[TIMES];
            Task<DRP>[] tasks = new Task<DRP>[TIMES];
            TCPSender[] senders = new TCPSender[TIMES];
            //sending, waiting for result, then send again

           
            for (int i = 0; i < TIMES; i++)
            {
                msg[i] = new DRP(DRPDevType.APP, "test" + i, "", "", 0, 0, DRPMessageType.SCANNED);
                senders[i] = new TCPSender();
                tasks[i] = sr1(senders[i], IP, msg[i], 0);
                Console.WriteLine("msg " + i + " sent");
            }

            for (int i = 0; i < TIMES; i++)
            {
                if (tasks[i].Wait(1200000))
                {
                    results[i] = tasks[i].Result;
                    Console.WriteLine("recv: " + results[i]);
                }
                else
                {
                    Console.WriteLine("task " + i + " did not return");
                }
                
            }
            Console.WriteLine("Serial finished.");
        }

        //sending several messages, without waiting for result
        public static async Task testSendAndResend()
        {
            DRP[] msg = new DRP[TIMES];
            DRP[] results = new DRP[TIMES];
            Task<DRP>[] tasks = new Task<DRP>[TIMES];
            TCPSender[] senders = new TCPSender[TIMES];
            //sending, waiting for result, then send again


            for (int i = 0; i < TIMES; i++)
            {
                //sid:f5ccac253e6e9ce70bd96a3b9a0b59d2
                msg[i] = new DRP(DRPDevType.APP, "sid:f5ccac253e6e9ce70bd96a3b9a0b59d2", "", "", 0, 0, DRPMessageType.SCANNED);
                senders[i] = new TCPSender();
                tasks[i] = sr2(senders[i], IP, msg[i]);
                Console.WriteLine("msg " + i + " sent");
            }

            for (int i = 0; i < TIMES; i++)
            {
                if (tasks[i].Wait(60000))
                {
                    results[i] = tasks[i].Result;
                    Console.WriteLine("recv: " + results[i]);
                }
                else
                {
                    Console.WriteLine("task " + i + " did not return");
                }

            }
            Console.WriteLine("Resend finished.");
        }
        //sending several messages, without waiting for result. Taking care of ACKS
        public static async Task testSendParallelAndRecognizeACKS()
        {
            Console.WriteLine("To be added.");
        }

    }

}
