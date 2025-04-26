namespace IPK
{
    /// <summary>
    /// This class is used for containing all data from arguments, with its default values
    /// </summary>
    public class InputData
    {
        public static string ProtocolType { get; set; } = string.Empty; //Should be given by user
        public static string Server { get; set; } = string.Empty; //User should give Server IP or Host
        public static ushort ServerPort { get; set; } = 4567; //standard port for server
        public static ushort Timeout { get; set; } = 250; //in milliseconds
        public static byte Retries { get; set; } = 3; //count of retries for sending a packet
    }
}