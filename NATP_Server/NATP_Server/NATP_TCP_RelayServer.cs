using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Buffer = System.Buffer;

namespace NATP.Server
{
    public class NATP_TCP_RelayServerSession : TcpSession, INATP_SignalingServerSender
    {
        private int connectionID;
        INATP_SignalingServerSender clientToHost;
        public NATP_TCP_RelayServerSession(TcpServer server, int connectionID, INATP_SignalingServerSender Sender) : base(server) 
        {
            this.clientToHost = Sender;
            this.connectionID = connectionID;  //relayCore = new NATP_RelayServerCore(this); 
        }
        #region Encrypt/Decrpty
        NetworkSerializer serializer = new NetworkSerializer(8192);
        public bool EncryptSend(byte[] data)
        {
            return Send(EncryptPackage(data)) > 0;
        }
        public bool EncryptSendAsync(byte[] data)
        {
            return SendAsync(EncryptPackage(data));
        }
        private byte[] EncryptPackage(byte[] data)
        {
            int pkgLength = data.Length;
            serializer.Clear();
            serializer.Write(pkgLength);
            serializer.Write(data);
            return serializer.ToArray();
        }
        private List<byte[]> DecryptPackage(byte[] data, int offset, int size)
        {
            byte[] pureData = new byte[size];
            Buffer.BlockCopy(data, offset, pureData, 0, size);
            return DecryptPackage(pureData);
        }
        private List<byte[]> DecryptPackage(byte[] data)
        {
            serializer.SetBuffer(data, 0, data.Length);
            List<byte[]> packages = new List<byte[]>();
            while (!serializer.IsEnd())
            {
                int pkgLength = serializer.ReadInt();
                packages.Add(serializer.ReadBytes(pkgLength));
            }
            return packages;
        }
        #endregion
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
            //Console.WriteLine("size: " + buffer.Length);
            clientToHost.EncryptSend(buffer);
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            
            var packages = DecryptPackage(buffer, (int)offset, (int)size);
            for (int i = 0; i < packages.Count; i++)
            {
                int pkgSize = packages[i].Length;
                //Console.WriteLine("Relay OnReceived: " + pkgSize);
                byte[] pure = new byte[pkgSize + 4];
                byte[] number = BitConverter.GetBytes(connectionID);
                pure[0] = number[3];
                pure[1] = number[2];
                pure[2] = number[1];
                pure[3] = number[0];
                System.Buffer.BlockCopy(packages[i], 0, pure, 4, pkgSize);
                clientToHost.EncryptSend(pure);
            }
            
            //ReceiveAsync();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"TCP session caught an error with code {error}");
        }
        public void NATP_OnConnected()
        {
            Console.WriteLine("Connected: " + connectionID);
            ServerMessage ssm = new ServerMessage(NATPMethod.ConnectionAttemptResponse);
            ssm.WriteUInt(NATPAttribute.ConnectionID, (uint)connectionID);
            var buffer = ssm.WriteRequest();
            
            if (!clientToHost.EncryptSendAsync(buffer)) Console.WriteLine("ConnectionAttemptResponse: Error");
            //else Console.WriteLine("ConnectionAttemptResponse: Success");
        }
    }
    public class NATP_TCP_RelayServer : TcpServer
    {
        private int connectionID = 100;
        INATP_SignalingServerSender Sender;
        private Dictionary<int, Guid> connectionID2SessionID = new Dictionary<int, Guid>();
        private object _lock = new object();
        public readonly int Port;
        public NATP_TCP_RelayServerSession FindSession(int connectionID)
        {
            //Console.WriteLine("FindSession: " + connectionID);
            lock(_lock)
            {
                if (connectionID2SessionID.ContainsKey(connectionID))
                    return (NATP_TCP_RelayServerSession)FindSession(connectionID2SessionID[connectionID]);
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
        public NATP_TCP_RelayServer(IPAddress address, int port, INATP_SignalingServerSender sender) : base(address, port) { this.Sender = sender; Port = port; }

        protected override TcpSession CreateSession() 
        {
            if (connectionID >= 10000000) connectionID = 0;
            var sess =  new NATP_TCP_RelayServerSession(this, connectionID, Sender); 
            connectionID2SessionID[connectionID++] = sess.Id; 
            return sess; 
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"TCP server caught an error with code {error}");
        }
    }
}
