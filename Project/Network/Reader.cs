using System.Net.Sockets;
using System.Text;

namespace IPK
{
    /// <summary>
    /// It's used to read data from Stream in TCP variant and packets of Data in UDP variant.
    /// After receiving a packet, it calls methods to process them, and with return value/exceptions to these methods it processes it.
    /// After it either ends or starts a new cycle of receiving a message.
    /// </summary>
    public class Reader
    {
        /// <summary>
        /// This method is for receiving data in TCP variant.
        /// It uses Stream to receive, then it processes data by Data.Check method,
        /// and depending on the answer, it either ends the program or continues to receive data.
        /// </summary>
        /// <param name="stream"> Used for a receiving packets. </param>
        /// <param name="reply"> When some request message is sent, we need to wait for *REPLY from server, and to be sure that we receive it, we set the timer and await
        /// for signal reply, if it's times out, we sent error, and we end connection - here we set this signal if we receive *REPLY</param>
        /// <param name="error"> When the program is started, it launches three async Tasks,
        /// and one of them is an error handler,
        /// which awaits for this signal, and then sends an error message and the program ends </param>
        /// <exception cref="StateException"> Thrown when some error with packet occurs,
        /// when this class process this exception,
        /// it sets error signal and continues to receive packets. </exception>
        public static async Task Read(NetworkStream stream, AsyncManualResetEvent reply, AsyncManualResetEvent error) /////TCP
        {
            var tempBuffer = new byte[4096];//for receiving and then for creating packets from received data.
            var stringBuffer = new StringBuilder();

            while (true)
            {
                int bytesRead = await stream.ReadAsync(tempBuffer, 0, tempBuffer.Length);
                if (bytesRead == 0)
                {
                    return; // Connection closed
                }

                stringBuffer.Append(Encoding.ASCII.GetString(tempBuffer, 0, bytesRead));
                Code type;
                while (true)
                {
                    string current = stringBuffer.ToString();
                    int msgEnd = current.IndexOf("\r\n", StringComparison.Ordinal);//because TCP can truncate packets in any moment, by searching \r\n we can easily find the end of a packet
                    if (msgEnd == -1)//if in packet isn't \r\n, it means that TCP truncated this, and we should wait for the end of this message, because it's bigger.
                        break;//so we end this cycle and we wait for other packets.

                    string fullMessage = current.Substring(0, msgEnd);
                    stringBuffer.Remove(0, msgEnd + 2);//after getting a packet out of it, we should delete it to free memory.

                    try
                    {
                        type = Data.Check(fullMessage);
                        int FSMreply = FSM.ReadAutomat(type);//depending on return code, we can understand if there is a problem and its type.
                        if (FSMreply == ReturnCode.Error && type == Code.Msg)
                        {
                            throw new StateException("MSG message received before authentication.");
                        }
                        else if (FSMreply == ReturnCode.Error && (type == Code.Reply || type == Code.NotReply))
                        {
                            throw new StateException("*REPLY message received before asking for joining.");
                        }
                        else if (FSMreply == ReturnCode.Error)//if an error or bye message is received.
                        {
                            return;
                        }

                        if (type == Code.Reply || type == Code.NotReply)//if it's a reply, that means that we have sent a request message and another process is waiting for an answer.
                        {
                            reply.Set();
                        }
                    }
                    catch (ErrorException ex)
                    {
                        Console.WriteLine($"ERROR: {ex.Message}");
                        ErrorHandler.ErrorMessage = ex.Message;
                        error.Set();
                        await Task.Delay(1000); //so it won't process anything else if there is an error.
                    }
                    catch (StateException ex)
                    {
                        Console.WriteLine($"ERROR: {ex.Message}");
                        ErrorHandler.ErrorMessage = ex.Message;
                        error.Set();
                        await Task.Delay(1000); //so it won't process anything else if there is an error.
                    }
                }
            } /////TCP
        }
    
        /// <summary>
        /// This method is for receiving data in UDP variant.
        /// It uses UdpClient to receive, then it processes data by Data.Check method,
        /// and depending on the answer, it either ends the program or continues to receive data.
        /// </summary>
        /// <param name="udpClient"> Here we use udpClient for receiving data, we connect it to some dynamic port, that we get from a received message. </param>
        /// <param name="signal"> Because UDP is stateless and unreliable, we should ensure that message is received,
        /// so only if there is a CONFIRM message, we can continue our chat, otherwise it is perceived as a disconnection, and we end the program.  </param>
        /// <param name="reply"> When some request message is sent, we need to wait for *REPLY from server, and to be sure that we receive it, we set the timer and await
        /// for signal reply, if it's times out, we sent error, and we end connection - here we set this signal if we receive *REPLY</param>
        /// <param name="error"> When the program is started, it launches three async Tasks,
        /// and one of them is an error handler,
        /// which awaits for this signal, and then sends an error message and the program ends </param>
        /// <exception cref="StateException"> Thrown when some error with packet occurs,
        /// when this class process this exception,
        /// it sets error signal, and continues to receive packets. </exception>
        public static async Task Read(UdpClient udpClient, AsyncManualResetEvent signal, AsyncManualResetEvent reply,
            AsyncManualResetEvent error) /////UDP
        {
            while (true)
            {
                using MemoryStream buffer = new();
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                ClientUDP.serverPort = result.RemoteEndPoint.Port;
                buffer.Write(result.Buffer, 0, result.Buffer.Length);
                try
                {
                    Code type = Data.Check(buffer.ToArray());
                    int FSMreply = FSM.ReadAutomat(type);
                    
                    if (type != Code.Confirm)//Firstly, we need to confirm a message, even if it's sent in a wrong state, so it won't send it again and again.
                    {
                        await ClientUDP.SendConfirm(udpClient, result.Buffer[1..3]);
                        ConfirmedPackets.Add(ConfirmedPackets.TransformEndian(result.Buffer[1..3]));
                    }
                    if (FSMreply == ReturnCode.Error && type == Code.Msg)
                    {
                        throw new StateException("MSG message received before authentication.");
                    }
                    if (FSMreply == ReturnCode.Error && (type == Code.Reply || type == Code.NotReply))
                    {
                        throw new StateException("*REPLY message received before asking for joining.");
                    }
                    if (FSMreply == ReturnCode.Error)//ERR or BYE received - we should terminate the program.
                    {
                        return;
                    }

                    if (type == Code.Reply || type == Code.NotReply)//we should let a sending process that *REPLY received.
                    {
                        reply.Set();
                    }
                    
                    if (type == Code.Confirm)//we should let a sending process that message is successfully sent.
                    {
                        signal.Set();
                    }
                }
                catch (ErrorException ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");//in a specification of a program, we should firstly show an error message and then process other steps.
                    await ClientUDP.SendConfirm(udpClient, result.Buffer[1..3]);
                    error.Set();
                }
                catch (StateException ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    // await ClientUDP.SendConfirm(udpClient, result.Buffer[1..3]); <- confirm is already sent.
                    ErrorHandler.ErrorMessage = ex.Message;
                    error.Set();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    buffer.Close();
                }
            }
        }
    }
}