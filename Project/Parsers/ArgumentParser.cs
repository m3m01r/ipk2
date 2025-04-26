using System.Net;

namespace IPK
{
    /// <summary>
    ///  This class is used for parsing arguments of a command line executing.
    /// </summary>
    public class ArgumentParser
    {
        /// <summary>
        /// It checks every flag and values after it, saves it, and then checks it mandatory parameters are given. 
        /// </summary>
        /// <param name="args"> Arguments from command line. </param>
        /// <exception cref="ArgumentException"> Thrown if any problem with argument parsing occurs. </exception>
        public static void Parse(string[] args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("No arguments provided. Try -h for help.");
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg == "-t")
                {
                    if (args.Length < i + 1)//if it's shorter, it means there is no data after option.
                    {
                        throw new ArgumentException("Please specify value for a parameter.");
                    }
                    if (args[i + 1] == "tcp" || args[i + 1] == "udp")
                    {
                        InputData.ProtocolType = args[i + 1];
                    }
                    else
                    {
                        throw new ArgumentException("Invalid protocol type. Use 'tcp' or 'udp'.");
                    }

                    i++;
                }
                else if (arg == "-s")
                {
                    if (args.Length < i + 1)//if it's shorter, it means there is no data after option.
                    {
                        throw new ArgumentException("Please specify value for a parameter.");
                    }
                    if (!IPAddress.TryParse(args[i + 1], out IPAddress? argIp)) //we try to parse it like ip
                    {
                        try //it should be a domain if it's not ip
                        {
                            IPHostEntry hostinfo = Dns.GetHostEntry(args[i + 1]);
                            InputData.Server = hostinfo.AddressList[0].ToString();
                        }
                        catch //otherwise it's not an ip
                        {
                            throw new ArgumentException("Write an address parameter.");
                        }
                    }
                    else
                    {
                        //if it's ip, we just set it to server
                        InputData.Server = argIp.ToString();
                    }

                    i++;
                }
                else if (arg == "-p")
                {
                    if (args.Length < i + 1)//if it's shorter, it means there is no data after option.
                    {
                        throw new ArgumentException("Please specify value for a parameter.");
                    }
                    if (ushort.TryParse(args[i + 1], out ushort port))
                    {
                        InputData.ServerPort = port;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid server port. Use a valid port number.");
                    }
                    i++;
                }
                else if (arg == "-d")
                {
                    if (args.Length < i + 1)//if it's shorter, it means there is no data after option.
                    {
                        throw new ArgumentException("Please specify value for a parameter.");
                    }
                    if (ushort.TryParse(args[i + 1], out ushort timeout))
                    {
                        InputData.Timeout = timeout;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid timeout value. Use a valid number.");
                    }

                    i++;
                }
                else if (arg == "-r")
                {
                    if (args.Length < i + 1)//if it's shorter, it means there is no data after option.
                    {
                        throw new ArgumentException("Please specify value for a parameter.");
                    }
                    if (byte.TryParse(args[i + 1], out byte retries))
                    {
                        InputData.Retries = retries;
                    }
                    else
                    {
                        throw new ArgumentException("Invalid number of retries. Use a valid number.");
                    }

                    i++;
                }
                else if (arg == "-h")
                {
                    Console.WriteLine("-------------------------------------");
                    Console.WriteLine("Usage: -t {tcp|udp} -s {server_ip} -p [port] -d [timeout] -r [retries]");
                    Console.WriteLine("Values in braces are required, others are optional.\n");
                    Console.WriteLine("Arguments description:");
                    Console.WriteLine("Argument: -t ");
                    Console.WriteLine("Description: Transport protocol used for connection (Should be given by user)");
                    Console.WriteLine("Values: tcp, udp\n");
                    Console.WriteLine("Argument: -s ");
                    Console.WriteLine("Description: IP address or hostname	Server IP or hostname (Should be given by user)");
                    Console.WriteLine("Values: IP address or hostname\n");
                    Console.WriteLine("Argument: -p ");
                    Console.WriteLine("Description: Server port (default: 4567)");
                    Console.WriteLine("Values: 0-65535\n");
                    Console.WriteLine("Argument: -d ");
                    Console.WriteLine("Description: UDP confirmation timeout (in milliseconds) (default: 250)");
                    Console.WriteLine("Values: 0-65535\n");
                    Console.WriteLine("Argument: -r ");
                    Console.WriteLine("Description: Maximum number of UDP retransmissions (default: 3)");
                    Console.WriteLine("Values: 0-255\n");
                    Console.WriteLine("Argument: -h ");
                    Console.WriteLine("Description: Prints program help output and exits");
                    Console.WriteLine("Values: None");
                    Console.WriteLine("-------------------------------------");
                    Environment.Exit(0);
                }
                else
                {
                    throw new ArgumentException($"Unknown argument: {arg}");
                }
            }
            //checking required arguments
            if (string.IsNullOrEmpty(InputData.ProtocolType) || string.IsNullOrEmpty(InputData.Server))
            {
                throw new ArgumentException("Protocol type and server address are required.");
            }
        }
    }
}