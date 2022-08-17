using System;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Linq;
using System.Net;

namespace iShare_Server
{

    public class HandleCallBacks
    {
        readonly private Socket client;
        private Socket PcSocket;
        readonly StreamWriter streamWriter;
        readonly StreamReader streamReader;

        public HandleCallBacks(Socket socket)
        {
            client = socket;
            streamWriter = new StreamWriter(new NetworkStream(client))
            {
                AutoFlush = true
            };
            streamReader = new StreamReader(new NetworkStream(client));

            string data = streamReader.ReadLine();
            Console.Write("\nConnected device is  " + data);

            if (data == "PC")
            {
                AddCredentials();
            }
            else if (data == "MOBILE")
            {
                Start();
            }
            else
            {
                Console.Write("\n Invalid request recieved i.e. " + data);
                client.Close();
            }

        }

        /*    private static Socket ChangePortNum(Socket socket)
            {
                //ToDO: Remove this function from all clients and server
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
                //ToDO: Remove this function from all clients and server
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
            }*/

        void HandleMobile()
        {

            try
            {
                if ((PcSocket = MatchIdPassword()) == null)
                {
                    Console.Write("\nSending ERROR Message to cLient as no Client found for the given credentials");
                    streamWriter.WriteLine("ERROR");
                }
                else
                {
                    streamWriter.WriteLine("SUCCESS");
                    StartCommunication();
                }
               
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

        void AddCredentials()
        {
            string ID = streamReader.ReadLine();
            string Password = streamReader.ReadLine();
            string UniqueID = streamReader.ReadLine();
            string Name = streamReader.ReadLine();
            Console.Write("\nID: " + ID + "\nPassword " + Password +  "\nName" + Name);

            ClientData clientData = new ClientData(client, ID, Password, Name);
            Server.Connections.Add(clientData);
        }
        void StartCommunication()
        {
            Thread MobileToPc = new Thread(() =>
            {
                Communication communicate = new Communication(client, PcSocket);

                //ToDO: check it after testing
                if (communicate.sendMessages())
                {
                    Console.WriteLine("\n communicate.sendMessages() has returned ");
                    //HandleMobile();
                }
            });
            MobileToPc.Start();

            Thread PcToMobile = new Thread(() =>
            {
                Communication communicate = new Communication(PcSocket, client);

                //ToDO: check it after testing

                if (communicate.sendMessages())
                {
                    Console.WriteLine("\n communicate.sendMessages() has returned ");
                   // HandleMobile();
                }
            });
            PcToMobile.Start();
            Console.Write("\n Communication Started");
        }
        Socket MatchIdPassword()
        {
            string ID = streamReader.ReadLine();
            string Password = streamReader.ReadLine();
            Console.Write("\nMobile Credentials are:\n\nID: " + ID + "\nPassword " + Password + "\n");

            Console.Write("\nTotal Objects in arraylist are " + Server.Connections.Count);
            int Count = 0;

            foreach (ClientData clientData in Server.Connections)
            {
                if (clientData.Equal(ID, Password))
                {
                    try
                    {
                        StreamWriter sr = new StreamWriter(new NetworkStream(clientData.GetSocket()));
                        sr.WriteLine("$PING$");
                        sr.Flush();
                        return clientData.GetSocket();

                    }
                    catch (IOException)
                    {
                        Console.Write("\n IOException occured. The client had left. Checking If client is available now or not");
                        Server.Connections.RemoveAt(Count);
                    }
                    catch (Exception e)
                    {
                        Console.Write("\n Exception occured. The z" + e);

                    }
                }
                Count++;
            }
            return null;
        }

        void ConnectByID()
        {
            string ID = streamReader.ReadLine();

            Console.Write("\nMobile Send ID " + ID);
            int Count = 0;

            foreach (ClientData clientData in Server.Connections)
            {
                if (clientData.GetID()==ID)
                {
                    try
                    {
                        StreamWriter sr = new StreamWriter(new NetworkStream(clientData.GetSocket()));
                        sr.WriteLine("$PING$");
                        sr.Flush();
                        streamWriter.WriteLine("SUCCESS");
                        PcSocket= clientData.GetSocket();
                        StartCommunication();
                        return;
                    }
                    catch (IOException)
                    {
                        Console.Write("\n IOException occured. The client had left. Checking If client is available now or not");
                        Server.Connections.RemoveAt(Count);
                    }
                    catch (Exception e)
                    {
                        Console.Write("\n Exception occured. " + e);
                    }
                }
                Count++;
            }
            Console.Write("\nSending ERROR Message to cLient as no Client found for the given Unique ID");
            streamWriter.WriteLine("ERROR");
            Start();
        }

        public void Start()
        {
            while (client.Connected)
            {
                Console.Write("\nServer Waiting for request to make connection");
                try
                {
                    string request = streamReader.ReadLine();
                    Console.Write("\nRecieved (for Connection)" + request + " Request");

                    if (request == null)
                    {
                        Console.Write("\nNull Request. Client Left");
                        client.Close();
                        break;
                    }
                    else if (request==RequestCodes.findByIdPassword)
                    {
                        //HandleMobile();
                        ConnectByID();

                        return;
                        //ToDo: What to do next
                    }else if (request==RequestCodes.findByID)
                    {
                        ConnectByID();
                        return;
                        //ToDo: What to do next
                    }
                   /* else if (request.Equals("driveDirectories"))
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
                   */
                    else
                    {
                        Console.Write("\nNo Action Found for this request!");
                    }

                }
                catch (Exception e)
                {
                    Console.Write("\nException " + e);
                    client.Close();
                    break;
                }

            }
            try
            {
                client.Close();
            }
            catch (Exception)
            {
                //Client was already closed
            }

        }


    }

}
