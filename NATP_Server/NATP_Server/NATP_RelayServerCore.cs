using System;
using System.Collections.Generic;
using System.Text;

namespace NATP.Server
{
    class NATP_RelayServerCore
    {
        private INATP_SignalingServerSender sender;

        public NATP_RelayServerCore(INATP_SignalingServerSender nATP_TCP_RelayServer)
        {
            this.sender = nATP_TCP_RelayServer;
        }

        internal void OnDisconnected()
        {
            throw new NotImplementedException();
        }

        internal void OnResponse(byte[] buffer, long offset, long size)
        {
            
        }
    }
}
