using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmiumScripter.Net
{
    public static class BinaryPacketByteSeralizer
    {
        public static byte[] StructArrayToBytes<T>(T[] structArray) where T : struct
        {
            int totalSize = structArray.Length * Marshal.SizeOf<T>();
            byte[] byteArray = new byte[totalSize];
            MemoryMarshal.Cast<T, byte>(structArray).CopyTo(byteArray);
            return byteArray;
        }
        static byte[] StructToBytes<T>(T data) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            byte[] byteArray = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(data, ptr, false);
                Marshal.Copy(ptr, byteArray, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return byteArray;
        }

        // Converts byte[] back to struct
        public static T BytesToStruct<T>(byte[] data) where T : struct
        {
            int size = Marshal.SizeOf<T>();
            if (data.Length < size)
                throw new ArgumentException($"Byte array too small for struct {typeof(T).Name}");

            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(data, 0, ptr, size);
                return Marshal.PtrToStructure<T>(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static byte[] SerializeDataPacket(DataPacket data)
        {
            byte[] d = StructArrayToBytes(data.Values);
            int size = 6 + d.Length;
            byte[] bytes = new byte[size]; // 4 (int) + 4 (float) + 2 (short)
            Buffer.BlockCopy(BitConverter.GetBytes(data.RollingCounter), 0, bytes, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(data.CommandSpecifier), 0, bytes, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(data.DataCount), 0, bytes, 4, 2);
            Buffer.BlockCopy(d, 0, bytes, 6, d.Length);  
            return bytes;
        }

        public static byte[] SerializeDescriptorList(ValueDescriptorList data)
        {
            String jsonString = JsonSerializer.Serialize(data);
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
            //byte[] d = StructArrayToBytes(data.Values);
            int size = 6 + jsonBytes.Length;
            byte[] bytes = new byte[size]; // 4 (int) + 4 (float) + 2 (short)
            Buffer.BlockCopy(BitConverter.GetBytes(data.RollingCounter), 0, bytes, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(data.CommandSpecifier), 0, bytes, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(data.DataCount), 0, bytes, 4, 2);
            Buffer.BlockCopy(jsonBytes, 0, bytes, 6, jsonBytes.Length);
            return bytes;
        }
        public static byte[] SerializeMessageCounter(MessageCounter data)
        {

            byte[] d = StructToBytes(data);
            int size = d.Length;
            byte[] bytes = new byte[size]; // 4 (int) + 4 (float) + 2 (short)
            Buffer.BlockCopy(BitConverter.GetBytes(data.RollingCounter), 0, bytes, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(data.CommandSpecifier), 0, bytes, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(data.DataCount), 0, bytes, 4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(data.DataCount), 0, bytes, 6, 2);
            return bytes;
        }


        // Deserializes byte[] back into ValueDescriptorList
        public static ValueDescriptorList DeserializeDescriptorList(byte[] data)
        {
            int jsonSize = data.Length - 6;
            string jsonString = Encoding.UTF8.GetString(data, 6, jsonSize);
            return JsonSerializer.Deserialize<ValueDescriptorList>(jsonString);
        }
        public static DataPacket DeserializeDataPacket(byte[] data, out EpochValue[] values)
        {
            DataPacket packet = new DataPacket
            {
                RollingCounter = BitConverter.ToUInt16(data, 0),
                CommandSpecifier = BitConverter.ToUInt16(data, 2),
                DataCount = BitConverter.ToUInt16(data, 4),
            };

            // Anzahl der EpochValue-Einträge berechnen
            int structSize = Marshal.SizeOf<EpochValue>();
            int valueCount = (data.Length - 6) / structSize;

            values = new EpochValue[valueCount];

            // Werte extrahieren
            MemoryMarshal.Cast<byte, EpochValue>(data.AsSpan(6)).CopyTo(values);

            return packet;
        }

        // Deserializes byte[] back into MessageCounter
        public static MessageCounter DeserializeMessageCounter(byte[] data)
        {
            return BytesToStruct<MessageCounter>(data);
        }


        // Extension methods for easier conversion
        public static byte[] ToBytes(this DataPacket data) => SerializeDataPacket(data);
        public static byte[] ToBytes(this MessageCounter data) => SerializeMessageCounter(data);
        public static byte[] ToBytes(this ValueDescriptorList data) => SerializeDescriptorList(data);
       


    }
}
