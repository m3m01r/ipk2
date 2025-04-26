using System.Text;

namespace IPK
{
    /// <summary>
    ///  This class is used to check received messages and return their code or throw exceptions if there is any problem with them.
    /// </summary>
    public class Data
    {
        ////////TCP
        /// <summary>
        /// Checks type of received data, and invokes corresponding methods. TCP variant.
        /// </summary>
        /// <param name="data"> Received data. </param>
        /// <returns> Code(type) of received message. </returns>
        /// <exception cref="ErrorException"> Thrown if the first byte of received data isn't corresponds to codes of messages given by specification.</exception>
        public static Code Check(string data) //TCP
        {
            if (data.ToUpper().StartsWith("ERR "))
                return Err(data);
            else if (data.ToUpper().StartsWith("REPLY "))
                return Reply(data);
            else if (data.ToUpper().StartsWith("MSG "))
                return Msg(data);
            else if (data.ToUpper().StartsWith("BYE "))
                return Bye(data);
            else
                throw new ErrorException("Bad type of message received");
        }
        /// <summary>
        /// Checks structure and content of ERROR message that should be due to specification. TCP variant.
        /// </summary>
        /// <param name="data"> Received data without code. </param>
        /// <returns> Return code of received data. </returns>
        /// <exception cref="ErrorException"> Thrown if a packet does not match specification - it's a malformed packet </exception>
        static Code Err(string data)
        {
            string[] parts = data.Split(' ');
            if (parts.Length < 5)
                throw new ErrorException("Malformed Packet");
            if (parts[1].ToUpper() != "FROM" || parts[3].ToUpper() != "IS")
                throw new ErrorException("Malformed Packet");
            
            if (MessageCheck.Check(string.Join(" ", parts[4..]), MsgIdentifiers.MessageContent) == ReturnCode.Success &&
                MessageCheck.Check(parts[2], MsgIdentifiers.DisplayName) == ReturnCode.Success)
                Console.WriteLine($"ERROR FROM {parts[2]}: {string.Join(" ", parts[4..])}");
            else
                throw new ErrorException("Malformed Packet");
            return Code.Err;
        }
        /// <summary>
        /// Checks structure and content of REPLY message that should be due to specification. TCP variant.
        /// </summary>
        /// <param name="data"> Received data without code. </param>
        /// <returns> Return code of received data. </returns>
        /// <exception cref="ErrorException"> Thrown if a packet does not match specification - it's a malformed packet </exception>
        static Code Reply(string data)
        {
            string[] parts = data.Split(' ');
            string result;
            if (parts.Length < 4)//it should be at least 4 parts.
                throw new ErrorException("Malformed Packet");
            if (parts[2].ToUpper() != "IS")
                throw new ErrorException("Malformed Packet");
            
            if (parts[1].ToUpper() == "OK")
                result = "Success";
            else if (parts[1].ToUpper() == "NOK")
                result = "Failure";
            else
                throw new ErrorException("Malformed Packet");

            if (MessageCheck.Check(string.Join(" ", parts[3..]), MsgIdentifiers.MessageContent) == ReturnCode.Success)
                Console.WriteLine($"Action {result}: {string.Join(" ", parts[3..])}");
            else
                throw new ErrorException("Malformed Packet");

            if (parts[1].ToUpper() == "OK")
                return Code.Reply;
            return Code.NotReply;
        }
        /// <summary>
        /// Checks structure and content of MSG message that should be due to specification. TCP variant.
        /// </summary>
        /// <param name="data"> Received data without code. </param>
        /// <returns> Return code of received data. </returns>
        /// <exception cref="ErrorException"> Thrown if a packet does not match specification - it's a malformed packet </exception>
        static Code Msg(string data)
        {
            string[] parts = data.Split(' ');
            if (parts.Length < 5)//it should be at least 5 parts.
                throw new ErrorException("Malformed Packet");
            if (parts[1].ToUpper() != "FROM" || parts[3].ToUpper() != "IS")
                throw new ErrorException("Malformed Packet");
            
            if (MessageCheck.Check(string.Join(" ", parts[4..]), MsgIdentifiers.MessageContent) == ReturnCode.Success &&
                MessageCheck.Check(parts[2], MsgIdentifiers.DisplayName) == ReturnCode.Success)
                Console.WriteLine($"{parts[2]}: {string.Join(" ", parts[4..])}");
            else
                throw new ErrorException("Malformed Packet");

            return Code.Msg;
        }
        /// <summary>
        /// Checks structure and content of BYE message that should be due to specification. TCP variant.
        /// </summary>
        /// <param name="data"> Received data without code. </param>
        /// <returns> Return code of received data. </returns>
        /// <exception cref="ErrorException"> Thrown if a packet does not match specification - it's a malformed packet </exception>
        static Code Bye(string data)
        {
            string[] parts = data.Split(' ');
            if (parts.Length != 3)//it should be exactly 3 parts.
                throw new ErrorException("Malformed Packet");
            if (parts[1].ToUpper() != "FROM")
                throw new ErrorException("Malformed Packet");
            
            if (MessageCheck.Check(parts[2], MsgIdentifiers.DisplayName) == ReturnCode.Error)
                throw new ErrorException("Malformed Packet");

            return Code.Bye;
        }
        ////////UDP
        /// <summary>
        /// Checks type of received data, and invokes corresponding methods. UDP variant.
        /// </summary>
        /// <param name="data"> Received data. </param>
        /// <returns> Code(type) of received message. </returns>
        /// <exception cref="ErrorException"> Thrown if the first byte of received data isn't corresponds to codes of messages given by specification.</exception>
        public static Code Check(byte[] data) //UDP
        {
            switch ((Code)data[0])
            {
                case Code.Confirm:
                    return Confirm(data[1..]);
                case Code.Reply:
                    return Reply(data[1..]);
                case Code.Msg:
                    return Msg(data[1..]);
                case Code.Ping:
                    return Ping(data[1..]);
                case Code.Err:
                    return Err(data[1..]);
                case Code.Bye:
                    return Bye(data[1..]);
                default:
                    throw new ErrorException("Bad type of message received");
            }
        }
        /// <summary>
        /// Checks structure and content of BYE message that should be due to specification. UDP variant.
        /// </summary>
        /// <param name="data"> Received data without code. </param>
        /// <returns> Return code of received data. </returns>
        /// <exception cref="ErrorException"> Thrown if a packet does not match specification - it's a malformed packet.
        /// Also, if its CONFIRM to a packet that's already confirmed - that's a malformed packet.  </exception>
        static Code Confirm(byte[] data) //only for UDP will be done it later
        {
            if (data.Length != 2)//it should be exactly 2 parts.
                throw new ErrorException("Malformed Packet");
            if (MessageCheck.Check( ConfirmedPackets.TransformEndian(data[0..2]).ToString(), MsgIdentifiers.MessageID) == ReturnCode.Error)
                throw new ErrorException("Malformed Packet");
            return Code.Confirm;
        }
        /// <summary>
        /// Checks structure and content of BYE message that should be due to specification. UDP variant.
        /// </summary>
        /// <param name="data"> Received data without code. </param>
        /// <returns> Return code of received data. </returns>
        /// <exception cref="ErrorException"> Thrown if a packet does not match specification - it's a malformed packet. </exception>
        static Code Reply(byte[] data)
        {
            if (data.Length < 7)//it should be at least 7 parts.
                throw new ErrorException("Malformed Packet");
            if (MessageCheck.Check( ConfirmedPackets.TransformEndian(data[0..2]).ToString(), MsgIdentifiers.MessageID) == ReturnCode.Error)
                throw new ErrorException("Malformed Packet");
            
            if (!ConfirmedPackets.Contains(ConfirmedPackets.TransformEndian(data[0..2])))//if it's in a confirmed packet, that means that we already processed it.
            {
                string result;
                if (data[2] == 0)
                    result = "Failure";
                else if (data[2] == 1)
                    result = "Success";
                else
                    throw new ErrorException("Malformed Packet");//if there is any other value than it doesn't meet the specification.
                byte[] reply = data[5..];
                if (reply[^1] == 0x00)//it should end with 0x00 byte that indicates the end of a part.
                    reply = reply[..^1];//we should cut it so it won't show
                else 
                    throw new ErrorException("Malformed Packet");
                if (MessageCheck.Check(Encoding.ASCII.GetString(reply), MsgIdentifiers.MessageContent) ==
                    ReturnCode.Success)
                    Console.WriteLine($"Action {result}: {Encoding.ASCII.GetString(reply)}");
                else
                    throw new ErrorException("Malformed Packet");
            }
            return data[2] switch
            {
                0 => Code.NotReply,
                _ => Code.Reply
            };
        }
        /// <summary>
        /// Checks structure and content of BYE message that should be due to specification. UDP variant.
        /// </summary>
        /// <param name="data"> Received data without code. </param>
        /// <returns> Return code of received data. </returns>
        /// <exception cref="ErrorException"> Thrown if a packet does not match specification - it's a malformed packet </exception>
        static Code Msg(byte[] data)
        {
            if (data.Length < 6)//it should be at least 6 parts.
                throw new ErrorException("Malformed Packet");
            if (!ConfirmedPackets.Contains(ConfirmedPackets.TransformEndian(data[0..2])))//if it's in a confirmed packet, that means that we already processed it.
            {
                string result = "";
                int i = 2;
                while (data[i] != 0)//it should end with 0x00 byte that indicates the end of a part.
                {
                    result += Encoding.ASCII.GetString([data[i++]]);
                }

                i++;
                byte[] msg = data[i..];
                if (msg[^1] == 0x00)//it should end with 0x00 byte that indicates the end of a part.
                    msg = msg[..^1];
                else 
                    throw new ErrorException("Malformed Packet");
                if (MessageCheck.Check(result, MsgIdentifiers.DisplayName) == ReturnCode.Success &&
                    MessageCheck.Check(Encoding.ASCII.GetString(msg), MsgIdentifiers.MessageContent) ==
                    ReturnCode.Success)
                    Console.Write($"{result}: {Encoding.ASCII.GetString(msg)}\n");
                else
                    throw new ErrorException("Malformed Packet");
            }
            return Code.Msg;
        }
        /// <summary>
        /// Checks structure and content of BYE message that should be due to specification. UDP variant.
        /// </summary>
        /// <param name="data"> Received data without code. </param>
        /// <returns> Return code of received data. </returns>
        /// <exception cref="ErrorException"> Thrown if a packet does not match specification - it's a malformed packet </exception>
        static Code Ping(byte[] data)
        {
            if (data.Length != 2)//it should be exactly 2 parts.
                throw new ErrorException("Malformed Packet");
            return Code.Ping;
        }
        /// <summary>
        /// Checks structure and content of ERR message that should be due to specification. UDP variant.
        /// </summary>
        /// <param name="data"> Received data without code. </param>
        /// <returns> Return code of received data. </returns>
        /// <exception cref="ErrorException"> Thrown if a packet does not match specification - it's a malformed packet </exception>
        static Code Err(byte[] data)
        {
            if (data.Length < 6)//it should be at least 6 parts.
                throw new ErrorException("Malformed Packet");

            if (!ConfirmedPackets.Contains(ConfirmedPackets.TransformEndian(data[0..2])))//if it's in a confirmed packet, that means that we already processed it.
            {
                string result = "";
                int i = 2;
                while (data[i] != 0)//it should end with 0x00 byte that indicates the end of a part.
                {
                    result += Encoding.ASCII.GetString([data[i++]]);
                }

                i++;
                byte[] err = data[i..];
                if (err[^1] == 0x00)//it should end with 0x00 byte that indicates the end of a part.
                    err = err[..^1];
                else
                    throw new ErrorException("Malformed Packet");

                if (MessageCheck.Check(result, MsgIdentifiers.DisplayName) == ReturnCode.Success &&
                    MessageCheck.Check(Encoding.ASCII.GetString(err), MsgIdentifiers.MessageContent) ==
                    ReturnCode.Success)
                    Console.WriteLine($"ERROR FROM {result}: {Encoding.ASCII.GetString(err)}");
                else
                    throw new ErrorException("Malformed Packet");
            }
            return Code.Err;
        }
        /// <summary>
        /// Checks structure and content of BYE message that should be due to specification. UDP variant.
        /// </summary>
        /// <param name="data"> Received data without code. </param>
        /// <returns> Return code of received data. </returns>
        /// <exception cref="ErrorException"> Thrown if a packet does not match specification - it's a malformed packet </exception>
        static Code Bye(byte[] data)
        {
            if (data.Length < 4)//it should be at least 4 parts.
                throw new ErrorException("Malformed Packet");
            if (!ConfirmedPackets.Contains(ConfirmedPackets.TransformEndian(data[0..2])))//if it's in a confirmed packet, that means that we already processed it.
            {
                byte[] bye = data[2..];
                if (bye[^1] == 0x00)//it should end with 0x00 byte that indicates the end of a part.
                    bye = bye[..^1];
                else 
                    throw new ErrorException("Malformed Packet");
                if (MessageCheck.Check(Encoding.ASCII.GetString(bye), MsgIdentifiers.DisplayName) != ReturnCode.Success)
                    throw new ErrorException("Malformed Packet");
            }
            return Code.Bye;
        }
    }
}