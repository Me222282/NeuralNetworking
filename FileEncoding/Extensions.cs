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
    }
}
