using System;
using System.Text.Json;

namespace NeuralNetworkingTest
{
    public struct Settings
    {
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

        public static Settings Parse(string json)
        {
            JsonElement decode = JsonDocument.Parse(json).RootElement;

            Settings values = new Settings();

            // World properties
            try
            {
                values.Seed = decode.GetProperty("seed").GetInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("Settings must contain the integer \"seed\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                values.WorldSize = decode.GetProperty("worldSize").GetInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("Settings must contain the integer \"worldSize\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                values.GenLength = decode.GetProperty("generationLength").GetInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("Settings must contain the integer \"generationLength\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                values.Gens = decode.GetProperty("generationCount").GetInt32();
            }
            catch (Exception)
            {
                values.Gens = 0;
            }

            // Lifeform properties
            try
            {
                values.LifeForms = decode.GetProperty("lifeformCount").GetInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("Settings must contain the integer \"lifeformCount\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                values.BrainSize = decode.GetProperty("brainSize").GetInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("Settings must contain the integer \"brainSize\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                values.InnerCells = decode.GetProperty("innerCells").GetInt32();
            }
            catch (Exception)
            {
                Console.WriteLine("Settings must contain the integer \"innerCells\".");
                throw new Exception("Invalid settings file");
            }
            try
            {
                values.Mutation = decode.GetProperty("mutationChance").GetDouble();
            }
            catch (Exception)
            {
                Console.WriteLine("Settings must contain the double \"mutationChance\".");
                throw new Exception("Invalid settings file");
            }

            // Exporting properties
            try
            {
                values.Dlls = GetStringArray(decode, "dlls");
            }
            catch (Exception)
            {
                values.Dlls = Array.Empty<string>();
            }

            bool exporting;
            try
            {
                values.ExportGens = GetIntArray(decode, "exports");

                exporting = values.ExportGens.Length > 0;
            }
            catch (Exception)
            {
                values.ExportGens = Array.Empty<int>();
                exporting = false;
            }
            if (exporting)
            {
                try
                {
                    values.ExportName = decode.GetProperty("exportName").GetString();
                }
                catch (Exception)
                {
                    Console.WriteLine("Settings must contain the string \"exportName\" when \"exports\" is included.");
                    throw new Exception("Invalid settings file");
                }
                try
                {
                    values.ExportPath = decode.GetProperty("exportPath").GetString();
                }
                catch (Exception)
                {
                    Console.WriteLine("Settings must contain the string \"exportPath\" when \"exports\" is included.");
                    throw new Exception("Invalid settings file");
                }
            }

            // Window properties
            try
            {
                values.Windowed = decode.GetProperty("windowed").GetBoolean();
            }
            catch (Exception)
            {
                Console.WriteLine("Settings must contain the boolean \"windowed\".");
                throw new Exception("Invalid settings file");
            }
            if (values.Windowed)
            {
                try
                {
                    values.Delay = decode.GetProperty("frameDelay").GetInt32();
                }
                catch (Exception)
                {
                    Console.WriteLine("Settings must contain the string \"frameDelay\" when \"windowed\" is true.");
                    throw new Exception("Invalid settings file");
                }
                try
                {
                    values.VSync = decode.GetProperty("vSync").GetBoolean();
                }
                catch (Exception)
                {
                    Console.WriteLine("Settings must contain the string \"vSync\" when \"windowed\" is true.");
                    throw new Exception("Invalid settings file");
                }
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
