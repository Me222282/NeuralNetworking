using System;
using Zene.Structs;

namespace Zene.NeuralNetworking
{
    public class Lifeform
    {
        public Lifeform(Gene[] genes, Vector2I pos, World world)
        {
            Genes = genes;
            NeuralNetwork = NeuralNetwork.Generate(genes);

            if (genes.Length < 1)
            {
                Colour = new Colour();
            }
            else
            {
                Colour = GeneColour(genes);
            }
            _location = pos;
            PreLocation = pos;

            Properties = new LifeProperties(0);

            CurrentWorld = world;
        }

        public Gene[] Genes { get; }
        public NeuralNetwork NeuralNetwork { get; }
        public int Age { get; set; } = 0;
        public bool Alive { get; set; } = true;

        public Colour Colour { get; }
        public LifeProperties Properties;
        public World CurrentWorld { get; internal set; }

        private Vector2I _location;
        public Vector2I Location
        {
            get
            {
                return _location;
            }
            set
            {
                // The movement is posible - no going outside bounds or into other lifeforms
                // Also updates world location tracking
                if (CurrentWorld.MoveLifeform(this, value))
                {
                    PreLocation = _location;
                    _location = value;
                }
            }
        }
        public Vector2I PreLocation { get; private set; }

        public bool Shift(Vector2I value)
        {
            // The movement is posible - no going outside bounds or into other lifeforms
            // Also updates world location tracking
            if (CurrentWorld.MoveLifeform(this, _location + value))
            {
                PreLocation = _location;
                _location += value;

                return true;
            }

            return false;
        }

        public Lifeform CreateChild(Vector2I location, World world)
        {
            Gene[] genes = new Gene[Genes.Length];

            for (int i = 0; i < Genes.Length; i++)
            {
                genes[i] = Genes[i].CreateChild();
            }

            return new Lifeform(
                genes,
                location,
                world);
        }

        /// <summary>
        /// Calculate the actions for this <see cref="Lifeform"/> for this frame.
        /// </summary>
        public void Update()
        {
            // Lifeform is dead - won't do anything
            if (!Alive) { return; }

            Age++;

            Properties.ClearNeuronValues();

            NeuralNetwork.Compute(this);
        }

        public override bool Equals(object obj)
        {
            return obj is Lifeform l &&
                l.Genes == Genes &&
                l.NeuralNetwork == NeuralNetwork &&
                l.Age == Age &&
                l.Properties == Properties;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Genes, NeuralNetwork, Age, Properties);
        }

        public static PRNG Random { get; set; }
        public static object RandSync { get; } = new object();

        public static Lifeform Generate(PRNG random, int nGene, Vector2I location, World world)
        {
            Gene[] genes = new Gene[nGene];

            for (int i = 0; i < nGene; i++)
            {
                genes[i] = Gene.Generate(random);
            }

            return new Lifeform(
                genes, 
                location,
                world);
        }

        public static Lifeform Generate(int nGene, Vector2I location, World world)
        {
            Gene[] genes = new Gene[nGene];

            for (int i = 0; i < nGene; i++)
            {
                genes[i] = Gene.Generate();
            }

            return new Lifeform(
                genes,
                location,
                world);
        }

        public static Lifeform[] FromGenes(PRNG random, Gene[] genes, int lifeCount, int width, int height)
        {
            Vector2I[] posHierarchy = World.NoiseMap(width, height, random.Generate(0, int.MaxValue));

            Lifeform[] lifeforms = new Lifeform[lifeCount];

            for (int i = 0; i < lifeCount; i++)
            {
                lifeforms[i] = new Lifeform(genes, posHierarchy[i], null);
            }

            return lifeforms;
        }
        public static Lifeform[] FromGenes(PRNG random, Gene[][] genes, int lifeCount, int width, int height)
        {
            Vector2I[] posHierarchy = World.NoiseMap(width, height, random.Generate(0, int.MaxValue));

            Lifeform[] lifeforms = new Lifeform[lifeCount];

            for (int i = 0; i < lifeCount; i++)
            {
                lifeforms[i] = new Lifeform(genes[random.Generate(0, genes.Length - 1)], posHierarchy[i], null);
            }

            return lifeforms;
        }

        public static Lifeform Empty { get; } = Dud(Vector2I.Zero, null);
        /// <summary>
        /// Creates a lifeless lifeform.
        /// </summary>
        public static Lifeform Dud(Vector2I location, World world)
        {
            return new Lifeform(Array.Empty<Gene>(), location, world);
        }

        public static bool operator ==(Lifeform a, Lifeform b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Lifeform a, Lifeform b)
        {
            return !a.Equals(b);
        }

        static Lifeform()
        {
            ColourGrade = 15;
        }

        private static int _colourGrade;
        private static int _colourMulti;
        public static byte ColourGrade
        {
            get => (byte)(_colourGrade - 1);
            set
            {
                _colourGrade = value <= 1 ? 2 : (value + 1);

                _colourMulti = 255 / (_colourGrade - 1);
            }
        }

        public static bool OneInChance(double chance)
        {
            // Guaranteed
            if (chance >= 1) { return true; }

            bool compare;

            lock (RandSync)
            {
                compare = Random.Generate() < chance;
            }

            return compare;
        }
        private static Colour GeneColour(Gene[] genes)
        {
            int r = 0;
            int g = 0;
            uint b = 0;

            foreach (Gene gene in genes)
            {
                r += gene.Source % Neuron.SourceModifier;
                g += gene.Destination % Neuron.DestinationModifier;

                double scale = Math.Round(Neuron.GetStrength(gene.Strength));

                // Value cannot be passed through % or /
                if (scale == 0.0)
                {
                    b += gene.Strength;
                    continue;
                }

                b += gene.Strength % (uint)scale;
            }

            return new Colour(
                (byte)((r % _colourGrade) * _colourMulti),
                (byte)((g % _colourGrade) * _colourMulti),
                (byte)((b % _colourGrade) * _colourMulti));
        }
    }
}
