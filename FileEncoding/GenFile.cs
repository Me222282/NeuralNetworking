using System;
using System.IO;
using K4os.Compression.LZ4.Streams;
using Zene.Structs;

namespace FileEncoding
{
    public struct GenFile
    {
        public GenFile(FramePart[,] frames, int worldSize, int generation, int brainSize, int innerCells, byte colourGrade)
        {
            Frames = frames;
            WorldSize = worldSize;
            Generation = generation;
            BrainSize = brainSize;
            InnerCells = innerCells;
            ColourGrade = colourGrade;
        }

        public FramePart[,] Frames { get; }
        public int FrameCount => Frames.GetLength(0);
        public int LifeCount => Frames.GetLength(1);

        public int WorldSize { get; }
        public int Generation { get; }
        public int BrainSize { get; }
        public int InnerCells { get; }
        public byte ColourGrade { get; }

        public void Export(Stream stream) => Export(stream, Frames, WorldSize, Generation, BrainSize, InnerCells, ColourGrade);

        public static readonly Validation Validation = new Validation("ZeneGen3");

        private enum DataType : byte
        {
            Int = 0,
            Short = 1,
            Byte = 2
        }
        public static void Export(Stream stream, FramePart[,] frameData, int worldSize, int generation, int brainSize, int innerCells, byte colourGrade)
        {
            int lifeCount = frameData.GetLength(1);
            int frameCount = frameData.GetLength(0);

            stream.Write(Validation);

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
        public static GenFile Import(Stream stream)
        {
            Validation v = stream.Read<Validation>();

            if (!v.Equals(Validation))
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

            int worldSize = (int)stream.Read<uint>();
            int frameCount = (int)stream.Read<uint>();
            int lifeCount = (int)stream.Read<uint>();
            int generation = stream.Read<int>();
            int brainSize = (int)stream.Read<uint>();
            int innerCells = (int)stream.Read<uint>();
            byte colourGrade = (byte)stream.ReadByte();

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

            return new GenFile(frames, worldSize, generation, brainSize, innerCells, colourGrade);
        }

        public static bool IsGenFile(string path)
        {
            Validation v = Validation.Get(path);

            return v.Equals(Validation);
        }
    }
}
