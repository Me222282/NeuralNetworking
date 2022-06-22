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

            RetreiveAllFiles(args, out string[] genFiles, out string[] netFiles);

            Core.Init();

            Program program = new Program(settings);
            program.Run(genFiles, netFiles);

            Core.Terminate();
        }

        private void Run(string[] gens, string[] nets)
        {
            if (gens.Length > nets.Length)
            {
                RunGeneration(gens);
                return;
            }
            if (nets.Length > 0)
            {
                RunNetworks(nets);
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
                //SimulateCustom();
                SimulateLive();
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
            GenFile[] genFiles = new GenFile[paths.Length];

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
                    genFiles[i] = GenFile.Import(stream);
                }
                catch (Exception)
                {
                    Console.WriteLine($"{paths[i]} is an invalid gen file.");
                    Console.ReadLine();
                    return;
                }

                stream.Close();
            }

            WindowOpen window = new WindowOpen(128 * 6, 128 * 6, paths, Settings, genFiles);

            window.Run();
        }
        private void RunNetworks(string[] paths)
        {
            NetFile[] netFiles = new NetFile[paths.Length];

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
                    netFiles[i] = NetFile.Import(stream);
                }
                catch (Exception)
                {
                    Console.WriteLine($"{paths[i]} is an invalid net file.");
                    Console.ReadLine();
                    return;
                }

                stream.Close();
            }

            try
            {
                Settings.LoadDlls(netFiles[0].Dlls);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                return;
            }

            Settings.SetupEnvironment(netFiles[0].CellOrder);
            Settings.CreateExport();

            // Continue using PRNG from original simulation
            Lifeform.Random = netFiles[0].Random;

            WindowLive window = new WindowLive(128 * 6, 128 * 6, paths[0], Settings, netFiles[0]);

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
                new Gene("SUB3", "YM_", 1d)
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

        public void SetupEnvironment()
        {
            CellValue[] neurons;

            string neuronsPath = Path.Combine(ExecutablePath, NeuronsPath);

            if (!File.Exists(neuronsPath))
            {
                neurons = Settings.GenerateCellValues(new FileStream(neuronsPath, FileMode.Create), Settings);
            }
            else
            {
                neurons = Settings.ParseCellValues(File.ReadAllText(neuronsPath), Settings);
            }

            Settings.SetupEnvironment(neurons);
        }

        public static void Export(int generation, FramePart[,] frames, Lifeform[] lifeforms, Settings settings)
        {
            FileStream genStream = new FileStream($"{settings.ExportPath}/{settings.ExportName}{generation}.gen", FileMode.Create);

            GenFile.Export(
                genStream,
                frames,
                settings.WorldSize,
                generation,
                settings.BrainSize,
                settings.InnerCells,
                Lifeform.ColourGrade);

            genStream.Close();

            StreamWriter txtStream = new StreamWriter($"{settings.ExportPath}/{settings.ExportName}-lf{generation}.txt", false);

            NetFile.ExportLifeforms(txtStream, lifeforms);

            txtStream.Close();

            FileStream netStream = new FileStream($"{settings.ExportPath}/{settings.ExportName}{generation}.net", FileMode.Create);

            NetFile.Export(
                netStream,
                generation,
                settings.Dlls,
                settings.CellValues,
                lifeforms);

            netStream.Close();
        }

        public static void RetreiveAllFiles(string[] paths, out string[] gens, out string[] nets)
        {
            if (paths == null || paths.Length == 0)
            {
                gens = Array.Empty<string>();
                nets = Array.Empty<string>();
                return;
            }

            LinkedList<string> linkedGens = new LinkedList<string>();
            LinkedList<string> linkedNets = new LinkedList<string>();

            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].IsDirectory())
                {
                    ManageDirectory(paths[i], linkedGens, linkedNets);
                    continue;
                }

                ManageFile(paths[i], linkedGens, linkedNets);
            }

            gens = new string[linkedGens.Count];
            linkedGens.CopyTo(gens, 0);

            nets = new string[linkedNets.Count];
            linkedNets.CopyTo(nets, 0);
        }
        private static void ManageDirectory(string dir, LinkedList<string> gens, LinkedList<string> nets)
        {
            SortFiles(Directory.GetFiles(dir), gens, nets);

            string[] subDirs = Directory.GetDirectories(dir);

            for (int i = 0; i < subDirs.Length; i++)
            {
                ManageDirectory(subDirs[i], gens, nets);
            }
        }
        private static void SortFiles(string[] files, LinkedList<string> gens, LinkedList<string> nets)
        {
            for (int i = 0; i < files.Length; i++)
            {
                ManageFile(files[i], gens, nets);
            }
        }
        private static void ManageFile(string file, LinkedList<string> gens, LinkedList<string> nets)
        {
            Validation v = Validation.Get(file);

            if (v == GenFile.Validation)
            {
                gens.AddLast(file);
                Console.WriteLine($"Found gen file: {file}");
                return;
            }
            if (v == NetFile.Validation)
            {
                nets.AddLast(file);
                Console.WriteLine($"Found net file: {file}");
                return;
            }
        }
    }
}
