using System.Net.Sockets;

namespace IPK
{
    /// <summary>
    /// This class is needed for processing C-c situation => terminating connection, but before it should have sent BYE message.
    /// </summary>
    public class ExitCc
    {
        /// <summary>
        /// Tcp variant that sends Bye message.
        /// </summary>
        /// <param name="stream"> Tcp stream that is used for sending data. </param>
        /// <param name="clientData"> Client data that are needed for sending a packet. </param>
        public static async Task SendBye(NetworkStream stream, ClientData clientData)
        {
            await ClientTCP.SendBye(stream, clientData.DisplayName);
        }
        /// <summary>
        /// Udp variant that sends Bye message.
        /// Due to properties of Udp, it should also do retransmission if needed to be sure that message is receieved, and also await for CONFIRM message.
        /// </summary>
        /// <param name="udpClient"> UdpClient that is used for sending data. </param>
        /// <param name="clientData"> Client data that are needed for sending a packet. </param>
        /// <param name="signal"> Signal that are needed to indicate that CONFIRM message is received. </param>
        /// <exception cref="ErrorException"> Exception that is sent if no CONFIRM message is received => connection is terminated by server non-gracefully =>that's error. </exception>
        public static async Task SendBye(UdpClient udpClient, ClientData clientData, AsyncManualResetEvent signal)
        {
            int attempt = 0;
            while (attempt <= InputData.Retries)
            {
                Task timeoutTask = Task.Delay(InputData.Timeout);
                Task sending = ClientUDP.SendBye(udpClient, clientData.DisplayName, signal);

                Task completedTask = await Task.WhenAny(sending, timeoutTask);
                if (completedTask == sending)
                {
                    break;
                }
                _ = Global.DecrementMessageID;
                attempt++;
            }

            if (attempt - 1 == InputData.Retries)
            {
                throw new ErrorException("Failed to send packet to server");
            }
        }
    }
}