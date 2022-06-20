using System;
using System.IO;
using Zene.NeuralNetworking;
using K4os.Compression.LZ4.Streams;

namespace FileEncoding
{
    public static class Network
    {
        public static readonly Validation Validation = new Validation("ZeneNet1");

        public static void ExportLifeforms(string path, Lifeform[] lifeforms)
        {
            StreamWriter stream = new StreamWriter(path, false);

            for (int i = 0; i < lifeforms.Length; i++)
            {
                stream.WriteLine($"Lifeform {i}\n");

                foreach (Neuron n in lifeforms[i].NeuralNetwork.Neurons)
                {
                    stream.WriteLine($"{n.Source.Name} - {n.Destination.Name} - {n.Scale}");
                }

                stream.WriteLine();
            }

            stream.Close();
        }

        public static void Export(Stream stream, int generation, string[] dlls, CellValue[] cellValues, Lifeform[] lifeforms)
        {
            if (lifeforms == null)
            {
                throw new ArgumentNullException(nameof(lifeforms));
            }
            if (dlls == null)
            {
                dlls = Array.Empty<string>();
            }

            stream.Write(Validation);
            stream.Write(generation);
            stream.Write(lifeforms.Length);
            stream.Write(dlls.Length);
            stream.Write(cellValues.Length);

            LZ4EncoderStream zip = LZ4Stream.Encode(stream);

            // Write data from random to stream
            Lifeform.Random.WriteToStream(stream);

            // Write list of dll paths
            for (int i = 0; i < dlls.Length; i++)
            {
                stream.Write(dlls[i]);
            }
            // Write the order and count of the neuron cells
            for (int i = 0; i < cellValues.Length; i++)
            {
                stream.Write(cellValues[i]);
            }

            for (int l = 0; l < lifeforms.Length; l++)
            {
                // Write network
                stream.Write(lifeforms[l].Genes);
            }

            zip.Dispose();
        }
        public static Gene[][] Import(Stream stream, out int generation, out string[] dlls, out CellValue[] cellValues)
        {
            Validation v = stream.Read<Validation>();

            if (!v.Equals(Validation))
            {
                throw new Exception($"{nameof(stream)} doesn't contain a network file.");
            }

            generation = stream.Read<int>();
            int lifeformLength = stream.Read<int>();
            int dllLength = stream.Read<int>();
            int cellLength = stream.Read<int>();

            LZ4DecoderStream zip = LZ4Stream.Decode(stream);

            // Read PRNG data from stream
            Lifeform.Random = PRNG.FromStream(stream);

            dlls = new string[dllLength];
            // Read list of dll paths
            for (int i = 0; i < dllLength; i++)
            {
                dlls[i] = stream.ReadString();
            }

            cellValues = new CellValue[cellLength];
            // Read the order and count of the neuron cells
            for (int i = 0; i < cellLength; i++)
            {
                cellValues[i] = stream.Read<CellValue>();
            }

            Gene[][] genes = new Gene[lifeformLength][];

            for (int l = 0; l < lifeformLength; l++)
            {
                // Read network
                genes[l] = stream.ReadArray<Gene>();
            }

            zip.Dispose();

            return genes;
        }

        public static bool IsNetFile(string path)
        {
            Validation v = Validation.Get(path);

            return v.Equals(Validation);
        }
    }
}
