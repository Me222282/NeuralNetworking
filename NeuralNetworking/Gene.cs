namespace Zene.NeuralNetworking
{
    public struct Gene
    {
        public Gene(ushort src, ushort des, uint str)
        {
            Source = src;
            Destination = des;
            Strength = str;
        }
        /// <summary>
        /// Asigns <see cref="Strength"/> to <paramref name="str"/> passed through <see cref="Neuron.GetStrength(double)"/>.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="des"></param>
        /// <param name="str"></param>
        public Gene(ushort src, ushort des, double str)
        {
            Source = src;
            Destination = des;
            Strength = Neuron.GetStrength(str);
        }

        public ushort Source { get; }
        public ushort Destination { get; }
        public uint Strength { get; }

        public Gene CreateChild()
        {
            ushort source = Source;
            ushort destination = Destination;
            uint strength = Strength;

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
                        strength += (uint)Lifeform.Random.Generate(-StrengthChange, StrengthChange);
                        break;
                }
            }

            return new Gene(
                source,
                destination,
                strength);
        }

        public static double MutationChance { get; set; }
        public static uint StrengthChange { get; set; } = 1000;

        public static Gene Generate(PRNG random)
        {
            return new Gene(
                (ushort)random.Generate(0, ushort.MaxValue),
                (ushort)random.Generate(0, ushort.MaxValue),
                (uint)random.Generate(0, uint.MaxValue));
        }
        public static Gene Generate() => Generate(Lifeform.Random);
    }
}
