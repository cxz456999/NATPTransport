using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NATP
{
    public class NetworkSerializer
    {
        private byte[] buffer;

        public byte[] ByteBuffer { get { return buffer; } }
        public int byteLength = 0; //total size of data 
        public int bytePos = 0; //current read position
        public int byteSent = 0;
        public int byteOffset = 0;
        public NetworkSerializer(int initBufferSize)
        {
            buffer = new byte[initBufferSize];
        }
        public void SetBuffer(byte[] buf, long offset, long size)
        {
            if (size > buffer.Length)
            {
                buffer = buf;
            }
            else
            {
                Buffer.BlockCopy(buf, 0, buffer, (int)offset, (int)size);
            }
            byteLength = (size > 0 && size <= buf.Length) ? (int)size : buf.Length;
            bytePos = (int)offset;
            byteOffset = bytePos;
            byteSent = 0;
        }
        public void SetBufferLength(int length)
        {
            byteLength = length;
            bytePos = 0;
        }
        public void SetStartPos() => bytePos = byteOffset;
        public void ResetBytePos() => bytePos = 0;
        public bool IsEnd() => bytePos >= byteLength;
        public void Clear()
        {
            byteLength = 0; //total size of data 
            bytePos = 0; //current read position
            byteSent = 0;
            byteOffset = 0;
        }
        public byte[] ToArray()
        {
            byte[] arr = new byte[byteLength];
            Buffer.BlockCopy(ByteBuffer, 0, arr, 0, byteLength);
            return arr;
        }

        public void WriteTimestamp()
        {
            long time = System.DateTime.Now.Ticks;
            Write(time);
        }

        public void Write(byte val)
        {
            ByteBuffer[byteLength++] = val;
        }
        public void Write(byte[] val)
        {
            //if (BitConverter.IsLittleEndian)
            //{
            //Array.Reverse(val, 0, val.Length);
            //}
            Buffer.BlockCopy(val, 0, ByteBuffer, byteLength, val.Length);
            //Array.Copy(val, 0, ByteBuffer, byteLength, val.Length);
            byteLength += val.Length;
        }

        public void Overwrite(byte[] val, int start)
        {
            Buffer.BlockCopy(val, 0, ByteBuffer, start, val.Length);
        }

        public void Write(byte[] val, int start, int length)
        {
            Buffer.BlockCopy(val, start, ByteBuffer, byteLength, length);
            byteLength += length;
        }

        public void Write(byte[] val, uint start, uint length)
        {
            Buffer.BlockCopy(val, (int)start, ByteBuffer, byteLength, (int)length);
            byteLength += (int)length;
        }

        public void Write(int val)
        {
            //fixed (byte* b = &ByteBuffer[byteLength])
            //     *((int*)b) = val;

            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, ByteBuffer, byteLength, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 4);
            byteLength += 4;
        }
        public void Write(uint val)
        {
            //fixed (byte* b = &ByteBuffer[byteLength])
            //    *((uint*)b) = val;

            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, ByteBuffer, byteLength, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 4);
            byteLength += 4;
        }
        public void Write(long val)
        {
            //fixed (byte* b = &ByteBuffer[byteLength])
            //    *((long*)b) = val;

            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, ByteBuffer, byteLength, 8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 8);
            byteLength += 8;
        }
        public void Write(ulong val)
        {
            //fixed (byte* b = &ByteBuffer[byteLength])
            //    *((ulong*)b) = val;

            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, ByteBuffer, byteLength, 8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 8);
            byteLength += 8;
        }
        public void Write(short val)
        {
            //fixed (byte* b = &ByteBuffer[byteLength])
            //    *((short*)b) = val;
            /*ByteBuffer[byteLength + 1] = (byte)(val);
            val >>= 8;
            ByteBuffer[byteLength] = (byte)(val);*/
            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, ByteBuffer, byteLength, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 2);
            byteLength += 2;
        }
        public void Write(ushort val)
        {
            //fixed (byte* b = &ByteBuffer[byteLength])
            //    *((ushort*)b) = val;

            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, ByteBuffer, byteLength, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 2);
            byteLength += 2;
        }
        public void Write(float val)
        {
            //fixed (byte* b = &ByteBuffer[byteLength])
            //    *((float*)b) = val;
            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, ByteBuffer, byteLength, 4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 4);
            byteLength += 4;
        }
        public void Write(double val)
        {
            //fixed (byte* b = &ByteBuffer[byteLength])
            //    *((double*)b) = val;
            Buffer.BlockCopy(BitConverter.GetBytes(val), 0, ByteBuffer, byteLength, 8);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, byteLength, 8);
            byteLength += 8;
        }
        public void WriteASCII(string val)
        {
            Write((ushort)val.Length);
            Write(Encoding.ASCII.GetBytes(val));
        }
        public void Write(string val)
        {
            byte[] utfBytes = Encoding.UTF8.GetBytes(val);
            Write((ushort)utfBytes.Length);
            Write(utfBytes);

        }

        public void Write(string val, int len)
        {
            byte[] utfBytes = Encoding.UTF8.GetBytes(val);
            Write((ushort)len);
            Write(utfBytes);

        }
        public void ReverserBuffer(int offset, int size)
        {
            if (size > 0 && offset >= 0 && size + offset < byteLength)
                Array.Reverse(ByteBuffer, offset, size);
        }
        public long ReadTimestamp()
        {
            long time = ReadLong();// BitConverter.ToInt64(ByteBuffer, bytePos);

            return time;
        }

        public int ReadInt()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 4);
            int val = BitConverter.ToInt32(ByteBuffer, bytePos);

            bytePos += 4;
            return val;
        }
        public uint ReadUInt()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 4);
            uint val = BitConverter.ToUInt32(ByteBuffer, bytePos);
            bytePos += 4;
            return val;
        }
        public long ReadLong()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 8);
            long val = BitConverter.ToInt64(ByteBuffer, bytePos);
            bytePos += 8;
            return val;
        }
        public ulong ReadULong()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 8);
            ulong val = BitConverter.ToUInt64(ByteBuffer, bytePos);
            bytePos += 8;
            return val;
        }
        public short ReadShort()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 2);
            short val = BitConverter.ToInt16(ByteBuffer, bytePos);
            bytePos += 2;

            return val;
        }
        public ushort ReadUShort()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 2);
            ushort val = BitConverter.ToUInt16(ByteBuffer, bytePos);
            bytePos += 2;
            return val;
        }

        public float ReadFloat()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 4);
            float val = BitConverter.ToSingle(ByteBuffer, bytePos);
            bytePos += 4;
            return val;
        }
        public double ReadDouble()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ByteBuffer, bytePos, 8);
            double val = BitConverter.ToDouble(ByteBuffer, bytePos);
            bytePos += 8;
            return val;
        }

        public string ReadStringASCII()
        {
            int cnt = ReadUShort();
            string result = Encoding.ASCII.GetString(ByteBuffer, bytePos, cnt);
            bytePos += cnt;
            return result;
        }

        public string ReadString()
        {
            int cnt = ReadUShort();
            string result = Encoding.UTF8.GetString(ByteBuffer, bytePos, cnt);
            bytePos += cnt;
            return result;
        }
        public string ReadString(int cnt)
        {
            string result = Encoding.UTF8.GetString(ByteBuffer, bytePos, cnt);
            bytePos += cnt;
            return result;
        }

        public byte ReadByte()
        {
            return ByteBuffer[bytePos++];
        }

        /*public byte[] ReadBytes(int len)
        {
            byte[] result = new byte[len];
            uint startPos = bytePos;
            uint endPos = bytePos + len;
            for (int i = startPos; i < endPos; i++)
            {
                result[i - startPos] = ByteBuffer[bytePos++];
            }
            return result;
        }*/
        public byte[] ReadBytes(int len)
        {
            byte[] result = new byte[len];
            int startPos = bytePos;
            int endPos = bytePos + len;
            for (int i = startPos; i < endPos; i++)
            {
                result[i - startPos] = ByteBuffer[bytePos++];
            }
            return result;
        }

        public byte[] ReadBytes()
        {
            byte cnt = ByteBuffer[bytePos++];

            byte[] result = new byte[cnt];
            int startPos = bytePos;
            int endPos = bytePos + cnt;
            for (int i = startPos; i < endPos; i++)
            {
                result[i - startPos] = ByteBuffer[bytePos++];
            }
            return result;
        }


        public static string ByteArrayToHexString(byte[] Bytes)
        {
            StringBuilder Result = new StringBuilder(Bytes.Length * 2);
            string HexAlphabet = "0123456789ABCDEF";

            foreach (byte B in Bytes)
            {
                Result.Append(HexAlphabet[(int)(B >> 4)]);
                Result.Append(HexAlphabet[(int)(B & 0xF)]);
            }

            return Result.ToString();
        }
    }

}