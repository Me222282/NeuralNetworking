using System;
using System.IO;
using Zene.NeuralNetworking;
using K4os.Compression.LZ4.Streams;

namespace FileEncoding
{
    public static class Network
    {
        private const string _v = "ZeneNet1";

        private struct Validation
        {
            public Validation(string str)
            {
                if (str.Length != 8)
                {
                    throw new Exception();
                }

                One = (byte)str[0];
                Two = (byte)str[1];
                Three = (byte)str[2];
                Four = (byte)str[3];
                Five = (byte)str[4];
                Six = (byte)str[5];
                Seven = (byte)str[6];
                Eight = (byte)str[7];
            }

            public byte One;
            public byte Two;
            public byte Three;
            public byte Four;
            public byte Five;
            public byte Six;
            public byte Seven;
            public byte Eight;

            public override bool Equals(object obj)
            {
                return obj is Validation v &&
                    v.One == One &&
                    v.Two == Two &&
                    v.Three == Three &&
                    v.Four == Four &&
                    v.Five == Five &&
                    v.Six == Six &&
                    v.Seven == Seven &&
                    v.Eight == Eight;
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(One, Two, Three, Four, Five, Six, Seven, Eight);
            }
        }

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

            stream.Write(new Validation(_v));
            stream.Write(generation);
            stream.Write(lifeforms.Length);
            stream.Write(dlls.Length);
            stream.Write(cellValues.Length);

            LZ4EncoderStream zip = LZ4Stream.Encode(stream);

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
                Gene[] genes = lifeforms[l].Genes;

                // Number of genes
                stream.Write(genes.Length);

                // Write network
                for (int g = 0; g < genes.Length; g++)
                {
                    stream.Write(genes[g].Source);
                    stream.Write(genes[g].Destination);

                    stream.Write(genes[g].Strength);
                }
            }

            zip.Dispose();
        }
        public static Gene[][] Import(Stream stream, out int generation, out string[] dlls, out CellValue[] cellValues)
        {
            Validation v = stream.Read<Validation>();

            if (!v.Equals(new Validation(_v)))
            {
                throw new Exception($"{nameof(stream)} doesn't contain a network file.");
            }

            generation = stream.Read<int>();
            int lifeformLength = stream.Read<int>();
            int dllLength = stream.Read<int>();
            int cellLength = stream.Read<int>();

            LZ4DecoderStream zip = LZ4Stream.Decode(stream);

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
                // Number of genes
                int geneLength = stream.Read<int>();

                genes[l] = new Gene[geneLength];

                // Write network
                for (int g = 0; g < geneLength; g++)
                {
                    ushort source = stream.Read<ushort>();
                    ushort destination = stream.Read<ushort>();

                    double strength = stream.Read<double>();

                    genes[l][g] = new Gene(source, destination, strength);
                }
            }

            zip.Dispose();

            return genes;
        }

        public static bool IsNetFile(string path)
        {
            Stream stream = new FileStream(path, FileMode.Open);

            Validation v = stream.Read<Validation>();

            stream.Close();

            return v.Equals(new Validation(_v));
        }
    }
}
