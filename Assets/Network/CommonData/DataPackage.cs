namespace Network.CommonData
{
    /// <summary>
    /// 数据包
    /// </summary>
    public class DataPackage
    {
        /// <summary>
        /// 包类型
        /// </summary>
        public ActionTypeEnum PacketType { get; set; }
        /// <summary>
        /// 包数据
        /// </summary>
        public DynamicBuffer Data { get; set; }
        /// <summary>
        /// 包数据
        /// </summary>
        public DataPackage(byte[] bytes, ActionTypeEnum type)
        {
            Data = new DynamicBuffer(bytes);
            PacketType = type;
        }
        /// <summary>
        /// 包数据
        /// </summary>
        public DataPackage(DynamicBuffer buffer, ActionTypeEnum type)
        {
            PacketType = type;
            Data = buffer;
        }
        /// <summary>
        /// 字节数组转化为数据包
        /// </summary>
        public static DataPackage Parse(byte[] bytes)
        {
            DynamicBuffer buffer = new DynamicBuffer(bytes);

            int packetType = 0;
            if (buffer.TryReadInt(ref packetType))
            {
                buffer.DiscardReadBytes();
                return new DataPackage(buffer, (ActionTypeEnum)packetType);
            }
            else
            {
                return null;
            }
        }
    }
}
