namespace IPK
{
    /// <summary>
    /// This class is used for saving all message ids that were received.
    /// It's necessary to know if this message was already processed and showed, so it won't process it again.
    /// </summary>
    public static class ConfirmedPackets
    {
        /// <summary>
        /// It's based on set, so it won't add some messages again, and also because it's HashSet, finding if some message in it is really fast. 
        /// </summary>
        private static readonly HashSet<ushort> Confirmed = [];
        /// <summary>
        /// Adds a packet (its message id) to a set.
        /// </summary>
        /// <param name="packetId"> Packet that should be added. </param>
        public static void Add(ushort packetId)
        {
            Confirmed.Add(packetId);
        }
        /// <summary>
        /// Checks if this packet (its message id) in a set.
        /// </summary>
        /// <param name="packetId"> Packet id, which availability should be checked. </param>
        /// <returns> True - it's in it, false - it isn't </returns>
        public static bool Contains(ushort packetId)
        {
            return Confirmed.Contains(packetId);
        }
        /// <summary>
        /// Because data comes in byte arrays, and we should convert them in ushort (its length of ids), just using BitConverter.ToUInt16 can lead to inconvenience.
        /// So we should check which Endian is used on a machine and then convert it right.
        /// </summary>
        /// <param name="data"> Id that should be converted to ushort. </param>
        /// <returns> Ushort representation of Id. </returns>
        public static ushort TransformEndian(byte[] data)
        {
            ushort value = BitConverter.ToUInt16(data, 0);
            if (BitConverter.IsLittleEndian)
            {
                value = (ushort)((value >> 8) | (value << 8));
            }

            return value;
        }
    }
}