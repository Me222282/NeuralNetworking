using FileEncoding;
using System;
using System.Collections.Generic;
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
        public double StrengthRange { get; private set; }
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
            if (ExportPath == null) { return; }

            Directory.CreateDirectory(ExportPath);
        }

        public DllLoad[] LoadedDlls { get; private set; }
        private LifeformCondition[] _checkLFs;

        public CellValue[] CellValues { get; private set; }

        public void LoadDlls() => LoadDlls(Dlls);
        public void LoadDlls(string[] dlls)
        {
            LoadedDlls = new DllLoad[dlls.Length];

            LinkedList<LifeformCondition> checkLFs = new LinkedList<LifeformCondition>();

            for (int i = 0; i < dlls.Length; i++)
            {
                DllLoad value;

                try
                {
                    value = DllLoad.LoadDll(dlls[i]);
                }
                catch (Exception e)
                {
                    throw new Exception($"Failed to load {dlls[i]}. {e.Message}");
                }

                LoadedDlls[i] = value;

                if (value.CanCheckLifeform)
                {
                    checkLFs.AddLast(value.CheckLifeform);
                }
            }

            _checkLFs = new LifeformCondition[checkLFs.Count];
            checkLFs.CopyTo(_checkLFs, 0);
        }

        public void SetupEnvironment(CellValue[] cellValues)
        {
            CellValues = cellValues;

            // Add neuron cells in order
            for (int n = 0; n < cellValues.Length; n++)
            {
                int d = cellValues[n].DllIndex;
                int c = cellValues[n].CellIndex;

                for (int i = 0; i < cellValues[n].Count; i++)
                {
                    if (LoadedDlls[d].CellNames[0] == "InnerCell")
                    {
                        InnerCells = c;
                    }

                    LoadedDlls[d].AddCell(c);
                }
            }

            Gene.StrengthRange = StrengthRange;
            Gene.MutationChance = Mutation;
            Lifeform.ColourGrade = ColourGrade;
            Lifeform.Random = new PRNG(Seed);
        }

        public bool CheckLifeform(Lifeform lifeform)
        {
            for (int i = 0; i < LoadedDlls.Length; i++)
            {
                bool value;

                try
                {
                    value = LoadedDlls[i].CheckLifeform(lifeform);
                }
                catch (Exception e)
                {
                    throw new Exception($"{LoadedDlls[i].Path} threw {e.GetType().FullName} with message: {e.Message}");
                }

                if (value)
                {
                    continue;
                }

                return false;
            }

            return true;
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
                values.StrengthRange = lifeform.GetProperty("strengthRange").GetDouble();
            }
            catch (Exception)
            {
                Console.WriteLine("\"lifeform\" must contain the double \"strengthRange\".");
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

                values.ExportPath = System.IO.Path.Combine(Program.ExecutablePath, values.ExportPath);
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

        public static CellValue[] ParseCellValues(string json, Settings settings)
        {
            JsonElement decode = JsonDocument.Parse(json).RootElement;

            LinkedList<CellValue> neurons = new LinkedList<CellValue>();

            Dictionary<string, Vector2I> locations = new Dictionary<string, Vector2I>();

            // Add all cells to dictionary
            for (int i = 0; i < settings.LoadedDlls.Length; i++)
            {
                if (!settings.LoadedDlls[i].ContainsCells)
                {
                    continue;
                }

                for (int s = 0; s < settings.LoadedDlls[i].CellNames.Length; s++)
                {
                    locations.Add(
                        settings.LoadedDlls[i].CellNames[s],
                        new Vector2I(i, s));
                }
            }

            foreach (JsonProperty je in decode.EnumerateObject())
            {
                bool exists = locations.TryGetValue(je.Name, out Vector2I index);

                if (!exists)
                {
                    throw new Exception($"No dll contains a cell named {je.Name}");
                }

                bool valid = je.Value.TryGetInt32(out int count);

                if (!valid)
                {
                    throw new Exception($"Property {je.Name} must contain an integer.");
                }

                neurons.AddLast(new CellValue(index.X, index.Y, count));
            }

            CellValue[] array = new CellValue[neurons.Count];
            neurons.CopyTo(array, 0);
            return array;
        }
        public static CellValue[] GenerateCellValues(Stream stream, Settings settings)
        {
            LinkedList<CellValue> neurons = new LinkedList<CellValue>();

            Utf8JsonWriter jw = new Utf8JsonWriter(stream,
                new JsonWriterOptions
                {
                    Indented = true,
                });

            jw.WriteStartObject();

            for (int i = 0; i < settings.LoadedDlls.Length; i++)
            {
                if (!settings.LoadedDlls[i].ContainsCells)
                {
                    continue;
                }

                for (int s = 0; s < settings.LoadedDlls[i].CellNames.Length; s++)
                {
                    neurons.AddLast(new CellValue(i, s, 1));
                    jw.WriteNumber(settings.LoadedDlls[i].CellNames[s], 1);
                }
            }

            jw.WriteEndObject();
            jw.Dispose();

            CellValue[] array = new CellValue[neurons.Count];
            neurons.CopyTo(array, 0);
            return array;
        }
    }
}
