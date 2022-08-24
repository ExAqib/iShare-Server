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
                 Start();
            }
            else if (data == "MOBILE")
            {
                string ID = streamReader.ReadLine();
                string name = streamReader.ReadLine();

                ClientData clientData = new ClientData(client, ID, null, name);
                Server.MobileConnections.Add(clientData);
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
                    else if (request == RequestCodes.findByIdPassword)
                    {
                        //HandleMobile();
                        ConnectByID();

                        return;
                    }
                    else if (request == RequestCodes.findByID)
                    {
                        ConnectByID();
                        return;
                    }
                    else if (request == RequestCodes.SendFileToMobile)
                    {
                        string id = streamReader.ReadLine();
                        SendFileToMobile(id, streamWriter);
                    }
                    else if (request == RequestCodes.appVersion)
                    {
                        streamWriter.WriteLine(iShareVersions.iShare_Android_ver);
                    }
                    else if (request == RequestCodes.serverVersion)
                    {
                        streamWriter.WriteLine(iShareVersions.iShare_Server_ver);
                    }
                    else if (request == RequestCodes.desktopVersion)
                    {
                        streamWriter.WriteLine(iShareVersions.iShare_Desktop_ver);
                    }
                    else if (request == RequestCodes.startingSharingData)
                    {
                        PcStatus.PcNotReady = false;
                        return;
                    }
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
            Console.Write("\nID: " + ID + "\nPassword " + Password + "\nName" + Name);

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
                if (clientData.GetID() == ID)
                {
                    try
                    {
                        StreamWriter sr = new StreamWriter(new NetworkStream(clientData.GetSocket()));
                        sr.WriteLine("$PING$");
                        sr.Flush();

                        if (PcStatus.PcCanSendFile)
                        {
                            sr.WriteLine("start_sharing");
                            sr.Flush();

                            PcStatus.PcCanSendFile = false;

                            streamWriter.WriteLine("SUCCESS");
                            PcSocket = clientData.GetSocket();
                            while (PcStatus.PcNotReady)
                            {

                            }
                            PcStatus.PcNotReady = true;
                            StartCommunication();
                        }
                        else
                        {
                            streamWriter.WriteLine("SUCCESS");
                            PcSocket = clientData.GetSocket();
                            StartCommunication();

                        }



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

        public void sendFileMobile(Socket PC, Socket Mobile)
        {

            NetworkStream PcNetworkStream = new NetworkStream(PC);
            NetworkStream MobileNetworkStream = new NetworkStream(Mobile);
            StreamReader streamReader = new StreamReader(PcNetworkStream);

            int bytesReceived = 0;
            int bufferSize = 2048 * 4;
            //int bufferSize = int.Parse(PC_StreamReader.ReadLine());
            Console.Write("\nReceiving file name ");

            string fileName = streamReader.ReadLine();
            Console.Write("\nfileName: " + fileName);

            int totalBytes = int.Parse(streamReader.ReadLine());
            Console.Write("\ntotalBytes : " + totalBytes);

            if (bufferSize + bytesReceived > totalBytes)
            {
                bufferSize = totalBytes - bytesReceived;
                Console.Write("\n Changing buffer size to " + bufferSize);
            }
            byte[] data = new byte[bufferSize];


            StreamWriter streamWriter = new StreamWriter(new NetworkStream(Mobile));

            streamWriter.WriteLine(fileName);
            Console.WriteLine($"\n Sending file name {fileName} to mobile");
            streamWriter.Flush();

            streamWriter.WriteLine(totalBytes);
            Console.WriteLine($"\n Sending totalBytes {totalBytes}");
            streamWriter.Flush();

            int size;
            int loop = 0;

            while (bytesReceived < totalBytes)
            {
                loop++;
                Console.Write("\n\n loop " + loop);

                size = PcNetworkStream.Read(data, 0, bufferSize);
                bytesReceived += size;
                Console.Write("\n Recieved " + bytesReceived + " bytes" + "( " + size + " )");

                MobileNetworkStream.Write(data, 0, size);
                if (bufferSize + bytesReceived > totalBytes)
                {
                    bufferSize = totalBytes - bytesReceived;
                    Console.Write("\n Changing buffer size to " + bufferSize);
                }

            }
            Console.Write("\n transfer done");

        }

        public void SendFileToMobile(string id, StreamWriter PC_Stream)
        {
            Console.WriteLine($"\n id received is {id} ");
            int count = 0;
            ClientData clientData;
            for (int i = 0; i < Server.MobileConnections.Count; i++)
            {
                clientData = (ClientData)Server.MobileConnections[i];
                Console.WriteLine($"\n clientData.GetID() is {clientData.GetID()}");

                if (clientData.GetID() == id)
                {
                    try
                    {
                        //The id that was given by PC for sending file to mobile is found 
                        Console.WriteLine($"\n The id that was given by PC for sending file to mobile is found  ");

                        //Inform mobile to receive file
                        StreamWriter sr = new StreamWriter(new NetworkStream(clientData.GetSocket()));
                        sr.WriteLine("RECEIVE_FILE");
                        sr.Flush();

                        //Inform PC to receive file
                        PC_Stream.WriteLine("found");

                        sendFileMobile(client, clientData.GetSocket());
                        return;

                    }
                    catch (IOException)
                    {
                        Console.Write("\n IOException occured. ");
                        Server.MobileConnections.RemoveAt(i);
                        i--;

                    }
                    catch (Exception e)
                    {
                        Console.Write("\n Exception occured. The z" + e);

                    }
                }

            }
           /* foreach (ClientData clientData in Server.MobileConnections)                    
            {
                Console.WriteLine($"\n clientData.GetID() is {clientData.GetID()}");

                if (clientData.GetID() == id)
                {
                    try
                    {
                        //The id that was given by PC for sending file to mobile is found 
                        Console.WriteLine($"\n The id that was given by PC for sending file to mobile is found  ");

                        PC_Stream.WriteLine("found");
                        StreamWriter sr = new StreamWriter(new NetworkStream(clientData.GetSocket()));

                        //Inform mobile to receive file
                        sr.WriteLine("RECEIVE_FILE");
                        sr.Flush();
                        sendFileMobile(client, clientData.GetSocket());
                        return;

                    }
                    catch (IOException)
                    {
                        Console.Write("\n IOException occured. ");
                        Server.MobileConnections.RemoveAt(count);
                    }
                    catch (Exception e)
                    {
                        Console.Write("\n Exception occured. The z" + e);

                    }
                }
                count++;
            }*/

            //The id that was given by PC for sending file to mobile is not found
            Console.WriteLine($"\nThe id that was given by PC for sending file to mobile is not found  ");
            PC_Stream.WriteLine("not_found");
            PC_Stream.Flush();

        }

    }

}
