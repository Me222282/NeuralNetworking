using System;
using System.IO;
using Zene.NeuralNetworking;
using K4os.Compression.LZ4.Streams;

namespace FileEncoding
{
    public struct NetFile
    {
        public NetFile(Gene[][] genes, int generation, string[] dlls, CellValue[] cellValues)
        {
            Genes = genes;
            Generation = generation;
            Dlls = dlls;
            CellOrder = cellValues;
        }

        public Gene[][] Genes { get; }

        public int Generation { get; }
        public string[] Dlls { get; }
        public CellValue[] CellOrder { get; }

        public void Export(Stream stream) => Export(stream, Generation, Dlls, CellOrder, Genes);

        public static readonly Validation Validation = new Validation("ZeneNet1");

        public static void ExportLifeforms(StreamWriter stream, Lifeform[] lifeforms)
        {
            for (int i = 0; i < lifeforms.Length; i++)
            {
                stream.WriteLine($"Lifeform {i}\n");

                foreach (Neuron n in lifeforms[i].NeuralNetwork.Neurons)
                {
                    stream.WriteLine($"{n.Source.Name} - {n.Destination.Name} - {n.Scale}");
                }

                stream.WriteLine();
            }
        }

        public static void Export(Stream stream, int generation, string[] dlls, CellValue[] cellValues, Gene[][] genes)
        {
            if (genes == null)
            {
                throw new ArgumentNullException(nameof(genes));
            }
            if (dlls == null)
            {
                dlls = Array.Empty<string>();
            }

            stream.Write(Validation);
            stream.Write(generation);
            stream.Write(genes.Length);
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

            for (int g = 0; g < genes.Length; g++)
            {
                // Write network
                stream.Write(genes[g]);
            }

            zip.Dispose();
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
        public static NetFile Import(Stream stream)
        {
            Validation v = stream.Read<Validation>();

            if (!v.Equals(Validation))
            {
                throw new Exception($"{nameof(stream)} doesn't contain a network file.");
            }

            int generation = stream.Read<int>();
            int lifeformLength = stream.Read<int>();
            int dllLength = stream.Read<int>();
            int cellLength = stream.Read<int>();

            LZ4DecoderStream zip = LZ4Stream.Decode(stream);

            // Read PRNG data from stream
            Lifeform.Random = PRNG.FromStream(stream);

            string[] dlls = new string[dllLength];
            // Read list of dll paths
            for (int i = 0; i < dllLength; i++)
            {
                dlls[i] = stream.ReadString();
            }

            CellValue[] cellValues = new CellValue[cellLength];
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

                Console.WriteLine($"Loaded lifeform {l}");
            }

            zip.Dispose();

            return new NetFile(genes, generation, dlls, cellValues);
        }

        public static bool IsNetFile(string path)
        {
            Validation v = Validation.Get(path);

            return v.Equals(Validation);
        }
    }
}
