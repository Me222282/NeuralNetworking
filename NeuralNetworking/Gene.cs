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
        /// Asigns <see cref="Strength"/> to <paramref name="str"/> passed through <see cref="Neuron.CreateStrength(double)"/>.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="des"></param>
        /// <param name="str"></param>
        public Gene(ushort src, ushort des, double str)
        {
            Source = src;
            Destination = des;
            Strength = Neuron.CreateStrength(str);
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
                ushort change = (ushort)Lifeform.Random.Generate(0, 1);

                // Make sure that the change is either -1 or +1
                if (change == 0) { change--; }

                switch (mutant)
                {
                    case 0:
                        source += change;
                        break;

                    case 1:
                        destination += change;
                        break;

                    case 2:
                        strength += change;
                        break;
                }
            }

            return new Gene(
                source,
                destination,
                strength);
        }

        public static double MutationChance { get; set; }

        public static Gene Generate(PRNG random)
        {
            return new Gene(
                (ushort)random.Generate(0, ushort.MaxValue),
                (ushort)random.Generate(0, ushort.MaxValue),
                (uint)random.Generate(0, uint.MaxValue));
        }

        public static Gene Generate()
        {
            ushort a;
            ushort b;
            uint c;

            lock (Lifeform.RandSync)
            {
                a = (ushort)Lifeform.Random.Generate(0, ushort.MaxValue);
                b = (ushort)Lifeform.Random.Generate(0, ushort.MaxValue);
                c = (uint)Lifeform.Random.Generate(0, uint.MaxValue);
            }

            return new Gene(a, b, c);
        }
    }
}
