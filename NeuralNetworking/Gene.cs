using System;
using System.Collections.Generic;

namespace Zene.NeuralNetworking
{
    public struct Gene
    {
        public Gene(ushort src, ushort des, double str)
        {
            Source = src;
            Destination = des;
            Strength = str;
        }
        public Gene(string src, string des, double str)
        {
            bool validSrc = _getCells.TryGetValue(src, out int srcIndex);

            if (!validSrc)
            {
                throw new Exception($"No get cell named {src} exists.");
            }

            bool validDes = _setCells.TryGetValue(des, out int desIndex);

            if (!validDes)
            {
                throw new Exception($"No set cell named {des} exists.");
            }

            Source = (ushort)srcIndex;
            Destination = (ushort)desIndex;
            Strength = str;
        }

        public ushort Source { get; }
        public ushort Destination { get; }
        public double Strength { get; }

        public Gene CreateChild(out bool mutation)
        {
            ushort source = Source;
            ushort destination = Destination;
            double strength = Strength;
            mutation = false;

            if (Lifeform.OneInChance(MutationChance))
            {
                //Console.WriteLine("Mutation");

                int mutant = Lifeform.Random.Generate(0, 2);

                switch (mutant)
                {
                    case 0:
                        source += (ushort)Lifeform.Random.Generate(
                            -NeuralNetwork.PosibleGetCells.Count + 1,
                            NeuralNetwork.PosibleGetCells.Count - 1);
                        break;

                    case 1:
                        destination += (ushort)Lifeform.Random.Generate(
                            -NeuralNetwork.PosibleSetCells.Count + 1,
                            NeuralNetwork.PosibleSetCells.Count - 1);
                        break;

                    case 2:
                        strength += Lifeform.Random.Generate(-StrengthChange, StrengthChange);
                        break;
                }

                mutation = true;
            }

            return new Gene(
                source,
                destination,
                strength);
        }
        public Gene CreateChild() => CreateChild(out _);

        public static double MutationChance { get; set; }
        public static double StrengthChange { get; set; } = 2d;
        public static double StrengthRange { get; set; }

        public static Gene Generate(PRNG random)
        {
            return new Gene(
                (ushort)random.Generate(0, ushort.MaxValue),
                (ushort)random.Generate(0, ushort.MaxValue),
                random.Generate(-StrengthRange, StrengthRange));
        }
        public static Gene Generate() => Generate(Lifeform.Random);

        private static readonly Dictionary<string, int> _getCells = new Dictionary<string, int>();
        private static readonly Dictionary<string, int> _setCells = new Dictionary<string, int>();

        public static void GenNameRef()
        {
            for (int i = 0; i < NeuralNetwork.PosibleGetCells.Count; i++)
            {
                _getCells.Add(NeuralNetwork.PosibleGetCells[i].Name, i);
            }

            for (int i = 0; i < NeuralNetwork.PosibleSetCells.Count; i++)
            {
                _setCells.Add(NeuralNetwork.PosibleSetCells[i].Name, i);
            }
        }
    }
}
