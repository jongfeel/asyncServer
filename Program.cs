using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTcpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 12346);
            listener.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                Accept(listener);
            }
        }

        public async static void Accept(TcpListener listener)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Accepted!");
            NetworkStream networkStream = client.GetStream();

            Task readTask = new Task(async () =>
            {
                while (client.Client.Poll(1, SelectMode.SelectRead) == false)
                {
                    #region Read
                    byte[] readByte = new byte[4096];
                    int count = await networkStream.ReadAsync(readByte, 0, readByte.Length);
                    string message = Encoding.Default.GetString(readByte, 0, count);
                    Console.WriteLine("Read: " + message);
                    #endregion
                }
                networkStream.Close();
                client.Close();

                Console.WriteLine("Client disconnected.");
                Console.WriteLine("Listening...");
            });
            readTask.Start();

            Task writeTask = new Task(async () =>
            {
                while (client.Client.Poll(1, SelectMode.SelectRead) == false)
                {
                    #region Write
                    string echoMessage = Console.ReadLine() + Environment.NewLine;
                    byte [] writeByte = Encoding.Default.GetBytes(echoMessage);
                    await networkStream.WriteAsync(writeByte, 0, writeByte.Length);
                    Console.WriteLine("Write: " + echoMessage);
                    #endregion
                }
            });
            writeTask.Start();
        }
    }
}
