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
    internal sealed class Program
    {
        #if (DEBUG)
        private const string SettingsPath = "settings.json";
        #else
        private const string SettingsPath = "../settings.json";
        #endif
        
        private Program(Settings settings)
        {
            Settings = settings;
        }

        public Settings Settings { get; set; }

        //private const double Googol = 10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000d;

        internal static string ExecutablePath;

        private static void Main(string[] args)
        {
            ExecutablePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            string settingsPath = Path.Combine(ExecutablePath, SettingsPath);

            if (!File.Exists(settingsPath))
            {
                Console.WriteLine("No settings file.");
                Console.WriteLine($"Path: {settingsPath}");
                Console.ReadLine();
                return;
            }

            Settings settings;

            try
            {
                settings = Settings.Parse(File.ReadAllText(settingsPath), settingsPath, args.Length > 0);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid settings file.");
                Console.WriteLine($"Path: {settingsPath}");
                Console.ReadLine();
                return;
            }

            Core.Init();

            Program program = new Program(settings);
            program.Run(args);

            Core.Terminate();
        }

        private void Run(string[] args)
        {
            if (args.Length > 0)
            {
                RunGeneration(args);
                return;
            }

            try
            {
                Settings.LoadDlls();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                return;
            }

            SetupEnvironment();

            Settings.CreateExport();

            if (Settings.Windowed)
            {
                SimulateCustom();
                //SimulateLive();
            }
            else { Simulate(); }
        }

        private void Simulate()
        {
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
                            Export(generation, frames, world.Lifeforms, Settings);

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
        private void SimulateLive()
        {
            WindowLive window = new WindowLive(128 * 6, 128 * 6, "Work", Settings);

            window.Run();
        }
        private void RunGeneration(string[] paths)
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

            WindowOpen window = new WindowOpen(128 * 6, 128 * 6, paths, Settings, frames, frameCount, lifeCount, worldSize, generation);

            window.Run();
        }

        private void SimulateCustom()
        {
            Gene[] genes1 = new Gene[]
            {
                new Gene(10, 10, -0.5),
                new Gene(11, 10, 0.5),
                new Gene(12, 11, 0.5),
                new Gene(13, 11, -0.5)
            };

            Lifeform l = new Lifeform(genes1, Vector2I.Zero, null);

            foreach (Neuron n in l.NeuralNetwork.Neurons)
            {
                Console.WriteLine($"Src: {n.Source.Name}, Dest: {n.Destination.Name}, Scale: {n.Scale}");
            }

            WindowLive window = new WindowLive(128 * 6, 128 * 6, "Work", Settings,
                new Gene[][]
                {
                    genes1
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

            // Have an odd number of neighbours
            World world = lifeform.CurrentWorld;
            Vector2I pos = lifeform.Location;

            int neighbours = 0;

            if (pos.X + 1 < world.Width)
            {
                neighbours += world.LifeformGrid[pos.X + 1, pos.Y] is null ? 0 : 1;
            }
            if (pos.Y + 1 < world.Height)
            {
                neighbours += world.LifeformGrid[pos.X, pos.Y + 1] is null ? 0 : 1;
            }
            if (pos.X - 1 >= 0)
            {
                neighbours += world.LifeformGrid[pos.X - 1, pos.Y] is null ? 0 : 1;
            }
            if (pos.Y - 1 >= 0)
            {
                neighbours += world.LifeformGrid[pos.X, pos.Y - 1] is null ? 0 : 1;
            }

            return neighbours % 2 == 1;
        }
        public void SetupEnvironment()
        {
            // Inner Cells
            for (int i = 0; i < Settings.InnerCells; i++)
            {
                InnerCell.Add();
            }

            LFLCell.Add();
            LFRCell.Add();
            LFUCell.Add();
            LFDCell.Add();

            // Load all possible Cells
            for (int d = 0; d < Settings.LoadedDlls.Length; d++)
            {
                if (!Settings.LoadedDlls[d].ContainsCells)
                {
                    continue;
                }

                for (int c = 0; c < Settings.LoadedDlls[d].CellNames.Length; c++)
                {
                    Settings.LoadedDlls[d].AddCell(c);
                }
            }

            Gene.MutationChance = Settings.Mutation;
            Lifeform.Random = new PRNG((ulong)Settings.Seed);
        }

        public static void Export(int generation, FramePart[,] frames, Lifeform[] lifeforms, Settings settings)
        {
            FileStream stream = new FileStream($"{settings.ExportPath}/{settings.ExportName}{generation}.gen", FileMode.Create);

            Gen.ExportFrames(
                stream,
                frames,
                settings.WorldSize,
                generation,
                settings.BrainSize,
                settings.InnerCells,
                Lifeform.ColourGrade);

            stream.Close();

            Network.ExportLifeforms($"{settings.ExportPath}/{settings.ExportName}-lf{generation}.txt", lifeforms);
        }
    }
}
