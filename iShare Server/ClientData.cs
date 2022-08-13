using System;
using System.Net.Sockets;
using System.Net;

namespace iShare_Server
{
    class ClientData
    {
        Socket PC;
        string ID;
        string Password;
        string UniqueID;
        EndPoint endPoint;
        public ClientData(Socket pC, string iD, string password, string uniqueID)
        {
            PC = pC;
            ID = iD;
            Password = password;
            endPoint = pC.RemoteEndPoint;
            UniqueID = uniqueID;    
        }

        public bool Equal(string iD, string password)
        {
            if (ID == iD && Password == password)
            {
                return true;
            }
            return false;
        }

        public bool SocketExists(EndPoint ip)
        {
            Console.Write("\nComparing " + ip + " and " + PC.RemoteEndPoint);

            return ip == endPoint;
        }

        public Socket GetSocket()
        {
            return PC;
        }
        public string GetUniqueID()
        {
            return UniqueID;
        }

    }

}
