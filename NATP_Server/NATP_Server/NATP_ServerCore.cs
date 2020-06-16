using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
namespace NATP.Server
{
    class NATP_ServerCore
    {
        private static Dictionary<string, INATP_SignalingServerSender> senderTable = new Dictionary<string, INATP_SignalingServerSender>();
        private static List<Room> room = new List<Room>();
        private static int avaliablePort = 0x4000;
        public static Dictionary<string, string> users = new Dictionary<string, string>();
        public static string ExternalIPAddress = "127.0.0.1";

        private NATP_TCP_RelayServer relayServer;

        
        private static object _lock = new object();
        private INATP_SignalingServerSender sender;
        private string clientKey;
        public IPEndPoint RemoteEndPoint;
        
        public NATP_ServerCore(INATP_SignalingServerSender sender)
        {
            this.sender = sender;
        }

        #region API
        public void OnResponse(byte[] buffer, long offset, long size)
        {
            ServerMessage ssM = new ServerMessage();
            NATP_TCP_RelayServerSession session;
            if (!ssM.FromBuffer(buffer, offset, size))
            {
                //Console.WriteLine("Received From Host {0} vs Get: {1}", size, ssM.data.Length);
                session = relayServer.FindSession(ssM.connectionID);
                if (session != null) session.EncryptSend(ssM.data);
                return;
            }
            //Console.WriteLine("\nIncomming Request: {0} ", Enum.GetName(typeof(NATPMethod), ssM.methodType));
            switch (ssM.methodType)
            {
                case NATPMethod.CreateRoomRequest:
                    OnCreateRoom(ssM);
                    break;
                case NATPMethod.CloseRoomRequest:
                    OnCloseRoom(ssM);
                    break;
                case NATPMethod.JoinRoomRequest:
                    OnJoinRoom(ssM);
                    break;
                case NATPMethod.GetRoomListRequest:
                    OnGetRoomList(ssM);
                    break;
                case NATPMethod.DisconnectSpecificClientRequest:
                    relayServer.SendDisconnectEventToClient(ssM.connectionID);
                    break;
                default:
                    
                    break;
            }
        }
        public void OnDisconnected()
        {
            if (relayServer != null) relayServer.SendDisconnectEventToAllClients();
            if (clientKey!=null)CloseRoom(clientKey);
        }
        #endregion

        private bool CloseRoom(string key)
        {     
            lock (_lock)
            {
                int idx = room.FindIndex(x => x.Key == key);
                if (idx >= 0)
                {
                    Console.WriteLine("Close Room: '{0}'", room[idx].ToString());
                    room.RemoveAt(idx);
                    relayServer.Stop();
                    if (senderTable.ContainsKey(key)) senderTable.Remove(key);
                }
                else return false;
            }
            
            return true;
        }
        #region On Events
        private void OnReceivedData(ServerMessage ssm)
        {
            int connectionID;
        }
        private void OnCreateRoom(ServerMessage ssm) 
        {
            string user = (string)ssm.Get(NATPAttribute.User);
            string password = (string)ssm.Get(NATPAttribute.Passowrd);
            if (!users.ContainsKey(user) || users[user] != password)
            { 
                ResponseCreateRoom(false);
                Console.WriteLine("Reject! Error: Wrong Username/Password");
                return;
            }
            string tag = (string)ssm.Get(NATPAttribute.RoomTag);
            string name = (string)ssm.Get(NATPAttribute.RoomName);
            string des = (string)ssm.Get(NATPAttribute.RoomDescription);
            string key = ExternalIPAddress + ":" + avaliablePort;
            relayServer = new NATP_TCP_RelayServer(IPAddress.Any, avaliablePort++, sender);
            relayServer.Start();
            Console.WriteLine("Create Room on port {0}", avaliablePort-1);
            ResponseCreateRoom(true);
            if (tag.Length <= 0)
            {
                Console.WriteLine("Create Room: '[{0}]{1} Failed!'", tag, key);
                ResponseCreateRoom(false);
            }
            lock (_lock)
            {
                int idx = room.FindIndex(x => x.Key == key);
                
                if (idx != -1)
                {
                    Console.WriteLine("Create Room: '[{0}]{1} Failed!'", tag, key);
                    //room[idx].Update(tag);
                    ResponseCreateRoom(false);
                    
                }
                else
                {
                    Room r = new Room(tag, new IPEndPoint(IPAddress.Parse(ExternalIPAddress), relayServer.Port), des, name);
                    room.Add(r);
                    senderTable.Add(key, sender);
                    clientKey = key;
                    
                    ResponseCreateRoom(true);
                    Console.WriteLine("Create Room: " + r.ToString());
                }
            }
        }
        private void OnCloseRoom(ServerMessage ssm) 
        {
            IPEndPoint ipe = (IPEndPoint)ssm.Get(NATPAttribute.RoomAddress);
            CloseRoom(ipe.ToString());
        }
        private void OnJoinRoom(ServerMessage ssm) 
        {
            IPEndPoint roomName = (IPEndPoint)ssm.Get(NATPAttribute.RoomAddress);
            string tag = (string)ssm.Get(NATPAttribute.RoomTag);
            string key = roomName.ToString();
            lock (_lock)
            {
                Room target = room.Find(x => x.Key == key && x.Tag == tag);
                if (target != null)
                {
                    if (senderTable.ContainsKey(key))
                    {
                        ResponseJoinRoom(true);
                        ResponseConnectionAttemptRequest(key, RemoteEndPoint);
                    }
                    else
                        ResponseJoinRoom(false);
                    Console.WriteLine("Peer '{0}' Join Room: '{1}'", RemoteEndPoint.ToString(), key);
                }
                else
                    Console.WriteLine("Peer '{0}' Join Room: '{1}' Failed", RemoteEndPoint.ToString(), key);
            }
        }
        private void OnGetRoomList(ServerMessage ssm)
        {
            ResponseGetRoomList((string)ssm.Get(NATPAttribute.RoomTag));
        }
        private void OnLeaveRoom(ServerMessage ssm) 
        {

        }
        #endregion
        #region Write
        private void ResponseCreateRoom(bool success)
        {
            ServerMessage ssm = new ServerMessage(NATPMethod.CreateRoomResponse);
           
            if (success) ssm.WriteEmpty(NATPAttribute.Success);
            else ssm.WriteEmpty(NATPAttribute.Failed);
            sender.EncryptSend(ssm.WriteRequest());
        }
        private void ResponseJoinRoom(bool success)
        {
            ServerMessage ssm = new ServerMessage(NATPMethod.JoinRoomResponse);
            if (success) ssm.WriteEmpty(NATPAttribute.Success);
            else ssm.WriteEmpty(NATPAttribute.Failed);
            sender.EncryptSend(ssm.WriteRequest());
            //sender.Disconnect();
        }
        private void ResponseGetRoomList(string tag)
        {
            ServerMessage ssm = new ServerMessage(NATPMethod.GetRoomListResponse);
            //
            List<Room> sameTag;
            lock (_lock)
            {
                sameTag = room.FindAll(x => x.Tag == tag);
            }
            //Console.WriteLine("Result Room Tag: {0}, total: {1}", tag, sameTag.Count);
            for (int i = 0; i < sameTag.Count; i++)
            {
                Console.WriteLine("Find: " + sameTag[i].ToString());
                byte[] addressByte = sameTag[i].IP.Address.GetAddressBytes();
                byte[] ip = new byte[3 + addressByte.Length];
                if (addressByte.Length > 4) ip[0] = 0x2;
                else ip[0] = 0x1;
                ushort port = (ushort)sameTag[i].IP.Port;
                ip[2] = (byte)(port & 0xff);
                ip[1] = (byte)((port >> 8) & 0xff);
                Array.Copy(addressByte, 0, ip, 3, addressByte.Length);
                ssm.WriteString(NATPAttribute.RoomName, sameTag[i].Name);
                ssm.WriteString(NATPAttribute.RoomDescription, sameTag[i].Description);
                ssm.WriteBytes(NATPAttribute.RoomAddress, ip);
                Console.WriteLine("Key: " + sameTag[i].Key);
            }
            Console.WriteLine("Result Room Tag: {0}, total: {1}", tag, sameTag.Count);
            sender.EncryptSend(ssm.WriteRequest());
        }
        private void ResponseConnectionAttemptRequest(string key, IPEndPoint ipe)
        {
            ServerMessage ssm = new ServerMessage(NATPMethod.ConnectionAttemptResponse);
            byte[] addressByte = ipe.Address.GetAddressBytes();
            byte[] ip = new byte[3 + addressByte.Length];
            if (addressByte.Length > 4) ip[0] = 0x2;
            else ip[0] = 0x1;
            ushort port = (ushort)ipe.Port;
            ip[2] = (byte)(port & 0xff);
            ip[1] = (byte)((port >> 8) & 0xff);
            Array.Copy(addressByte, 0, ip, 3, addressByte.Length);
            ssm.WriteBytes(NATPAttribute.PeerAddress, ip);
            lock (_lock)
            {
                senderTable[key].EncryptSend(ssm.WriteRequest());
            }
        }
        #endregion
        #region Utilities
        
        private static byte[] IPString2Bytes(string ip)
        {
            return BitConverter.GetBytes(stringToHexIP(ip));
        }
        private static uint stringToHexIP(string strIP)
        {
            uint ip = 0;
            string[] ipseg = strIP.Split('.');
            for (int i = 0; i < ipseg.Length; i++)
            {
                uint first = uint.Parse(ipseg[i]);
                string hexValue = first.ToString("X");
                uint uintAgain = uint.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
                ip = (ip << 8) + uintAgain;
            }
            return ip;
        }
        #endregion
    }
}
