using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NATP.Server
{
   /* public class NATP_UDP_RelayServerSession : UcpSession, INATP_SignalingServerSender
    {
        private int connectionID;
        INATP_SignalingServerSender clientToHost;
        public NATP_UDP_RelayServerSession(TcpServer server, int connectionID, INATP_SignalingServerSender Sender) : base(server)
        {
            this.clientToHost = Sender;
            this.connectionID = connectionID;  //relayCore = new NATP_RelayServerCore(this); 
        }
        protected override void OnConnected()
        {
            NATP_OnConnected();
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Id {connectionID} disconnected!");
            ServerMessage ssm = new ServerMessage(NATPMethod.ClientDisconnectionResponse);
            ssm.WriteUInt(NATPAttribute.ConnectionID, (uint)connectionID);
            var buffer = ssm.WriteRequest();
            Console.WriteLine("size: " + buffer.Length);
            clientToHost.Send(buffer);
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            Console.WriteLine("Relay OnReceived: " + size);
            byte[] pure = new byte[size + 4];
            byte[] number = BitConverter.GetBytes(connectionID);
            pure[0] = number[3];
            pure[1] = number[2];
            pure[2] = number[1];
            pure[3] = number[0];
            System.Buffer.BlockCopy(buffer, (int)offset, pure, 4, (int)size);
            clientToHost.Send(pure);
            ReceiveAsync();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP session caught an error with code {error}");
        }
        public void NATP_OnConnected()
        {
            Console.WriteLine("Connected: " + connectionID);
            ServerMessage ssm = new ServerMessage(NATPMethod.ConnectionAttemptResponse);
            ssm.WriteUInt(NATPAttribute.ConnectionID, (uint)connectionID);
            var buffer = ssm.WriteRequest();
            Console.WriteLine("size: " + buffer.Length);
            clientToHost.Send(buffer);
        }
    }
    public class NATP_UDP_RelayServer : TcpServer
    {
        private int connectionID = 0x4000;
        INATP_SignalingServerSender Sender;
        private Dictionary<int, Guid> connectionID2SessionID = new Dictionary<int, Guid>();
        private object _lock = new object();
        public readonly int Port;
        public TcpSession FindSession(int connectionID)
        {
            //Console.WriteLine("FindSession: " + connectionID);
            lock (_lock)
            {
                if (connectionID2SessionID.ContainsKey(connectionID))
                    return FindSession(connectionID2SessionID[connectionID]);
                return null;
            }

        }
        public void SendDisconnectEventToAllClients()
        {
            ServerMessage ssm = new ServerMessage(NATPMethod.DisconnectFromServer);
            var buffer = ssm.WriteRequest();
            Multicast(buffer);
        }
        public bool SendDisconnectEventToClient(int connectionID)
        {
            ServerMessage ssm = new ServerMessage(NATPMethod.DisconnectFromServer);
            var buffer = ssm.WriteRequest();
            var session = FindSession(connectionID);
            if (session == null) return false;
            lock (_lock)
            {
                connectionID2SessionID.Remove(connectionID);
                session.SendAsync(buffer);
            }
            return true;
        }
        public NATP_UDP_RelayServer(IPAddress address, int port, INATP_SignalingServerSender sender) : base(address, port) { this.Sender = sender; Port = port; }

        protected override UdpSession CreateSession()
        {
            var sess = new NATP_TCP_RelayServerSession(this, connectionID, Sender);
            connectionID2SessionID[connectionID++] = sess.Id;
            return sess;
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP server caught an error with code {error}");
        }
    }*/
}
