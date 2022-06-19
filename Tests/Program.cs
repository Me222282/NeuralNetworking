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
        private const string NeuronsPath = "neurons.json";
#else
        private const string SettingsPath = "../settings.json";
        private const string NeuronsPath = "../neurons.json";
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
                    world = world.NextGeneration(Settings.LifeForms, Settings.CheckLifeform);

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

                if (Settings.Delay > 0)
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
            Gene.GenNameRef();
            
            Gene[] genes1 = new Gene[]
            {
                new Gene("LFL", "XM_", 1d),
                new Gene("LFR", "XM_", -1d),
                new Gene("LFU", "YM_", 1d),
                new Gene("LFD", "YM_", -1d)
            };
            /*
            Gene[] genes1 = new Gene[]
            {
                // LFL
                new Gene("CONST1", "SUB0", 1d),
                new Gene("LFL", "SUB0", 1d),
                new Gene("SUB0", "XM_", 1d),

                // LFR
                new Gene("CONST1", "SUB1", 1d),
                new Gene("LFR", "SUB1", 1d),
                new Gene("SUB1", "XM_", -1d),

                // LFU
                new Gene("CONST1", "SUB2", 1d),
                new Gene("LFU", "SUB2", 1d),
                new Gene("SUB2", "YM_", -1d),

                // LFD
                new Gene("CONST1", "SUB3", 1d),
                new Gene("LFD", "SUB3", 1d),
                new Gene("SUB3", "YM_", 1d),
            };*/

            Lifeform l = new Lifeform(genes1, Vector2I.Zero, null);

            foreach (Neuron n in l.NeuralNetwork.Neurons)
            {
                Console.WriteLine($"Src: {n.Source.Name}, Dest: {n.Destination.Name}, Scale: {n.Scale}");
            }

            WindowLive window = new WindowLive(128 * 6, 128 * 6, "Work", Settings,
                new Gene[][]
                {
                    genes1,
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

        public void SetupEnvironment()
        {
            Neurons[] neurons;

            if (!File.Exists(NeuronsPath))
            {
                neurons = Neurons.Generate(new FileStream(NeuronsPath, FileMode.Create), Settings);
            }
            else
            {
                neurons = Neurons.Parse(File.ReadAllText(NeuronsPath), Settings);
            }

            Settings.SetupEnvironment(neurons);

            Settings.SelectedDll = Settings.LoadedDlls.FindDll("neighbours");
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
