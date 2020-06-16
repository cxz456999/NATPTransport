using System;
using System.Collections.Generic;
using System.Text;

namespace NATP.Server
{
    public interface INATP_SignalingServerSender
    {
        bool EncryptSend(byte[] data);
        bool EncryptSendAsync(byte[] data);
        bool SendAsync(byte[] buffer);
        long Send(byte[] buffer);

        bool Disconnect();

        void NATP_OnConnected();
    }
}
