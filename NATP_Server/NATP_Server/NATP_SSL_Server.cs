using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using NetCoreServer;

namespace NATP.Server
{
    class NATP_SSL_SignalingSession : SslSession, INATP_SignalingServerSender
    {
        private NATP_ServerCore sigCore;
        public NATP_SSL_SignalingSession(SslServer server) : base(server) { sigCore = new NATP_ServerCore(this); }
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
            serializer.ResetBytePos();
            serializer.Write(pkgLength);
            serializer.Write(data);
            return serializer.ToArray();
        }
        private List<byte[]> DecryptPackage(byte[] data, int offset, int size)
        {
            byte[] pureData = new byte[size];
            Array.Copy(data, offset, pureData, 0, size);
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

        protected override void OnHandshaked()
        {
            Console.WriteLine($"Chat SSL session with Id {Id} handshaked!");
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Chat SSL session with Id {Id} disconnected!");
            sigCore.OnDisconnected();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            sigCore.OnResponse(buffer, offset, size);
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat SSL session caught an error with code {error}");
        }


        public void NATP_OnConnected()
        {
            Console.WriteLine("IP " + IPAddress.Parse(((IPEndPoint)Socket.RemoteEndPoint).Address.ToString()) + " on port number " + ((IPEndPoint)Socket.RemoteEndPoint).Port.ToString() + " connected!");
            sigCore.RemoteEndPoint = (IPEndPoint)Socket.RemoteEndPoint;
        }
    }

    class NATP_SSL_Server : SslServer
    {
        public NATP_SSL_Server(SslContext context, IPAddress address, int port) : base(context, address, port) { }

        protected override SslSession CreateSession() { return new NATP_SSL_SignalingSession(this); }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat SSL server caught an error with code {error}");
        }
    }
}