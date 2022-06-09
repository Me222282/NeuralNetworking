using System;
using System.IO;
using System.Collections.Generic;
using Zene.NeuralNetworking;
using Zene.Structs;
using Zene.Windowing;
using System.Text;
using System.IO.Compression;
using System.Threading.Tasks;

namespace NeuralNetworkingTest
{
    class Program
    {
        private const string SettingsPath = "settings.json";
        public static Settings Settings;

        private static void Main(string[] args)
        {
            Core.Init();

            if (!File.Exists(SettingsPath))
            {
                Console.WriteLine("No settings file.");
                Console.ReadLine();
                return;
            }

            try
            {
                Settings = Settings.Parse(File.ReadAllText(SettingsPath));
            }
            catch (Exception)
            {
                Console.ReadLine();
                return;
            }

            if (Settings.Windowed)
            {
                SimulateCustom();
                //SimulateLive();
            }
            else
            {
                Simulate();
            }

            //RunGeneration(new string[] { "output100.gen", "output200.gen", "output300.gen" });
            //RunGeneration(args);

            Core.Terminate();
        }

        private static void Simulate()
        {
            SetupEnvironment();

            List<int> exportGens = new List<int>(Settings.ExportGens);

            World world = new World(
                Settings.LifeForms,
                Settings.BrainSize,
                Settings.WorldSize,
                Settings.WorldSize,
                Settings.GenLength);

            FramePart[,] frames = null;

            int exporting = 0;
            object exportRef = new object();

            bool exportGen = exportGens.Contains(world.Generation);

            int counter = 0;
            while (world.Generation < Settings.Gens || Settings.Gens <= 0)
            {
                if (counter >= Settings.GenLength)
                {
                    if (exportGen)
                    {
                        int generation = world.Generation;

                        lock (exportRef) { exporting++; }

                        Task.Run(() =>
                        {
                            byte[] export = ExportFrames(frames, Settings.WorldSize);
                            File.WriteAllBytes($"{Settings.ExportPath}/{Settings.ExportName}{generation}.gen", export);

                            ExportLifeforms($"{Settings.ExportPath}/{Settings.ExportName}-lf{generation}.txt", world.Lifeforms);

                            lock (exportRef) { exporting--; }
                        });
                    }

                    counter = 0;
                    world = world.NextGeneration(Settings.LifeForms, CheckLifeform);

                    exportGen = exportGens.Contains(world.Generation);

                    if (exportGen)
                    {
                        frames = new FramePart[Settings.GenLength, world.Lifeforms.Length];
                    }
                }

                world.Update();

                if (exportGen)
                {
                    for (int i = 0; i < world.Lifeforms.Length; i++)
                    {
                        frames[counter, i] = new FramePart(world.Lifeforms[i]);
                    }
                }

                counter++;

                if (Settings.Delay != 0)
                {
                    System.Threading.Thread.Sleep(Settings.Delay);
                }
            }

            while (exporting > 0)
            {
                // Do nothing to let exporting finish
            }
        }
        private static void SimulateLive()
        {
            SetupEnvironment();

            WindowL window = new WindowL(128 * 6, 128 * 6, "Work");

            window.Run();
        }
        private static void RunGeneration(string[] paths)
        {
            byte[][] data = new byte[paths.Length][];

            StringBuilder text = new StringBuilder();

            for (int i = 0; i < paths.Length; i++)
            {
                text.Append(paths[i]);

                if (i + 1 != paths.Length)
                {
                    text.Append(" - ");
                }

                data[i] = File.ReadAllBytes(paths[i]);
            }

            WindowW window = new WindowW(128 * 6, 128 * 6, text.ToString(), data);

            window.Run();
        }

        private static void SimulateCustom()
        {
            SetupEnvironment();

            Gene[] genes1 = new Gene[]
            {
                new Gene(4, 4, -5.0),
                new Gene(5, 5, -5.0)
            };

            Gene[] genes2 = new Gene[]
            {
                new Gene(10, 4, 1.0),
                new Gene(10, 5, 1.0)
            };

            Gene[] genes3 = new Gene[]
            {
                new Gene(13, 4, 1.0)
            };

            Gene[] genes4 = new Gene[]
            {
                new Gene(12, 4, 2.0)
            };

            WindowL window = new WindowL(128 * 6, 128 * 6, "Work",
                new Gene[][]
                {
                    genes1,
                    genes2,
                    genes3,
                    genes4
                });

            window.Run();
        }

        private static void Convert(string[] paths, DataType type)
        {
            foreach (string s in paths)
            {
                byte[] data = File.ReadAllBytes(s);

                FramePart[,] frameData = ImportFrames(data, out _, out _, out int ws);

                byte[] newData = ExportFrames(frameData, ws, type);

                // Rename path to add " - New" to end of file name
                string newS = s;
                int dot = s.IndexOf('.');
                newS = newS.Insert(dot, " - New");

                File.WriteAllBytes(newS, newData);
            }
        }
        private static void ConvertFromOld(string[] paths)
        {
            foreach (string s in paths)
            {
                byte[] data = File.ReadAllBytes(s);

                FramePart[,] frameData = ImportFramesOld(data, out _, out _, out int ws);

                DataType type;

                // Determine smallest type that can fit world
                if (ws <= byte.MaxValue)
                {
                    type = DataType.Byte;
                }
                else if (ws <= short.MaxValue)
                {
                    type = DataType.Short;
                }
                else
                {
                    type = DataType.Int;
                }

                byte[] newData = ExportFrames(frameData, ws, type);

                // Rename path to add " - New" to end of file name
                string newS = s;
                int dot = s.IndexOf('.');
                newS = newS.Insert(dot, " - New");

                File.WriteAllBytes(newS, newData);
            }
        }

        //public static ICheckLifeform CheckLifeformFunc;
        public static bool CheckLifeform(Lifeform lifeform)
        {
            // Get to left
            //return lifeform.Location.X > (lifeform.CurrentWorld.Width / 2);
            
            // Get to centre X
            //return lifeform.Location.X > (lifeform.CurrentWorld.Width / 4) &&
            //    lifeform.Location.X < (lifeform.CurrentWorld.Width - (lifeform.CurrentWorld.Width / 4));
            
            // Get to corners
            //return (lifeform.Location.X < (lifeform.CurrentWorld.Width / 4) && (lifeform.Location.Y < (lifeform.CurrentWorld.Height / 4))) ||
            //    (lifeform.Location.X > (lifeform.CurrentWorld.Width - (lifeform.CurrentWorld.Width / 4)) && (lifeform.Location.Y > (lifeform.CurrentWorld.Height - (lifeform.CurrentWorld.Height / 4)))) ||
            //    (lifeform.Location.X > (lifeform.CurrentWorld.Width - (lifeform.CurrentWorld.Width / 4)) && (lifeform.Location.Y < (lifeform.CurrentWorld.Height / 4))) ||
            //    (lifeform.Location.X < (lifeform.CurrentWorld.Width / 4) && (lifeform.Location.Y > (lifeform.CurrentWorld.Height - (lifeform.CurrentWorld.Height / 4))));
            
            // Get to checkered patern location
            //return ((lifeform.Location.X + lifeform.Location.Y) % 2) == 0;
            
            // Get to the centre
            return (lifeform.Location.X > (lifeform.CurrentWorld.Width / 4)) &&
                (lifeform.Location.X < (lifeform.CurrentWorld.Width - (lifeform.CurrentWorld.Width / 4))) &&
                (lifeform.Location.Y > (lifeform.CurrentWorld.Height / 4)) &&
                (lifeform.Location.Y < (lifeform.CurrentWorld.Height - (lifeform.CurrentWorld.Height / 4)));
        }
        public static void SetupEnvironment()
        {
            // Inner Cells
            for (int i = 0; i < Settings.InnerCells; i++)
            {
                InnerCell.Add();
            }

            // Position Cells
            NeuralNetwork.PosibleGetCells.Add(new XPCell());
            NeuralNetwork.PosibleGetCells.Add(new YPCell());
            // Specific
            NeuralNetwork.PosibleGetCells.Add(new PUCell());
            NeuralNetwork.PosibleGetCells.Add(new PDCell());
            NeuralNetwork.PosibleGetCells.Add(new PLCell());
            NeuralNetwork.PosibleGetCells.Add(new PRCell());

            // New
            NeuralNetwork.PosibleGetCells.Add(new RandCell());
            NeuralNetwork.PosibleGetCells.Add(new SinCell());
            NeuralNetwork.PosibleGetCells.Add(new CosCell());
            NeuralNetwork.PosibleGetCells.Add(new TimeCell());

            XMCell.Add();
            YMCell.Add();

            LifeProperties.NeuronValueNumber = NeuralNetwork.PosibleSetCells.Count;

            Gene.MutationChance = Settings.Mutation;
            Lifeform.Random = new PRNG((ulong)Settings.Seed);
        }


        public static void ExportLifeforms(string path, Lifeform[] lifeforms)
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < lifeforms.Length; i++)
            {
                StringBuilder str = new StringBuilder($"Lifeform {i}\n");

                foreach (Neuron n in lifeforms[i].NeuralNetwork.Neurons)
                {
                    str.AppendLine($"{n.Source.Name} - {n.Destination.Name} - {n.Scale}");
                }

                lines.Add(str.ToString());
                lines.Add("");
            }

            File.WriteAllLines(path, lines);
        }

        public struct FramePart
        {
            public FramePart(Lifeform l)
            {
                Colour = l.Colour;
                Position = l.Location;
                Alive = l.Alive;
            }
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
        public static unsafe FramePart[,] ImportFramesOld(byte[] data, out int frameCount, out int lifeCount, out int worldSize)
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
        public static unsafe byte[] ExportFrames(FramePart[,] frameData, int worldSize, DataType type)
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
                data[di + 2] = frameData[0, i].Colour.B;
            }
            int readOffset = lifeCount * 3;
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
        public static unsafe byte[] ExportFrames(FramePart[,] frameData, int worldSize)
        {
            DataType type;

            // Determine smallest type that can fit world
            if (worldSize <= byte.MaxValue)
            {
                type = DataType.Byte;
            }
            else if (worldSize <= short.MaxValue)
            {
                type = DataType.Short;
            }
            else
            {
                type = DataType.Int;
            }

            return ExportFrames(frameData, worldSize, type);
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
