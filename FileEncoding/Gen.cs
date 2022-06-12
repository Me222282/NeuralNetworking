using System;
using System.IO;
using System.IO.Compression;
using K4os.Compression.LZ4.Streams;
using Zene.Structs;

namespace FileEncoding
{
    public static class Gen
    {
        private struct Validation
        {
            public Validation(string str)
            {
                if (str.Length != 8)
                {
                    throw new Exception();
                }

                One = (byte)str[0];
                Two = (byte)str[1];
                Three = (byte)str[2];
                Four = (byte)str[3];
                Five = (byte)str[4];
                Six = (byte)str[5];
                Seven = (byte)str[6];
                Eight = (byte)str[7];
            }

            public byte One;
            public byte Two;
            public byte Three;
            public byte Four;
            public byte Five;
            public byte Six;
            public byte Seven;
            public byte Eight;

            public override bool Equals(object obj)
            {
                return obj is Validation v &&
                    v.One == One &&
                    v.Two == Two &&
                    v.Three == Three &&
                    v.Four == Four &&
                    v.Five == Five &&
                    v.Six == Six &&
                    v.Seven == Seven &&
                    v.Eight == Eight;
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(One, Two, Three, Four, Five, Six, Seven, Eight);
            }
        }
        public enum DataType : byte
        {
            Int = 0,
            Short = 1,
            Byte = 2
        }

        public static void ExportFrames(Stream stream, FramePart[,] frameData, int worldSize, int generation, int brainSize, int innerCells, byte colourGrade)
        {
            int lifeCount = frameData.GetLength(1);
            int frameCount = frameData.GetLength(0);

            stream.Write(new Validation("ZeneGen3"));

            DataType type;
            // Determine smallest type that can fit world
            if (worldSize <= byte.MaxValue)
            { type = DataType.Byte; }
            //
            else if (worldSize <= ushort.MaxValue)
            { type = DataType.Short; }
            //
            else { type = DataType.Int; }

            // Add data type info
            stream.WriteByte((byte)type);
            // World size
            stream.Write((uint)worldSize);
            // Frame count
            stream.Write((uint)frameCount);
            // Lifeform count
            stream.Write((uint)lifeCount);
            // Generation
            stream.Write(generation);
            // Brain size
            stream.Write((uint)brainSize);
            // Inner cells
            stream.Write((uint)innerCells);
            // Colour grade
            stream.WriteByte(colourGrade);

            LZ4EncoderStream zip = LZ4Stream.Encode(stream);

            // Colour data
            for (int i = 0; i < lifeCount; i++)
            {
                zip.Write(frameData[0, i].Colour);
            }
            // Frame data
            for (int f = 0; f < frameCount; f++)
            {
                for (int l = 0; l < lifeCount; l++)
                {
                    int x = frameData[f, l].Position.X;
                    int y = frameData[f, l].Position.Y;
                    bool a = frameData[f, l].Alive;

                    switch (type)
                    {
                        case DataType.Int:
                            zip.Write(a ? (uint)x : uint.MaxValue);
                            zip.Write(a ? (uint)y : uint.MaxValue);
                            break;

                        case DataType.Short:
                            zip.Write(a ? (ushort)x : ushort.MaxValue);
                            zip.Write(a ? (ushort)y : ushort.MaxValue);
                            break;

                        case DataType.Byte:
                            zip.WriteByte(a ? (byte)x : byte.MaxValue);
                            zip.WriteByte(a ? (byte)y : byte.MaxValue);
                            break;

                        default:
                            throw new Exception();
                    }
                }
            }

            zip.Dispose();
        }
        public static FramePart[,] ImportFrames(Stream stream,
            out int frameCount, out int lifeCount, out int worldSize,
            out int generation, out int brainSize, out int innerCells, out byte colourGrade)
        {
            Validation v = stream.Read<Validation>();

            if (!v.Equals(new Validation("ZeneGen3")))
            {
                throw new Exception($"{nameof(stream)} doesn't contain a gen file.");
            }

            int dataSize = stream.ReadByte() switch
            {
                0 => 4,
                1 => 2,
                2 => 1,
                _ => throw new Exception("Invalid data size specifier.")
            };

            worldSize = (int)stream.Read<uint>();
            frameCount = (int)stream.Read<uint>();
            lifeCount = (int)stream.Read<uint>();
            generation = stream.Read<int>();
            brainSize = (int)stream.Read<uint>();
            innerCells = (int)stream.Read<uint>();
            colourGrade = (byte)stream.ReadByte();

            LZ4DecoderStream zip = LZ4Stream.Decode(stream);

            Span<Colour> colours = stackalloc Colour[lifeCount];
            for (int i = 0; i < lifeCount; i++)
            {
                colours[i] = (Colour)zip.Read<Colour3>();
            }

            FramePart[,] frames = new FramePart[frameCount, lifeCount];

            for (int f = 0; f < frameCount; f++)
            {
                for (int l = 0; l < lifeCount; l++)
                {
                    int x, y;
                    bool alive = true;

                    switch (dataSize)
                    {
                        case 4:
                            // Int
                            x = (int)zip.Read<uint>();
                            y = (int)zip.Read<uint>();

                            if (x == int.MaxValue && y == int.MaxValue) { alive = false; }
                            break;

                        case 2:
                            // Short
                            x = zip.Read<ushort>();
                            y = zip.Read<ushort>();

                            if (x == short.MaxValue && y == short.MaxValue) { alive = false; }
                            break;

                        case 1:
                            // Byte
                            x = zip.ReadByte();
                            y = zip.ReadByte();

                            if (x == byte.MaxValue && y == byte.MaxValue) { alive = false; }
                            break;

                        default:
                            throw new Exception();
                    }

                    frames[f, l] = new FramePart(colours[l], x, y, alive);
                }

                Console.WriteLine($"Loaded frame {f}");
            }

            zip.Dispose();

            return frames;
        }

        private static bool ValidateGenFile(byte[] data)
        {
            return data[0] == 'Z' &&
                data[1] == 'e' &&
                data[2] == 'n' &&
                data[3] == 'e' &&
                data[4] == 'G' &&
                data[5] == 'e' &&
                data[6] == 'n';
        }
        public static unsafe FramePart[,] ImportFramesOld(byte[] data, out int frameCount, out int lifeCount, out int worldSize)
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

                Console.WriteLine($"Loaded frame {f}");
            }

            zip.Dispose();
            mem.Dispose();

            return frames;
        }
    }
}
