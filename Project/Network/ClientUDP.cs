using System.Net.Sockets;
using System.Text;

namespace IPK
{
    /// <summary>
    /// In this class is defined methods that are used to create and send messages in UDP.
    /// </summary>
    public class ClientUDP
    {
        /// <summary>
        /// User should give IP.
        /// </summary>
        public static string serverIp = "";
        /// <summary>
        /// Default port that's given for communication.
        /// </summary>
        public static int serverPort = 4567;
        
        /// <summary>
        /// Checks given data, then creates AUTH packet or throw exception (depends on check of given data), inserts data in a packet and then sends it.
        /// After sending also awaits for CONFIRM message.
        /// This packet is needed to authenticate on a server for further communication.
        /// </summary>
        /// <param name="udpClient"> UdpClient that is used for sending packets. </param>
        /// <param name="Username"> The Username that is never shown, just needed to be authenticated with server. </param>
        /// <param name="DisplayName"> The Name, which's visible for everyone on a server, can be changed for a certain user. </param>
        /// <param name="Secret"> Password for accessing server. </param>
        /// <param name="signal"> This signal is set when CONFIRM is received.
        /// By using it, we ensure that connection stays reliable, if we don't get CONFIRMMed, we think that connection is closed. </param>
        /// <exception cref="FormatingException"> Thrown if there is a problem with given data - it's not meets constraints. </exception>
        public static async Task SendAuth(UdpClient udpClient, string Username, string DisplayName, string Secret, AsyncManualResetEvent signal)
        {
            if (MessageCheck.Check(Username, MsgIdentifiers.Username) == ReturnCode.Success &&
                MessageCheck.Check(DisplayName, MsgIdentifiers.DisplayName) == ReturnCode.Success &&
                MessageCheck.Check(Secret, MsgIdentifiers.Secret) == ReturnCode.Success)
            {
                List<byte> message =
                [
                    // Code of message 
                    (byte)Code.Auth,
                    // MessageID 
                    .. BitConverter.GetBytes(Global.MessageID).Reverse(),
                    // Username
                    .. Encoding.ASCII.GetBytes(Username),
                    // Byte separator
                    0,
                    // DisplayName
                    .. Encoding.ASCII.GetBytes(DisplayName),
                    // Byte separator
                    0,
                    // Secret
                    .. Encoding.ASCII.GetBytes(Secret),
                    // Byte separator
                    0,
                ];
                byte[] data = message.ToArray();
                await udpClient.SendAsync(data, data.Length, serverIp, serverPort);
                await signal.WaitAsync();
                signal.Reset();
            }
            else
            {
                throw new FormatingException("Invalid data for AUTH message.");
            }
        }

        /// <summary>
        /// Checks given data, then creates JOIN packet or throw exception (depends on check of given data), inserts data in a packet and then sends it.
        /// This packet is needed to join desired by user channel.
        /// </summary>
        /// <param name="udpClient"> UdpClient that is used for sending packets. </param>
        /// <param name="ChannelID"> ID of a channel where user wants to join. </param>
        /// <param name="DisplayName"> The Name, which's visible for everyone on a server, can be changed for a certain user. </param>
        /// <param name="signal"> This signal is set when CONFIRM is received.
        /// By using it, we ensure that connection stays reliable, if we don't get CONFIRMMed, we think that connection is closed. </param>
        /// <exception cref="FormatingException"> Thrown if there is a problem with given data - it's not meets constraints. </exception>
        public static async Task SendJoin(UdpClient udpClient, string ChannelID, string DisplayName,
            AsyncManualResetEvent signal)
        {
            if (MessageCheck.Check(ChannelID, MsgIdentifiers.ChannelID) == ReturnCode.Success &&
                MessageCheck.Check(DisplayName, MsgIdentifiers.DisplayName) == ReturnCode.Success)
            {
                List<byte> message =
                [
                    // Code of message 
                    (byte)Code.Join,
                    // MessageID 
                    .. BitConverter.GetBytes(Global.MessageID).Reverse(),
                    // channelID
                    .. Encoding.ASCII.GetBytes(ChannelID),
                    // Byte separator
                    0,
                    // DisplayName
                    .. Encoding.ASCII.GetBytes(DisplayName),
                    // Byte separator
                    0,
                ];
                byte[] data = message.ToArray();
                await udpClient.SendAsync(data, data.Length, serverIp, serverPort);
                await signal.WaitAsync();
                signal.Reset();
            }
            else
            {
                throw new FormatingException("Invalid data for JOIN message.");
            }
        }

        /// <summary>
        /// Checks given data, then creates ERR packet or throw exception (depends on check of given data), inserts data in a packet and then sends it.
        /// This packet is needed for sending errors that occurred during communication with the server.
        /// </summary>
        /// <param name="udpClient"> UdpClient that is used for sending packets. </param>
        /// <param name="DisplayName"> The Name, which's visible for everyone on a server, can be changed for a certain user. </param>
        /// <param name="MessageContent"> Data of a message that will be displayed on a server. </param>
        /// <param name="signal"> This signal is set when CONFIRM is received.
        /// By using it, we ensure that connection stays reliable, if we don't get CONFIRMMed, we think that connection is closed. </param>
        /// <exception cref="FormatingException"> Thrown if there is a problem with given data - it's not meets constraints. </exception>
        public static async Task SendErr(UdpClient udpClient, string DisplayName, string MessageContent,
            AsyncManualResetEvent signal)
        {
            if (MessageCheck.Check(DisplayName, MsgIdentifiers.DisplayName) == ReturnCode.Success &&
                MessageCheck.Check(MessageContent, MsgIdentifiers.MessageContent) == ReturnCode.Success)
            {
                if (MessageContent.Length>60000)
                    MessageContent = MessageContent.Substring(0,60000);
                List<byte> message =
                [
                    // Code of message 
                    (byte)Code.Err,
                    // MessageID 
                    .. BitConverter.GetBytes(Global.MessageID).Reverse(),
                    // DisplayName
                    .. Encoding.ASCII.GetBytes(DisplayName),
                    // Byte separator
                    0,
                    // MessageContent
                    .. Encoding.ASCII.GetBytes(MessageContent),
                    // Byte separator
                    0,
                ];
                byte[] data = message.ToArray();
                await udpClient.SendAsync(data, data.Length, serverIp, serverPort);
                await signal.WaitAsync();
                signal.Reset();
            }
            else
            {
                throw new FormatingException("Invalid data for ERR message.");
            }
        }

        /// <summary>
        /// Checks given data, then creates BYE packet or throw exception (depends on check of given data), inserts data in a packet and then sends it.
        /// This packet is needed for ending connection with server, program implicitly sending it if a user presses C-c or C-d. 
        /// </summary>
        /// <param name="udpClient"> UdpClient that is used for sending packets. </param>
        /// <param name="DisplayName"> The Name, which's visible for everyone on a server, can be changed for a certain user. </param>
        /// <param name="signal"> This signal is set when CONFIRM is received.
        /// By using it, we ensure that connection stays reliable, if we don't get CONFIRMMed, we think that connection is closed. </param>
        /// <exception cref="FormatingException"> Thrown if there is a problem with given data - it's not meets constraints. </exception>
        public static async Task SendBye(UdpClient udpClient, string DisplayName, AsyncManualResetEvent signal)
        {
            if (MessageCheck.Check(DisplayName, MsgIdentifiers.DisplayName) == ReturnCode.Success)
            {
                List<byte> message =
                [
                    // Code of message 
                    (byte)Code.Bye,
                    // MessageID 
                    .. BitConverter.GetBytes(Global.MessageID).Reverse(),
                    // DisplayName
                    .. Encoding.ASCII.GetBytes(DisplayName),
                    // Byte separator
                    0,
                ];
                byte[] data = message.ToArray();
                await udpClient.SendAsync(data, data.Length, serverIp, serverPort);
                await signal.WaitAsync();
                signal.Reset();
            }
            else
            {
                throw new FormatingException("Invalid data for BYE message.");
            }
        }

        /// <summary>
        /// Checks given data, then creates MSG packet or throw exception (depends on check of given data), inserts data in a packet and then sends it.
        /// This packet is needed for sending a message - the main source of communication with other users, not server.
        /// </summary>
        /// <param name="udpClient"> UdpClient that is used for sending packets. </param>
        /// <param name="DisplayName"> The Name, which's visible for everyone on a server, can be changed for a certain user. </param>
        /// <param name="MessageContent"> Data of a message that will be displayed on a server. </param>
        /// <param name="signal"> This signal is set when CONFIRM is received.
        /// By using it, we ensure that connection stays reliable, if we don't get CONFIRMMed, we think that connection is closed. </param>
        /// <exception cref="FormatingException"> Thrown if there is a problem with given data - it's not meets constraints. </exception>
        public static async Task SendMsg(UdpClient udpClient, string DisplayName, string MessageContent,
            AsyncManualResetEvent signal)
        {
            if (MessageCheck.Check(DisplayName, MsgIdentifiers.DisplayName) == ReturnCode.Success &&
                MessageCheck.Check(MessageContent, MsgIdentifiers.MessageContent) == ReturnCode.Success)
            {
                if (MessageContent.Length>60000)
                    MessageContent = MessageContent.Substring(0,60000);
                List<byte> message =
                [
                    // Code of message 
                    (byte)Code.Msg,
                    // MessageID 
                    .. BitConverter.GetBytes(Global.MessageID).Reverse(),
                    // DisplayName
                    .. Encoding.ASCII.GetBytes(DisplayName),
                    // Byte separator
                    0,
                    // MessageContent
                    .. Encoding.ASCII.GetBytes(MessageContent),
                    // Byte separator
                    0,
                ];
                byte[] data = message.ToArray();
                await udpClient.SendAsync(data, data.Length, serverIp, serverPort);
                await signal.WaitAsync();
                signal.Reset();
            }
            else
            {
                throw new FormatingException("Invalid data for MSG message.");
            }
        }

        /// <summary>
        /// Creates CONFIRM packet, inserts data in a packet and then sends it.
        /// This packet is needed for sending the right communication between server and client, to ensure that connection stays reliable.
        /// We need to do it because UDP if stateless unreliable protocol and reliability is our responsibility.
        /// Unlike other methods in this class, it doesn't wait for CONFIRM message - otherwise server and client would be sending only CONFIRM messages between each other.
        /// </summary>
        /// <param name="udpClient"> UdpClient that is used for sending packets. </param>
        /// <param name="retMsgID"> MessageID that this packet CONFIRMS. </param>
        public static async Task SendConfirm(UdpClient udpClient, byte[] retMsgID)
        {
            
            if (MessageCheck.Check(ConfirmedPackets.TransformEndian(retMsgID).ToString(), MsgIdentifiers.MessageID) == ReturnCode.Error)
            {
                throw new FormatingException("Invalid refMsg value for CONFIRM.");
            }
            List<byte> message =
            [
                // Code of message 
                (byte)Code.Confirm,
                // MessageID 
                .. retMsgID
            ];
            byte[] data = message.ToArray();
            await udpClient.SendAsync(data, data.Length, serverIp, serverPort);
        }
    }
}