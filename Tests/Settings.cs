using System;
using System.IO;
using System.Text.Json;
using Zene.NeuralNetworking;

namespace NeuralNetworkingTest
{
    public struct Settings
    {
        public Settings(string path)
        {
            Path = path;

            Seed = default;
            WorldSize = default;
            GenLength = default;
            Gens = default;
            LifeForms = default;
            BrainSize = default;
            InnerCells = default;
            Mutation = default;
            Dlls = default;
            ExportGens = default;
            ExportName = default;
            ExportPath = default;
            Windowed = default;
            Delay = default;
            VSync = default;
        }

        public int Seed { get; private set; }
        public int WorldSize { get; private set; }
        public int GenLength { get; private set; }
        public int Gens { get; private set; }

        public int LifeForms { get; private set; }
        public int BrainSize { get; private set; }
        public int InnerCells { get; private set; }
        public double Mutation { get; private set; }

        public string[] Dlls { get; private set; }
        public int[] ExportGens { get; private set; }
        public string ExportName { get; private set; }
        public string ExportPath { get; private set; }

        public bool Windowed { get; private set; }
        public int Delay { get; private set; }
        public bool VSync { get; private set; }

        public string Path { get; }

        /// <summary>
        /// Makes sure export path exists
        /// </summary>
        public void CreateExport()
        {
            Directory.CreateDirectory(ExportPath);
        }

        public static Settings Parse(string json, string path)
        {
            JsonElement decode = JsonDocument.Parse(json).RootElement;
            JsonElement world;
            JsonElement lifeform;
            JsonElement exporting = default;
            bool exportingExists = true;
            JsonElement window;
            
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
                Console.WriteLine("Settings must contain the property \"window\".");
                throw new Exception("Invalid settings file");
            }
            
            Settings values = new Settings(path);

            // World properties
            try
            {
                values.Seed = world.GetProperty("seed").GetInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("\"world\" must contain the integer \"seed\".");
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
                values.InnerCells = lifeform.GetProperty("innerCells").GetInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("\"lifeform\" must contain the integer \"innerCells\".");
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
                Lifeform.ColourGrade = lifeform.GetProperty("colourGrade").GetByte();
            }
            catch (Exception) { }

            // Exporting properties
            try
            {
                values.Dlls = GetStringArray(decode, "dlls");
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
            if (values.Windowed)
            {
                try
                {
                    values.Delay = window.GetProperty("frameDelay").GetInt32();
                }
                catch (Exception)
                {
                    Console.WriteLine("\"window\" must contain the string \"frameDelay\" when \"render\" is true.");
                    throw new Exception("Invalid settings file");
                }
                try
                {
                    values.VSync = window.GetProperty("vSync").GetBoolean();
                }
                catch (Exception)
                {
                    Console.WriteLine("\"window\" must contain the string \"vSync\" when \"render\" is true.");
                    throw new Exception("Invalid settings file");
                }
            }
            else
            {
                // If loading files - these properties need to be loaded
                try
                {
                    values.Delay = window.GetProperty("frameDelay").GetInt32();
                }
                catch (Exception) { }
                try
                {
                    values.VSync = window.GetProperty("vSync").GetBoolean();
                }
                catch (Exception) { }
            }

            return values;
        }

        private static string[] GetStringArray(JsonElement json, string name)
        {
            JsonElement array = json.GetProperty(name);

            string[] value = new string[array.GetArrayLength()];

            for (int i = 0; i < value.Length; i++)
            {
                value[i] = array[i].GetString();
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
    }
}
