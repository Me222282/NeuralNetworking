using System;
using System.Collections.Generic;
using System.IO;
using Zene.Structs;
using Zene.Windowing;
using System.IO.Compression;

namespace GenFileLoader
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            // No file to load
            if (args.Length == 0)
            {
                Console.WriteLine("No file loaded.");
                Console.WriteLine("Enter path to file or nothing to exit");

                List<string> paths = new List<string>();
                while (true)
                {
                    string path = Console.ReadLine();

                    // Nothing means exit
                    if (path == null || path == "" && paths.Count == 0) { return; }
                    else if (path == null || path == "")
                    {
                        args = paths.ToArray();
                        break;
                    }

                    if (!File.Exists(path))
                    {
                        Console.WriteLine("Path doesn't exist.");
                        continue;
                    }

                    paths.Add(path);
                    Console.WriteLine("Enter path to file or nothing to execute entered paths");
                }
            }

            Core.Init();

            Window(args);

            Core.Terminate();
        }

        public static void Window(string[] paths)
        {
            byte[][] data = new byte[paths.Length][];

            string text = "";

            for (int i = 0; i < paths.Length; i++)
            {
                text += paths[i] + " - ";

                data[i] = File.ReadAllBytes(paths[i]);
            }

            text = text.Remove(text.Length - 3);

            WindowW window = new WindowW(128 * 6, 128 * 6, text, data);

            window.Run();
        }

        public struct FramePart
        {
            public FramePart(byte r, byte g, byte b, int x, int y, bool a)
            {
                Colour = new Colour(r, g, b);
                Position = new Vector2I(x, y);
                Alive = a;
            }
            public FramePart(Colour c, int x, int y, bool a)
            {
                Colour = c;
                Position = new Vector2I(x, y);
                Alive = a;
            }

            public Colour Colour { get; }
            public Vector2I Position { get; }
            public bool Alive { get; }
        }

        public static unsafe FramePart[,] ImportFrames_old(byte[] data, out int frameCount, out int lifeCount, out int worldSize)
        {
            int readOffset = 0;

            worldSize = BitConverter.ToInt32(data, readOffset);
            readOffset += 4;
            frameCount = BitConverter.ToInt32(data, readOffset);
            readOffset += 4;
            lifeCount = BitConverter.ToInt32(data, readOffset);
            readOffset += 4;

            FramePart[,] frames = new FramePart[frameCount, lifeCount];

            for (int f = 0; f < frameCount; f++)
            {
                for (int l = 0; l < lifeCount; l++)
                {
                    byte r = data[readOffset];
                    readOffset++;
                    byte g = data[readOffset];
                    readOffset++;
                    byte b = data[readOffset];
                    readOffset++;

                    int x = BitConverter.ToInt32(data, readOffset);
                    readOffset += 4;
                    int y = BitConverter.ToInt32(data, readOffset);
                    readOffset += 4;

                    frames[f, l] = new FramePart(r, g, b, x, y, true);
                }
            }

            return frames;
        }

        public static unsafe FramePart[,] ImportFrames(byte[] data, out int frameCount, out int lifeCount, out int worldSize)
        {
            if (!ValidateGenFile(data))
            {
                throw new Exception($"{nameof(data)} doesn't contain a gen file.");
            }

            int dataSize = data[7] switch
            {
                0 => 4,
                1 => 2,
                2 => 1,
                _ => throw new Exception("Invalid data size specifier.")
            };

            // Find worldSize, frameCount and lifeCount
            worldSize = BitConverter.ToInt32(data, 8);
            frameCount = BitConverter.ToInt32(data, 12);
            lifeCount = BitConverter.ToInt32(data, 16);

            int readOffset = 20;

            Span<byte> s;
            fixed (byte* ptr = &data[readOffset])
            {
                s = new Span<byte>(ptr, data.Length - readOffset);
            }
            MemoryStream mem = new MemoryStream(s.ToArray());

            ZipArchive zip = new ZipArchive(mem, ZipArchiveMode.Read, true);
            ZipArchiveEntry entry = zip.GetEntry("data");
            Stream stream = entry.Open();

            Span<Colour> colours = stackalloc Colour[lifeCount];
            for (int i = 0; i < lifeCount; i++)
            {
                int r = stream.ReadByte();
                int g = stream.ReadByte();
                int b = stream.ReadByte();

                colours[i] = new Colour((byte)r, (byte)g, (byte)b);
            }

            FramePart[,] frames = new FramePart[frameCount, lifeCount];

            byte[] currentData = new byte[dataSize * 2];

            for (int f = 0; f < frameCount; f++)
            {
                for (int l = 0; l < lifeCount; l++)
                {
                    stream.Read(currentData, 0, currentData.Length);

                    int x, y;
                    bool alive = true;

                    switch (dataSize)
                    {
                        case 4:
                            // Int
                            x = BitConverter.ToInt32(currentData, 0);
                            y = BitConverter.ToInt32(currentData, 4);

                            if (x == int.MaxValue && y == int.MaxValue) { alive = false; }
                            break;

                        case 2:
                            // Short
                            x = BitConverter.ToInt16(currentData, 0);
                            y = BitConverter.ToInt16(currentData, 2);

                            if (x == short.MaxValue && y == short.MaxValue) { alive = false; }
                            break;

                        case 1:
                            // Byte
                            x = currentData[0];
                            y = currentData[1];

                            if (x == byte.MaxValue && y == byte.MaxValue) { alive = false; }
                            break;

                        default:
                            throw new Exception();
                    }

                    frames[f, l] = new FramePart(colours[l], x, y, alive);
                }
            }

            zip.Dispose();
            mem.Dispose();

            return frames;
        }

        public static bool ValidateGenFile(byte[] data)
        {
            return data[0] == 'Z' &&
                data[1] == 'e' &&
                data[2] == 'n' &&
                data[3] == 'e' &&
                data[4] == 'G' &&
                data[5] == 'e' &&
                data[6] == 'n';
        }

        public enum DataType : byte
        {
            Int = 0,
            Short = 1,
            Byte = 2
        }
        public static unsafe byte[] ExportFrames2(FramePart[,] frameData, int worldSize, DataType type)
        {
            int dataSize = type switch
            {
                DataType.Int => 4,
                DataType.Short => 2,
                DataType.Byte => 1,
                _ => throw new Exception("Invalid data size specifier.")
            };

            int lifeCount = frameData.GetLength(1);
            int frameCount = frameData.GetLength(0);

            int size =
                // Position data
                (frameCount * lifeCount * dataSize * 2) +
                // Colour data
                (lifeCount * 3);

            byte[] data = new byte[size];

            // Colour data
            for (int i = 0; i < lifeCount; i++)
            {
                int di = i * 3;
                data[di] = frameData[0, i].Colour.R;
                data[di + 1] = frameData[0, i].Colour.G;
                data[di + 2] = frameData[0, i].Colour.A;
            }
            int readOffset = lifeCount * 3;
            // Frame data
            for (int f = 0; f < frameCount; f++)
            {
                for (int l = 0; l < lifeCount; l++)
                {
                    int x = frameData[f, l].Position.X;
                    int y = frameData[f, l].Position.X;
                    bool a = frameData[f, l].Alive;

                    switch (type)
                    {
                        case DataType.Int:
                            WriteBytes(data, ref readOffset,
                                a ? x : int.MaxValue,
                                a ? y : int.MaxValue);
                            break;

                        case DataType.Short:
                            WriteBytes(data, ref readOffset,
                                a ? (short)x : short.MaxValue,
                                a ? (short)y : short.MaxValue);
                            break;

                        case DataType.Byte:
                            data[readOffset] = a ? (byte)x : byte.MaxValue;
                            data[readOffset + 1] = a ? (byte)y : byte.MaxValue;
                            readOffset += 2;
                            break;

                        default:
                            throw new Exception();
                    }
                }
            }

            MemoryStream mem = new MemoryStream();
            ZipArchive zip = new ZipArchive(mem, ZipArchiveMode.Create, true);
            ZipArchiveEntry entry = zip.CreateEntry("data");

            Stream wrtier = entry.Open();
            wrtier.Write(data);

            zip.Dispose();

            mem.Seek(0, SeekOrigin.Begin);
            byte[] compressedData = mem.ToArray();

            mem.Dispose();

            byte[] finalData = new byte[compressedData.Length + 20];
            // Specifies it's a zene generation file
            finalData[0] = (byte)'Z';
            finalData[1] = (byte)'e';
            finalData[2] = (byte)'n';
            finalData[3] = (byte)'e';
            finalData[4] = (byte)'G';
            finalData[5] = (byte)'e';
            finalData[6] = (byte)'n';

            // Add data type info
            finalData[7] = (byte)type;

            // Add data info
            Span<byte> valueI = stackalloc byte[4];

            // World size
            BitConverter.TryWriteBytes(valueI, worldSize);
            finalData[8] = valueI[0];
            finalData[9] = valueI[1];
            finalData[10] = valueI[2];
            finalData[11] = valueI[3];

            // Frame count
            BitConverter.TryWriteBytes(valueI, frameCount);
            finalData[12] = valueI[0];
            finalData[13] = valueI[1];
            finalData[14] = valueI[2];
            finalData[15] = valueI[3];

            // Lifeform count
            BitConverter.TryWriteBytes(valueI, lifeCount);
            finalData[16] = valueI[0];
            finalData[17] = valueI[1];
            finalData[18] = valueI[2];
            finalData[19] = valueI[3];

            Array.Copy(compressedData, 0, finalData, 20, compressedData.Length);

            return finalData;
        }

        private static void WriteBytes(byte[] data, ref int index, short a, short b)
        {
            Span<byte> output = stackalloc byte[2];
            BitConverter.TryWriteBytes(output, a);

            data[index] = output[0];
            data[index + 1] = output[1];

            BitConverter.TryWriteBytes(output, b);

            data[index + 2] = output[0];
            data[index + 3] = output[1];

            index += 4;
        }
        private static void WriteBytes(byte[] data, ref int index, int a, int b)
        {
            Span<byte> output = stackalloc byte[4];
            BitConverter.TryWriteBytes(output, a);

            data[index] = output[0];
            data[index + 1] = output[1];
            data[index + 2] = output[2];
            data[index + 3] = output[3];

            BitConverter.TryWriteBytes(output, b);

            data[index + 4] = output[0];
            data[index + 5] = output[1];
            data[index + 6] = output[2];
            data[index + 7] = output[3];

            index += 8;
        }
    }
}
