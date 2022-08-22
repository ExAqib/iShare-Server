using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;

using System.Collections;

namespace iShare_Server
{
    //For Storing all the Connected PC's
    partial class Server
    {
        public static ArrayList Connections = new ArrayList();
        public static ArrayList MobileConnections = new ArrayList();

        static void Main(string[] args)
        {
            Console.SetWindowSize(60, 20);

            Console.WriteLine("Server Details ");

            // ConnectClient();
            int PORT_NUM = 9999;
            TcpListener tcpListener;

            while (true)
            {
                try
                {
                    tcpListener = StartServer(PORT_NUM);

                    Socket server;
                    int totalClients = 0;

                    while (true)
                    {
                        Console.Write("\n\nServer is listening for Client...");
                        server = tcpListener.AcceptSocket();

                        totalClients++;
                        Console.Write("\n\t\t\tClient " + totalClients + " Connected\n");


                        if (server.RemoteEndPoint is IPEndPoint remoteIpEndPoint)
                        {
                            Console.WriteLine("\n\n\t\t\tClient's IP Address: " + remoteIpEndPoint.Address + "\n\t\t\tClient's Port No. : " + remoteIpEndPoint.Port);
                        }


                        Thread t = new Thread(() =>
                        {
                            HandleCallBacks handle = new HandleCallBacks(server);
                            //handle.start();
                        });
                        t.Start();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("\n Exception Occured i.e " + e);
                    Console.WriteLine("\n  Press 1 to Reconnect or any other key to close");
                    string key = Console.ReadLine();
                    if (key == "1")
                    {
                        continue;
                    }
                    else { Environment.Exit(-1); }
                }

            }


        }
        private static TcpListener StartServer(int PORT_NUM)
        {
            string IP_ADDRESS = "0.0.0.0";
            string hostName = Dns.GetHostName();

            var host = Dns.GetHostEntry(hostName);
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    IP_ADDRESS = ip.ToString();
                }
            }

            TcpListener tcpListener = new TcpListener(IPAddress.Any, PORT_NUM);
            tcpListener.Start();
            PORT_NUM = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            Console.Write("\nName: " + hostName + "\nIP Address " + IP_ADDRESS + "\nPort Num:  " + PORT_NUM);
            return tcpListener;


        }

        private static void ConnectClient()
        {

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            Console.WriteLine("ipaddress..." + ipAddress);

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 0);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);
                Console.WriteLine("\n\nClient's IP Address: " + localEndPoint.Address + "\nClient's Port No. : " + localEndPoint.Port);

                Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.  
                Socket handler = listener.Accept();

                Console.WriteLine("connected...");

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception..." + e);

            }

        }

    }
}
