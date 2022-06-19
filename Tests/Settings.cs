using System;
using System.IO;
using System.Text.Json;
using Zene.NeuralNetworking;
using Zene.Structs;

namespace NetworkProgram
{
    public class Settings
    {
        public Settings(string path)
        {
            Path = path;
        }

        public ulong Seed { get; private set; }
        public int WorldSize { get; private set; }
        public int GenLength { get; private set; }
        public int Gens { get; private set; }

        public int LifeForms { get; private set; }
        public int BrainSize { get; private set; }
        public int InnerCells { get; private set; }
        public double Mutation { get; private set; }
        public byte ColourGrade { get; private set; }

        public string[] Dlls { get; private set; }
        public int[] ExportGens { get; private set; }
        public string ExportName { get; private set; }
        public string ExportPath { get; private set; }

        public bool Windowed { get; private set; }
        public int Delay { get; private set; }
        public bool VSync { get; private set; }
        public bool LowPoly { get; private set; }
        public double BorderSize { get; private set; }
        public Colour BorderColour { get; private set; }

        public string Path { get; }

        /// <summary>
        /// Makes sure export path exists
        /// </summary>
        public void CreateExport()
        {
            Directory.CreateDirectory(ExportPath);
        }

        public DllLoad[] LoadedDlls { get; private set; }
        private int _selectedDll = 0;
        /// <summary>
        /// The <see cref="DllLoad"/> from <see cref="LoadedDlls"/> used by <see cref="CheckLifeform(Lifeform)"/>.
        /// </summary>
        public int SelectedDll
        {
            get => _selectedDll;
            set
            {
                if (LoadedDlls.Length <= value)
                {
                    throw new IndexOutOfRangeException();
                }
                if (!LoadedDlls[value].CanCheckLifeform)
                {
                    throw new Exception("Selected dll doesn't contain a \"CheckLifeform\" method.");
                }

                _selectedDll = value;
            }
        }

        public void LoadDlls()
        {
            LoadedDlls = new DllLoad[Dlls.Length];

            for (int i = 0; i < Dlls.Length; i++)
            {
                DllLoad value;

                try
                {
                    value = DllLoad.LoadDll(Dlls[i]);
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to load {Dlls[i]}. {e.Message}");
                }
                
                LoadedDlls[i] = value;
            }
        }

        public void SetupEnvironment(Neurons[] neurons)
        {
            // Add neuron cells in order
            for (int n = 0; n < neurons.Length; n++)
            {
                int d = neurons[n].DllIndex;
                int c = neurons[n].CellIndex;

                for (int i = 0; i < neurons[n].Count; i++)
                {
                    if (LoadedDlls[d].CellNames[0] == "InnerCell")
                    {
                        InnerCells = c;
                    }

                    LoadedDlls[d].AddCell(c);
                }
            }

            Gene.MutationChance = Mutation;
            Lifeform.ColourGrade = ColourGrade;
            Lifeform.Random = new PRNG(Seed);
        }

        public bool CheckLifeform(Lifeform lifeform)
        {
            bool value;

            if (!LoadedDlls[_selectedDll].CanCheckLifeform)
            {
                throw new Exception($"Selected dll {LoadedDlls[_selectedDll].Path} doesn't support CheckLifeform.");
            }

            try
            {
                value = LoadedDlls[_selectedDll].CheckLifeform(lifeform);
            }
            catch (Exception e)
            {
                throw new Exception($"{LoadedDlls[_selectedDll].Path} threw {e.GetType().FullName} with message: {e.Message}");
            }

            return value;
        }

        public static Settings Parse(string json, string path, bool forceWindow = false)
        {
            JsonElement decode = JsonDocument.Parse(json).RootElement;
            JsonElement world;
            JsonElement lifeform;
            JsonElement exporting = default;
            bool exportingExists = true;
            JsonElement window = default;
            bool windowProps = true;

            Settings values = new Settings(path);

            try
            {
                world = decode.GetProperty("world");
            }
            catch (Exception)
            {
                Console.WriteLine("Settings must contain the property \"world\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                lifeform = decode.GetProperty("lifeform");
            }
            catch (Exception)
            {
                Console.WriteLine("Settings must contain the property \"lifeform\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                exporting = decode.GetProperty("exporting");
            }
            catch (Exception)
            {
                exportingExists = false;
            }
            try
            {
                window = decode.GetProperty("window");
            }
            catch (Exception)
            {
                if (forceWindow)
                {
                    Console.WriteLine("Settings must contain the property \"window\".");
                    throw new Exception("Invalid settings file");
                }
                else
                {
                    values.Windowed = false;
                    windowProps = false;
                }
            }

            // World properties
            try
            {
                values.Seed = world.GetProperty("seed").GetUInt64();
            }
            catch (Exception)
            {
                Console.WriteLine("\"world\" must contain the unsigned long \"seed\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                values.WorldSize = world.GetProperty("size").GetInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("\"world\" must contain the integer \"worldSize\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                values.GenLength = world.GetProperty("generationLength").GetInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("\"world\" must contain the integer \"generationLength\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                values.Gens = world.GetProperty("generationCount").GetInt32();
            }
            catch (Exception)
            {
                values.Gens = 0;
            }

            // Lifeform properties
            try
            {
                values.LifeForms = lifeform.GetProperty("count").GetInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("\"lifeform\" must contain the integer \"lifeformCount\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                values.BrainSize = lifeform.GetProperty("brainSize").GetInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("\"lifeform\" must contain the integer \"brainSize\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                values.Mutation = lifeform.GetProperty("mutationChance").GetDouble();
            }
            catch (Exception)
            {
                Console.WriteLine("\"lifeform\" must contain the double \"mutationChance\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                values.ColourGrade = lifeform.GetProperty("colourGrade").GetByte();
            }
            catch (Exception) { }

            // Exporting properties
            try
            {
                values.Dlls = GetPathArray(decode, "dlls");
            }
            catch (Exception)
            {
                values.Dlls = Array.Empty<string>();
            }

            if (exportingExists)
            {
                 try
                {
                    values.ExportGens = GetIntArray(exporting, "exports");
                }
                catch (Exception)
                {
                    values.ExportGens = Array.Empty<int>();
                }
                try
                {
                    values.ExportName = exporting.GetProperty("name").GetString();
                }
                catch (Exception)
                {
                    Console.WriteLine("\"exporting\" must contain the string \"name\"");
                    throw new Exception("Invalid settings file");
                }
                try
                {
                    values.ExportPath = exporting.GetProperty("path").GetString();
                }
                catch (Exception)
                {
                    Console.WriteLine("\"exporting\" must contain the string \"path\"");
                    throw new Exception("Invalid settings file");
                }
            }

            if (!windowProps) { return values; }

            // Window properties
            try
            {
                values.Windowed = window.GetProperty("render").GetBoolean();
            }
            catch (Exception)
            {
                Console.WriteLine("\"window\" must contain the boolean \"render\".");
                throw new Exception("Invalid settings file");
            }
            values.Windowed |= forceWindow;

            try
            {
                values.Delay = window.GetProperty("frameDelay").GetInt32();
            }
            catch (Exception)
            {
                if (values.Windowed)
                {
                    Console.WriteLine("\"window\" must contain the integer \"frameDelay\" when \"render\" is true.");
                    throw new Exception("Invalid settings file");
                }
            }
            try
            {
                values.VSync = window.GetProperty("vSync").GetBoolean();
            }
            catch (Exception)
            {
                if (values.Windowed)
                {
                    Console.WriteLine("\"window\" must contain the boolean \"vSync\" when \"render\" is true.");
                    throw new Exception("Invalid settings file");
                }
            }
            try
            {
                values.LowPoly = window.GetProperty("lowPoly").GetBoolean();
            }
            catch (Exception) { }
            try
            {
                values.BorderSize = window.GetProperty("borderSize").GetDouble();
            }
            catch (Exception)
            {
                values.BorderSize = 0.5;
            }
            try
            {
                values.BorderColour = GetColour(window, "borderColour");
            }
            catch (Exception)
            {
                values.BorderColour = new Colour(128, 128, 128);
            }

            return values;
        }

        private static string[] GetPathArray(JsonElement json, string name)
        {
            JsonElement array = json.GetProperty(name);

            string[] value = new string[array.GetArrayLength()];

            for (int i = 0; i < value.Length; i++)
            {
                value[i] = System.IO.Path.Combine(Program.ExecutablePath, array[i].GetString());
            }

            return value;
        }
        private static int[] GetIntArray(JsonElement json, string name)
        {
            JsonElement array = json.GetProperty(name);

            int[] value = new int[array.GetArrayLength()];

            for (int i = 0; i < value.Length; i++)
            {
                value[i] = array[i].GetInt32();
            }

            return value;
        }
        private static Colour GetColour(JsonElement json, string name)
        {
            JsonElement array = json.GetProperty(name);

            int length = array.GetArrayLength();

            if (length != 3 ||
                length != 4)
            {
                throw new Exception("Invalid colour.");
            }

            byte[] value = new byte[4];
            value[3] = 255;

            for (int i = 0; i < length; i++)
            {
                value[i] = array[i].GetByte();
            }

            return new Colour(value[0], value[1], value[2], value[3]);
        }
    }
}
