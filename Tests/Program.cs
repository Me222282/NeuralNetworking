using System;
using System.IO;
using System.Collections.Generic;
using Zene.NeuralNetworking;
using Zene.Structs;
using Zene.Windowing;
using System.Threading.Tasks;
using FileEncoding;
using System.Reflection;

namespace NetworkProgram
{
    class Program
    {
        #if (DEBUG)
        private const string SettingsPath = "settings.json";
        #else
        private const string SettingsPath = "../settings.json";
        #endif
        
        public static Settings Settings;

        //private const double Googol = 10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d;

        private static void Main(string[] args)
        {
            //ConvertFromOld(args);
            //return;

            Core.Init();

            string settingsPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                SettingsPath);

            if (!File.Exists(settingsPath))
            {
                Console.WriteLine("No settings file.");
                Console.WriteLine($"Path: {settingsPath}");
                Console.ReadLine();

                Core.Terminate();
                return;
            }

            try
            {
                Settings = Settings.Parse(File.ReadAllText(settingsPath), settingsPath, args.Length > 0);
            }
            catch (Exception)
            {
                Console.ReadLine();

                Core.Terminate();
                return;
            }
            
            if (args.Length > 0)
            {
                RunGeneration(args);

                Core.Terminate();
                return;
            }

            Settings.CreateExport();

            if (Settings.Windowed)
            {
                //SimulateCustom();
                SimulateLive();
            }
            else { Simulate(); }

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
            if (exportGen)
            {
                frames = new FramePart[Settings.GenLength, world.Lifeforms.Length];
            }
            
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
                            FileStream stream = new FileStream($"{Settings.ExportPath}/{Settings.ExportName}{generation}.gen", FileMode.Create);

                            Gen.ExportFrames(
                                stream,
                                frames,
                                Settings.WorldSize,
                                generation,
                                Settings.BrainSize,
                                Settings.InnerCells,
                                Lifeform.ColourGrade);

                            stream.Close();

                            Network.ExportLifeforms($"{Settings.ExportPath}/{Settings.ExportName}-lf{generation}.txt", world.Lifeforms);

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
            
            WindowLive window = new WindowLive(128 * 6, 128 * 6, "Work");

            window.Run();
        }
        private static void RunGeneration(string[] paths)
        {
            FramePart[][,] frames = new FramePart[paths.Length][,];
            int[] frameCount = new int[paths.Length];
            int[] lifeCount = new int[paths.Length];
            int[] worldSize = new int[paths.Length];
            int[] generation = new int[paths.Length];

            for (int i = 0; i < paths.Length; i++)
            {
                FileStream stream;
                try
                {
                    stream = new FileStream(paths[i], FileMode.Open);
                }
                catch (Exception)
                {
                    Console.WriteLine($"{paths[i]} is an invalid path.");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine($"Opened file {i} at {paths[i]}");

                try
                {
                    frames[i] = Gen.ImportFrames(stream, out frameCount[i], out lifeCount[i], out worldSize[i], out generation[i], out _, out _, out _);
                }
                catch (Exception)
                {
                    Console.WriteLine($"{paths[i]} is an invalid gen file.");
                    Console.ReadLine();
                    return;
                }

                stream.Close();
            }

            WindowOpen window = new WindowOpen(128 * 6, 128 * 6, paths, frames, frameCount, lifeCount, worldSize, generation);

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

            WindowLive window = new WindowLive(128 * 6, 128 * 6, "Work",
                new Gene[][]
                {
                    genes1,
                    genes2,
                    genes3,
                    genes4
                });

            window.Run();
        }

        private static void ConvertFromOld(string[] paths)
        {
            foreach (string s in paths)
            {
                byte[] data = File.ReadAllBytes(s);

                FramePart[,] frameData = Gen.ImportFramesOld(data, out _, out _, out int ws);

                Console.WriteLine(s);

                Console.WriteLine("Enter generation");
                int g = int.Parse(Console.ReadLine());

                Console.WriteLine("Enter brain size");
                int b = int.Parse(Console.ReadLine());

                Console.WriteLine("Enter inner cell count");
                int i = int.Parse(Console.ReadLine());

                Console.WriteLine();

                Gen.ExportFrames(new FileStream(s, FileMode.Create), frameData, ws, g, b, i, 15);
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
    }
}
