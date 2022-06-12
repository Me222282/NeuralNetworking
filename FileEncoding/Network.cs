using System;
using System.IO;
using Zene.NeuralNetworking;

namespace FileEncoding
{
    public static class Network
    {
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
    }
}
