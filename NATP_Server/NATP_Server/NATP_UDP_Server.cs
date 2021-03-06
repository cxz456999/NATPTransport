﻿using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Buffer = System.Buffer;

namespace NATP.Server
{
    public class NATP_UDP_SignalingSession : TcpSession, INATP_SignalingServerSender
    {
        private NATP_ServerCore sigCore;
        public NATP_UDP_SignalingSession(TcpServer server) : base(server) { sigCore = new NATP_ServerCore(this); }
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
            Console.WriteLine($"Chat TCP session with Id {Id} disconnected!");
            sigCore.OnDisconnected();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            sigCore.OnResponse(buffer, offset, size);

        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP session caught an error with code {error}");
        }

        public void NATP_OnConnected()
        {
            Console.WriteLine("IP " + IPAddress.Parse(((IPEndPoint)Socket.RemoteEndPoint).Address.ToString()) + " on port number " + ((IPEndPoint)Socket.RemoteEndPoint).Port.ToString() + " connected!");
            sigCore.RemoteEndPoint = (IPEndPoint)Socket.RemoteEndPoint;
        }
    }
    public class NATP_UDP_Server : TcpServer
    {
        public NATP_UDP_Server(IPAddress address, int port) : base(address, port) { }

        protected override TcpSession CreateSession() { return new NATP_TCP_Session(this); }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP server caught an error with code {error}");
        }
    }
}
