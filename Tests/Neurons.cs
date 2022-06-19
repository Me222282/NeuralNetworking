using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Zene.Structs;

namespace NetworkProgram
{
    public struct Neurons
    {
        private Neurons(int di, int ci, int count)
        {
            DllIndex = di;
            CellIndex = ci;
            Count = count;
        }

        public int DllIndex { get; }
        public int CellIndex { get; }
        public int Count { get; }

        public static Neurons[] Parse(string json, Settings settings)
        {
            JsonElement decode = JsonDocument.Parse(json).RootElement;

            LinkedList<Neurons> neurons = new LinkedList<Neurons>();

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

                neurons.AddLast(new Neurons(index.X, index.Y, count));
            }

            Neurons[] array = new Neurons[neurons.Count];
            neurons.CopyTo(array, 0);
            return array;
        }

        public static Neurons[] Generate(Stream stream, Settings settings)
        {
            LinkedList<Neurons> neurons = new LinkedList<Neurons>();

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
                    neurons.AddLast(new Neurons(i, s, 1));
                    jw.WriteNumber(settings.LoadedDlls[i].CellNames[s], 1);
                }
            }

            jw.WriteEndObject();
            jw.Dispose();

            Neurons[] array = new Neurons[neurons.Count];
            neurons.CopyTo(array, 0);
            return array;
        }
    }
}
