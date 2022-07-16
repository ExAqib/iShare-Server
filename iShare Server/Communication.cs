using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;

namespace iShare_Server
{
    partial class Program
    {

        public partial class HandleCallBacks
        {
            class Communication
            {
                Socket PC;
                Socket Mobile;
                StreamReader PC_Stream;
                StreamWriter Mobile_Stream;

                EndPoint PcEndPoint;
                EndPoint MobileEndPoint;

                public Communication(Socket pC, Socket mobile)
                {
                    Mobile = mobile;
                    PC = pC;

                    PcEndPoint = PC.RemoteEndPoint;
                    MobileEndPoint = Mobile.RemoteEndPoint;
                    PC_Stream = new StreamReader(new NetworkStream(PC));
                    Mobile_Stream = new StreamWriter(new NetworkStream(Mobile));
                    Mobile_Stream.AutoFlush = true;
                }

                public Boolean sendMessages()
                {
                    while (PC.Connected || Mobile.Connected)
                    {
                        Console.Write("\nWaiting for message");
                        String message;

                        try
                        {
                            message = PC_Stream.ReadLine();

                            Console.Write("\nRECIEVED(" + message + ")MESSAGE");


                            if (message == null)
                            {
                                Console.Write("\n\n\t\t Recived NULL Message. Someone has Left");

                                PcLeft(PcEndPoint);

                                Console.Write("\nInforming other party");

                                InformClient(Mobile_Stream);

                                PC.Close();
                                break;

                            }
                            else if (message == "CLIENT_LEFT_ACKNOWLEDGEMENT")
                            {
                                // TODO: If PC has left, what to do with mobile

                                Console.Write("\n\n\t\t Other party Acknowledged the cliet left message (Making no change to socket)");
                                return false;
                            }
                            else if (message == "CLIENT_LEFT_ACKNOWLEDGEMENT_MOBILE")
                            {
                                Console.Write("\n\n\t\t Other party (MOBILE) Acknowledged the cliet left message (Closisng the socket)");
                                return true;

                            }

                            else if (message == "FileStart")
                            {
                                sendFile();
                            }
                            else if (message == "$RECEIVE_FILE$")
                            {
                                sendFileToPc();
                            }
                            else
                            {
                                try
                                {
                                    Mobile_Stream.WriteLine(message);
                                    Console.Write("\nMessage Send  ");
                                }
                                catch (Exception e)
                                {
                                    Console.Write("\n\nEXCEPTION OCCURED WHILE WRITING. (PC had left) " + e);

                                    Console.Write("\n\n\t\tSomeone has Left. Closing the Socket ");

                                    
                                    Mobile.Close();
                                    break;
                                }

                            }

                        }
                        catch (IOException e)
                        {
                            Console.Write("\n\tIO Exception Occured while READING\n " + e);

                            Console.Write("\n\n\t\tSomeone has Left");
                            PcLeft(PcEndPoint);
                            Console.Write("\nInforming other party");

                            InformClient(Mobile_Stream);
                        
                            PC.Close();
                            break;
                        }



                    }
                    return false;

                }

                private bool PcLeft(EndPoint Ep)
                {
                    int Count = 0;
                    foreach (ClientData clientData in Connections)
                    {
                        if (clientData.SocketExists(Ep))
                        {
                            Console.Write("\nPC has left ");
                            Console.Write("\nlength of array list BEFORE REMOVAL  " + Connections.Count);

                            Connections.RemoveAt(Count);

                            Console.Write("\nlength of array list AFTER REMOVAL  " + Connections.Count);
                            return true;

                        }
                        Count++;
                    }

                    Console.Write("\nMobile has left ");
                    return false;
                }
                private void InformClient(StreamWriter streamWriter)
                {
                    try
                    {
                        streamWriter.WriteLine("CLIENT_LEFT");
                        Console.Write("\nInformed");

                    }
                    catch (IOException e)
                    {
                        Console.Write("\n\nOther Client has also left as IOException has Occured while Informing i.e \n" + e);

                    }
                }
                public void sendFileToPc()
                {
                    Console.Write("\n Recieving file from mobile" );
                    NetworkStream networkStream = new NetworkStream(PC);

                    String FileName=PC_Stream.ReadLine();
                    Console.Write("\n Recieved FileName is " + FileName);

                    int bufferSize = int.Parse(PC_Stream.ReadLine());
                    Console.Write("\n Recieved Buffer Size: " + bufferSize);

                    int totalBytes = int.Parse(PC_Stream.ReadLine());
                    Console.Write("\ntotalBytes : " + totalBytes); 
                    
                    int halfData = int.Parse(PC_Stream.ReadLine());
                    Console.Write("\nhalfData : " + halfData);

                    byte[] buffer = new Byte[bufferSize];

                    int bytesRead=0;
                    int BytesRecieved=0;

                    var fileStream = File.Create("C:\\Users\\aqibn\\Desktop\\abc.pdf");
                    // var fileStream = File.Create("C:\\Users\\aqibn\\Desktop\\"+FileName);
                    //networkStream.CopyTo(fileStream);


                    /*                    String message;
                                       while((message = PC_Stream.ReadLine())!="$File_ENDED")
                                        {
                                            Console.Write("\n" + message);

                                             fileStream.Write(Encoding.ASCII.GetBytes(message));
                                            fileStream.Flush();

                                        }
                    */

                    //  Console.Write("\nSleeping thread for 2 seconds");
                    //  System.Threading.Thread.Sleep(2000);

                    var reader = new BinaryReader(networkStream);
                    int AvaiableData;
                    // reader.ReadBytes(bufferSize);



                    while (BytesRecieved!=totalBytes)
                    {
                        AvaiableData = PC.Available;

                        BytesRecieved += AvaiableData;
                        //networkStream.CopyTo(fileStream);

                        fileStream.Write(reader.ReadBytes(AvaiableData),0, AvaiableData);
                        fileStream.Flush();
                        Console.Write("\n AvaiableData " + AvaiableData + " BytesRecieved  " + BytesRecieved );

                        /* //networkStream.Read(buffer, 0, bufferSize);
                         //data=reader.ReadBytes(bufferSize);
                         //bytesRead += bufferSize;
                         //bytesRead = PC.Receive(data);
                         totalBytesRecieved += bytesRead;

                         Console.Write("\ntotalBytesRecieved  " + totalBytesRecieved + " Bytes Read " + bytesRead +" length of data array is "+ data.Length);


                         if (totalBytes - totalBytesRecieved < bufferSize)
                         {
                             Console.Write("\nAdjusting buff size from  " + bufferSize + "to");
                             bufferSize = totalBytes - totalBytesRecieved;
                             Console.Write("\nto" + bufferSize);
                         }

                        fileStream.Write(data, 0, bytesRead);
                       //  fileStream.Write(data, 0, data.Length);

                         if (bufferSize > 1)
                         {
                             data = new byte[bufferSize];

                         }
 */

                    }

                    /*
                   bytesRead=networkStream.Read(data, 0, bufferSize);
                    totalBytesRecieved = bytesRead;
                    while (totalBytesRecieved != totalBytes)
                    {
                        if (totalBytes - totalBytesRecieved < bufferSize)
                        {
                            Console.Write("\nAdjusting buff size from  " + bufferSize + "to" ) ;
                            bufferSize = totalBytes - totalBytesRecieved;
                            Console.Write("\nto"+bufferSize);

                        }
                        Console.Write("\nBytes Read " + totalBytesRecieved);

                        fileStream.Write(data, 0, bytesRead);
                         data = new byte[bufferSize];
                       bytesRead = networkStream.Read(data, 0, bufferSize);
                        totalBytesRecieved += bytesRead;
                    }*/

                    //fileStream.Write(data, 0, bytesRead);

                    Console.Write("\nFile Closed"  );
                    fileStream.Close();

                    Console.Write("\n transfer done");

                } 
                
                public void sendFile()
                {
                    int bytesReceived = 0;
                    int bufferSize = int.Parse(PC_Stream.ReadLine());
                    Console.Write("\nbuffer Size: " + bufferSize);

                    int totalBytes = int.Parse(PC_Stream.ReadLine());
                    Console.Write("\ntotalBytes : " + totalBytes);

                    byte[] data = new byte[bufferSize];

                    NetworkStream PcNetworkStream = new NetworkStream(PC);
                    NetworkStream MobileNetworkStream = new NetworkStream(Mobile);

                    int size ;
                    int loop = 0;

                    //bufferSize /= 2;


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


            }



        }
    }

}
