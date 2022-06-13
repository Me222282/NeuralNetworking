using System;
using System.IO;
using Zene.NeuralNetworking;
using K4os.Compression.LZ4.Streams;
using System.Collections.Generic;

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

        public static void Export(Stream stream, int generation, string[] dlls, Lifeform[] lifeforms)
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

            LZ4EncoderStream zip = LZ4Stream.Encode(stream);

            // Write list of dll paths
            for (int i = 0; i < dlls.Length; i++)
            {
                stream.Write(dlls[i].Length);

                // Write string
                for (int c = 0; c < dlls[i].Length; c++)
                {
                    stream.Write(dlls[i][c]);
                }
            }

            for (int l = 0; l < lifeforms.Length; l++)
            {
                Neuron[] neurons = lifeforms[l].NeuralNetwork.Neurons;

                // Number of neurons
                stream.Write(neurons.Length);

                // Write network
                for (int n = 0; n < neurons.Length; n++)
                {
                    stream.Write(neurons[n].Source.Name.GetHashCode());
                    stream.Write(neurons[n].Destination.Name.GetHashCode());

                    stream.Write(neurons[n].Scale);
                }
            }

            zip.Dispose();
        }
        public static Gene[][] Import(Stream stream, Dictionary<int, int> neuronMap, out int generation, out string[] dlls)
        {
            Validation v = stream.Read<Validation>();

            if (!v.Equals(new Validation(_v)))
            {
                throw new Exception($"{nameof(stream)} doesn't contain a network file.");
            }

            generation = stream.Read<int>();
            int lifeformLength = stream.Read<int>();
            int dllLength = stream.Read<int>();

            LZ4DecoderStream zip = LZ4Stream.Decode(stream);

            dlls = new string[dllLength];

            // Read list of dll paths
            for (int i = 0; i < dllLength; i++)
            {
                int strLength = stream.Read<int>();

                char[] str = new char[strLength];

                // Write string
                for (int c = 0; c < dlls[i].Length; c++)
                {
                    str[c] = stream.Read<char>();
                }

                dlls[i] = new string(str);
            }

            Gene[][] genes = new Gene[lifeformLength][];

            for (int l = 0; l < lifeformLength; l++)
            {
                // Number of neurons
                int neuronLength = stream.Read<int>();

                genes[l] = new Gene[neuronLength];

                // Write network
                for (int g = 0; g < neuronLength; g++)
                {
                    int source = stream.Read<int>();
                    int destination = stream.Read<int>();

                    double scale = stream.Read<double>();

                    bool success = neuronMap.TryGetValue(source, out int sourceLocation) &
                        neuronMap.TryGetValue(destination, out int destinationLocation);

                    if (!success)
                    {
                        throw new Exception("File contains unmapped or unknown neuron cell.");
                    }

                    genes[l][g] = new Gene((ushort)sourceLocation, (ushort)destinationLocation, scale);
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
