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
        public static bool IsAccepted = false;

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 12346);
            listener.Start();
            
            while (true)
            {
                //if (IsAccepted == false)
                //{
                //    Accept(listener);
                //}
                
                Console.WriteLine("Listening...");

                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Accepted!!");

                IPEndPoint endpoint = client.Client.LocalEndPoint as IPEndPoint;
                Console.WriteLine("Client: " + endpoint?.Address);
                
                client.Close();
            }
        }

        public async static Task<string> ReadLineAsync()
        {
            return await Console.In.ReadLineAsync();
        }

        public async static void Accept(TcpListener listener)
        {
            IsAccepted = true;
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Accepted!");
            IsAccepted = false;

            NetworkStream networkStream = client.GetStream();

            Task readTask = new Task(async () =>
            {
                while (client.Client.Poll(1, SelectMode.SelectRead) == false)
                {
                    #region Read - using StringBuilder
                    byte[] readByte = new byte[4];
                    StringBuilder sb = new StringBuilder();
                    do
                    {
                        int count = await networkStream.ReadAsync(readByte, 0, readByte.Length);
                        sb.Append(Encoding.Default.GetString(readByte, 0, count));
                    } while (networkStream.DataAvailable);
                                        
                    string message = sb.ToString();
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
