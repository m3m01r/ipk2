using System.Net.Sockets;
using System.Text;


namespace IPK
{
    /// <summary>
    /// In this class is defined methods that are used to create and send messages in TCP.
    /// </summary>
    public class ClientTCP
    {
        /// <summary>
        /// Checks given data, then creates AUTH packet or throw exception (depends on check of given data), inserts data in a packet and then sends it.
        /// This packet is needed to authenticate on a server for further communication.
        /// </summary>
        /// <param name="stream"> Stream that is used for sending packets. </param>
        /// <param name="Username"> The Username that is never shown, just needed to be authenticated with server. </param>
        /// <param name="DisplayName"> The Name, which's visible for everyone on a server, can be changed for a certain user. </param>
        /// <param name="Secret"> Password for accessing server. </param>
        /// <exception cref="FormatingException"> Thrown if there is a problem with given data - it's not meets constraints. </exception>
        public static async Task SendAuth(NetworkStream stream, string Username, string DisplayName, string Secret)
        {
            if (MessageCheck.Check(Username, MsgIdentifiers.Username) == ReturnCode.Success &&
                MessageCheck.Check(DisplayName, MsgIdentifiers.DisplayName) == ReturnCode.Success &&
                MessageCheck.Check(Secret, MsgIdentifiers.Secret) == ReturnCode.Success)
            {
                byte[] data = Encoding.ASCII.GetBytes($"AUTH {Username} AS {DisplayName} USING {Secret}\r\n");
                await stream.WriteAsync(data, 0, data.Length);
            }
            else
            {
                throw new FormatingException("Invalid data for AUTH command.");
            }
        }
        
        /// <summary>
        /// Checks given data, then creates JOIN a packet or throw exception (depends on check of given data), inserts data in a packet and then sends it.
        /// This packet is needed to join desired by user channel.
        /// </summary>
        /// <param name="stream"> Stream that is used for sending packets. </param>
        /// <param name="ChannelID"> ID of a channel where user wants to join. </param>
        /// <param name="DisplayName"> The Name, which's visible for everyone on a server, can be changed for a certain user. </param>
        /// <exception cref="FormatingException"> Thrown if there is a problem with given data - it's not meets constraints. </exception>
        public static async Task SendJoin(NetworkStream stream, string ChannelID, string DisplayName)
        {
            if (MessageCheck.Check(ChannelID, MsgIdentifiers.ChannelID) == ReturnCode.Success &&
                MessageCheck.Check(DisplayName, MsgIdentifiers.DisplayName) == ReturnCode.Success)
            {
                // List<byte> message =
                byte[] data = Encoding.ASCII.GetBytes($"JOIN {ChannelID} AS {DisplayName}\r\n");
                await stream.WriteAsync(data, 0, data.Length);
            }
            else
            {
                throw new FormatingException("Invalid data for JOIN command.");
            }
        }

        /// <summary>
        /// Checks given data, then creates MSG packet or throw exception (depends on check of given data), inserts data in a packet and then sends it.
        /// This packet is needed for sending a message - the main source of communication with other users, not server.
        /// </summary>
        /// <param name="stream"> Stream that is used for sending packets. </param>
        /// <param name="DisplayName"> The Name, which's visible for everyone on a server, can be changed for a certain user. </param>
        /// <param name="MessageContent"> Data of a message that will be displayed on a server. </param>
        /// <exception cref="FormatingException"> Thrown if there is a problem with given data - it's not meets constraints. </exception>
        public static async Task SendMsg(NetworkStream stream, string DisplayName, string MessageContent)
        {
            if (MessageCheck.Check(DisplayName, MsgIdentifiers.DisplayName) == ReturnCode.Success &&
                MessageCheck.Check(MessageContent, MsgIdentifiers.MessageContent) == ReturnCode.Success)
            {
                if (MessageContent.Length>60000)
                    MessageContent = MessageContent.Substring(0,60000);
                byte[] data = Encoding.ASCII.GetBytes($"MSG FROM {DisplayName} IS {MessageContent}\r\n");
                await stream.WriteAsync(data, 0, data.Length);
            }
            else
            {
                throw new FormatingException("Invalid data for MSG command.");
            }
        }

        /// <summary>
        /// Checks given data, then creates a BYE packet or throw exception (depends on check of given data), inserts data in a packet and then sends it.
        /// This packet is needed for ending connection with server, program implicitly sending it if a user presses C-c or C-d. 
        /// </summary>
        /// <param name="stream"> Stream that is used for sending packets. </param>
        /// <param name="DisplayName"> The Name, which's visible for everyone on a server, can be changed for a certain user. </param>
        /// <exception cref="FormatingException"> Thrown if there is a problem with given data - it's not meets constraints. </exception>
        public static async Task SendBye(NetworkStream stream, string DisplayName)
        {
            if (MessageCheck.Check(DisplayName, MsgIdentifiers.DisplayName) == ReturnCode.Success)
            {
                byte[] data = Encoding.ASCII.GetBytes($"BYE FROM {DisplayName}\r\n");
                await stream.WriteAsync(data, 0, data.Length);
            }
            else
            {
                throw new FormatingException("Invalid data for BYE command.");
            }
        }

        /// <summary>
        /// Checks given data, then creates ERR packet or throw exception (depends on check of given data), inserts data in a packet and then sends it.
        /// This packet is needed for sending errors that occurred during communication with the server.
        /// </summary>
        /// <param name="stream"> Stream that is used for sending packets. </param>
        /// <param name="DisplayName"> The Name, which's visible for everyone on a server, can be changed for a certain user. </param>
        /// <param name="MessageContent"> Data of a message that will be displayed on a server. </param>
        /// <exception cref="FormatingException"> Thrown if there is a problem with given data - it's not meets constraints. </exception>
        public static async Task SendErr(NetworkStream stream, string DisplayName, string MessageContent)
        {
            if (MessageCheck.Check(DisplayName, MsgIdentifiers.DisplayName) == ReturnCode.Success &&
                MessageCheck.Check(MessageContent, MsgIdentifiers.MessageContent) == ReturnCode.Success)
            {
                if (MessageContent.Length>60000)
                    MessageContent = MessageContent.Substring(0,60000);
                byte[] data = Encoding.ASCII.GetBytes($"ERR FROM {DisplayName} IS {MessageContent}\r\n");
                await stream.WriteAsync(data, 0, data.Length);
            }
            else
            {
                throw new FormatingException("Invalid data for ERR command.");
            }
        }
    }
}