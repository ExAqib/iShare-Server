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
        string Name;
        EndPoint endPoint;
        public ClientData(Socket pC, string iD, string password, string name)
        {
            PC = pC;
            ID = iD;
            Password = password;
            endPoint = pC.RemoteEndPoint;
            Name = name;
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

        public string GetName()
        {
            return Name;
        }

        public string GetID() { return ID; }

    }

}
