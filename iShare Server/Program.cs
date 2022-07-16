using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Net.NetworkInformation;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections;

namespace iShare_Server
{
    partial class Program
    {
        //For Storing all the Connected PC's
        static ArrayList Connections = new ArrayList();
        static void Main(string[] args)
        {
            Console.SetWindowSize(60, 20);

            Console.WriteLine("Server Details ");

            // ConnectClient();
            TcpListener tcpListener = StartServer();
            Socket server;
            int totalClients = 0;

            while (true)
            {
                Console.Write("\n\nServer is listening for Client...");
                server = tcpListener.AcceptSocket();

                totalClients++;
                Console.Write("\n\t\t\tClient " + totalClients + " Connected\n");

                IPEndPoint remoteIpEndPoint = server.RemoteEndPoint as IPEndPoint;

                if (remoteIpEndPoint != null)
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
        private static TcpListener StartServer()
        {
            int PORT_NUM = 9999;
            String IP_ADDRESS = "0.0.0.0";

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
            //TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);         

            tcpListener.Start();
            PORT_NUM = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            Console.Write("\nName: " + hostName + "\nIP Address " + IP_ADDRESS + "\nPort Num:  " + PORT_NUM);

            return tcpListener;
        }

        private static Socket ChangePortNum(Socket socket)
        {
            NetworkStream networkStream = new NetworkStream(socket);
            StreamWriter streamWriter = new StreamWriter(networkStream);

            int PortNum = GetAvailablePort(10000);
            streamWriter.WriteLine(PortNum);
            streamWriter.Flush();

            TcpListener tcpListener = new TcpListener(IPAddress.Any, PortNum);
            tcpListener.Start();
            Socket server = tcpListener.AcceptSocket();
            socket.Close();
            return server;

        }
        public static int GetAvailablePort(int startingPort)
        {
            var portArray = new System.Collections.Generic.List<int>();

            var properties = IPGlobalProperties.GetIPGlobalProperties();

            // Ignore active connections
            var connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            // Ignore active tcp listners
            var endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            // Ignore active UDP listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (var i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;
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

        public partial class HandleCallBacks
        {
            private Socket client;
            private Socket PcSocket;
            StreamWriter streamWriter;
            StreamReader streamReader;

            public HandleCallBacks(Socket socket)
            {
                client = ChangePortNum(socket);
                Console.Write("\nPort number changed");
                streamWriter = new StreamWriter(new NetworkStream(client));
                streamWriter.AutoFlush = true;
                streamReader = new StreamReader(new NetworkStream(client));

                String data = streamReader.ReadLine();
                Console.Write("\nReieved: " + data);

                if (data == "PC")
                {
                    addCredentials();
                }
                else if (data == "MOBILE")
                {
                    HandleMobile();
                }
                else
                {
                    Console.Write("\n Invalid request recieved i.e. " + data);

                }

            }

            void HandleMobile()
            {
                try
                {
                    while ((PcSocket = findPc()) == null)
                    {
                        Console.Write("\nSending ERROR Message to cLient as no Client found for the given credentials");
                        streamWriter.WriteLine("ERROR");
                    }
                    streamWriter.WriteLine("SUCCESS");
                    startCommunication();
                }
                catch (IOException e)
                {
                    Console.Write("\n\n \t\t IOException Occured. Closing the socket \n\n " + e);
                    client.Close();

                }
                catch (Exception e)
                {
                    Console.Write("\n\n \t\t Exception \n\n " + e);

                }
            }

            void addCredentials()
            {
                Dictionary<string, string> Credentials = new Dictionary<string, string>();

                String ID = streamReader.ReadLine();
                String Password = streamReader.ReadLine();
                Console.Write("\nID: " + ID + "\nPassword " + Password);

                Credentials.Add("ID", ID);
                Credentials.Add("Password", Password);

                ClientData clientData = new ClientData(client, ID, Password);
                Connections.Add(clientData);
            }
            void startCommunication()
            {
                Thread MobileToPc = new Thread(() =>
                {
                    Communication communicate = new Communication(client, PcSocket);
                    if (communicate.sendMessages())
                    {
                        HandleMobile();
                    }
                });
                MobileToPc.Start();

                Thread PcToMobile = new Thread(() =>
                {
                    Communication communicate = new Communication(PcSocket, client);
                    if (communicate.sendMessages())
                    {
                        HandleMobile();
                    }
                });
                PcToMobile.Start();
                Console.Write("\n Communication Started");
            }
            Socket findPc()
            {
                String ID = streamReader.ReadLine();
                String Password = streamReader.ReadLine();
                Console.Write("\nMobile Credentials are:\n\nID: " + ID + "\nPassword " + Password + "\n");

                Console.Write("\nTotal Objects in arraylist are "+ Connections.Count);
                int Count= 0;

                foreach (ClientData clientData in Connections)
                {
                    if (clientData.Equal(ID, Password))
                    {
                        try {
                            StreamWriter sr = new StreamWriter(new NetworkStream(clientData.getSocket()));
                            sr.WriteLine("$PING$");
                            sr.Flush();
                            return clientData.getSocket();

                        }
                        catch (IOException)
                        {
                            Console.Write("\n IOException occured. The client had left. Checking If client is available now or not");
                            Connections.RemoveAt(Count);


                        }
                        catch (ObjectDisposedException)
                        {
                            Console.Write("\n ObjectDisposedException occured. The client had left. Checking If client is available now or not");
                            Connections.RemoveAt(Count);

                        }catch (Exception)
                        {
                            Console.Write("\n Exception occured. The client had left. Checking If client is available now or not");

                        }
                    }
                    Count++;
                }

                return null;
            }

            /*  public void start()
              {
                  if (server.Connected)
                  {
                      Console.Write("\nClient connected!");
                      NetworkStream networkStream = new NetworkStream(server);
                      StreamReader streamReader = new StreamReader(networkStream);

                      while (server.Connected)
                      {
                          Console.Write("\nServer Waiting for request");
                          try
                          {
                              String request = streamReader.ReadLine();
                              Console.Write("\nRecieved " + request + " Request");

                              if (request == null)
                              {
                                  Console.Write("\nNull Request. Client Left");
                                  server.Close();
                                  break;
                              }
                              if (request.Equals("driveNames"))
                              {
                                  sendDrives(server);
                              }
                              else if (request.Equals("driveDirectories"))
                              {
                                  //First send driveDirectories message, then drive name (c:/) from client
                                  sendDriveDirectories(server);
                              }
                              else if (request.Equals("subDirectories"))
                              {
                                  sendSubDirectories(server);
                              }
                              else if (request.Equals("downloadFile"))
                              {
                                  SendFile_3(server);
                              }
                              else if (request.Equals("downloadFileFast"))
                              {
                                  SendFile_2(server);
                              }
                              else if (request == null)
                              {
                                  Console.Write("\nNull Request");
                                  Console.Write("\n Client Left");
                                  break;
                              }
                              else
                              {
                                  Console.Write("\nNo Action Found for this request!");
                              }

                          }
                          catch (IOException e)
                          {
                              Console.Write("\nIOEXception Occured i.e " + e);

                          }
                          catch (OutOfMemoryException e)
                          {
                              Console.Write("\nOut of Memory Exception occured " + e);
                              server.Close();
                              break;
                          }
                          catch (Exception e)
                          {
                              Console.Write("\nException " + e);
                          }

                      }
                  }
                  else
                  {
                      Console.Write("\nClient disconnected");
                      server.Close();
                  }
              }*/

            class ClientData
            {
                Socket PC;
                String ID;
                String Password;
                EndPoint endPoint;

                public ClientData(Socket pC, string iD, string password)
                {
                    PC = pC;
                    ID = iD;
                    Password = password;
                    endPoint = pC.RemoteEndPoint;
                }

                public Boolean Equal(string iD, string password)
                {
                    if (ID == iD && Password == password)
                    {
                        return true;
                    }
                    return false;
                }

                public Boolean SocketExists(EndPoint ip)
                {
                    Console.Write("\nComparing "+ip+" and "+PC.RemoteEndPoint);

                    return ip == endPoint;
                }

                public Socket getSocket()
                {
                    return PC;
                }

            }

           

        }
    }

}
