using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FileEncoding
{
    internal static class Extensions
    {
        public static unsafe void Write<T>(this Stream stream, T value) where T : unmanaged
            => stream.Write(new ReadOnlySpan<byte>(&value, sizeof(T)));

        public static unsafe T Read<T>(this Stream stream) where T : unmanaged
        {
            Span<byte> data = stackalloc byte[sizeof(T)];
            stream.Read(data);

            return MemoryMarshal.Cast<byte, T>(data)[0];
        }

        public static unsafe void Write(this Stream stream, string value)
        {
            stream.Write(value.Length);

            fixed (char* ptr = &value.AsSpan()[0])
            {
                stream.Write(new ReadOnlySpan<byte>(ptr, sizeof(char) * value.Length));
            }
        }

        public static unsafe string ReadString(this Stream stream)
        {
            int length = stream.Read<int>();

            byte[] data = new byte[sizeof(char) * length];
            stream.Read(data);

            string value;

            fixed (byte* ptr = &data[0])
            {
                value = new string((char*)ptr);
            }

            return value;
        }
    }
}
