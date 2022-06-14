using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zene.Structs;

namespace Zene.NeuralNetworking
{
    public delegate bool LifeformCondition(Lifeform lifeform, World world);

    public class World
    {
        public static bool Multithreading { get; set; } = true;

        public World(int seed, int lifeCount, int geneCount, int width, int height, int genLength)
        {
            Width = width;
            Height = height;

            Generation = 0;
            GenerationLength = genLength;

            _rect = new Rectangle(0, height - 1, width - 1, height - 1);

            Lifeforms = new Lifeform[lifeCount];

            LifeformGrid = new Lifeform[width, height];
            Vector2I[] posHierarchy = NoiseMap(width, height, seed);

            _random = new PRNG((ulong)seed);

            for (int i = 0; i < lifeCount; i++)
            {
                Vector2I pos = posHierarchy[i];

                Lifeform life = Lifeform.Generate(_random, geneCount, pos, this);

                LifeformGrid[pos.X, pos.Y] = life;
                Lifeforms[i] = life;
            }
        }
        public World(int lifeCount, int geneCount, int width, int height, int genLength)
        {
            Width = width;
            Height = height;

            Generation = 0;
            GenerationLength = genLength;

            _rect = new Rectangle(0, height - 1, width - 1, height - 1);

            Lifeforms = new Lifeform[lifeCount];

            LifeformGrid = new Lifeform[width, height];
            Vector2I[] posHierarchy = NoiseMap(width, height, Lifeform.Random.Generate(0, int.MaxValue));

            _random = Lifeform.Random;

            for (int i = 0; i < lifeCount; i++)
            {
                Vector2I pos = posHierarchy[i];

                Lifeform life = Lifeform.Generate(geneCount, pos, this);

                LifeformGrid[pos.X, pos.Y] = life;
                Lifeforms[i] = life;
            }
        }

        public World(int seed, int width, int height, Lifeform[] lifeforms, int genLength, int genStart = 0)
        {
            Width = width;
            Height = height;

            Generation = genStart;
            GenerationLength = genLength;

            _rect = new Rectangle(0, height - 1, width - 1, height - 1);

            Lifeforms = lifeforms;
            LifeformGrid = new Lifeform[width, height];

            _random = new PRNG((ulong)seed);

            for (int i = 0; i < lifeforms.Length; i++)
            {
                Vector2I pos = lifeforms[i].Location;

                if (LifeformGrid[pos.X, pos.Y] is not null)
                {
                    throw new Exception("Cannot place 2 different lifeforms in the same spot.");
                }

                lifeforms[i].CurrentWorld = this;
                LifeformGrid[pos.X, pos.Y] = lifeforms[i];
            }
        }
        public World(int width, int height, Lifeform[] lifeforms, int genLength, int genStart = 0)
        {
            Width = width;
            Height = height;

            Generation = genStart;
            GenerationLength = genLength;

            _rect = new Rectangle(0, height - 1, width - 1, height - 1);

            Lifeforms = lifeforms;
            LifeformGrid = new Lifeform[width, height];

            _random = Lifeform.Random;

            for (int i = 0; i < lifeforms.Length; i++)
            {
                Vector2I pos = lifeforms[i].Location;

                if (LifeformGrid[pos.X, pos.Y] is not null)
                {
                    throw new Exception("Cannot place 2 different lifeforms in the same spot.");
                }

                lifeforms[i].CurrentWorld = this;
                LifeformGrid[pos.X, pos.Y] = lifeforms[i];
            }
        }

        private World(int width, int height, int generation, int genLength, PRNG r)
        {
            Width = width;
            Height = height;

            Generation = generation;
            GenerationLength = genLength;

            _random = r;

            _rect = new Rectangle(0, height - 1, width - 1, height - 1);
            LifeformGrid = new Lifeform[width, height];
        }

        public Lifeform[,] LifeformGrid { get; private set; }

        public Lifeform[] Lifeforms { get; private set; }

        public int Width { get; }
        public int Height { get; }
        public int Generation { get; }

        /// <summary>
        /// Time between iterations - in seconds.
        /// </summary>
        public double DeltaTime { get; set; } = 1.0 / 16.0;
        public int CurrentIteration { get; private set; } = 0;
        public int GenerationLength { get; }
        /// <summary>
        /// Time through generation - in seconds.
        /// </summary>
        public double Time
        {
            get => DeltaTime * CurrentIteration;
        }

        private readonly Rectangle _rect;

        private readonly PRNG _random;

        public void Update()
        {
            // Trying to update at end of generation
            if (CurrentIteration == (GenerationLength - 1)) { return; }

            CurrentIteration++;

            if (Multithreading)
            {
                Parallel.ForEach(Lifeforms, life =>
                {
                    life.Update();
                });
                return;
            }

            foreach (Lifeform life in Lifeforms)
            {
                life.Update();
            }
        }

        public delegate void DrawLifeform(Lifeform lifeform);
        /// <summary>
        /// Updates all lifeforms whilst symoltaniusly drawing them with <paramref name="drawMethod"/>.
        /// </summary>
        /// <param name="drawMethod">The method to draw the lifeforms with.</param>
        public void UpdateDraw(DrawLifeform drawMethod)
        {
            // Trying to update at end of generation
            if (CurrentIteration == (GenerationLength - 1)) { return; }

            CurrentIteration++;

            foreach (Lifeform life in Lifeforms)
            {
                if (!life.Alive) { continue; }

                life.Update();
                drawMethod.Invoke(life);
            }
        }

        public bool MoveLifeform(Lifeform lifeform, Vector2I pos)
        {
            // Position is out of bounds
            if (!_rect.Contains(pos)) { return false; }
            // Position is already taken
            if (LifeformGrid[pos.X, pos.Y] is not null) { return false; }

            LifeformGrid[lifeform.Location.X, lifeform.Location.Y] = null;
            LifeformGrid[pos.X, pos.Y] = lifeform;

            return true;
        }

        public World NextGeneration(int width, int height, int lifeCount, LifeformCondition lifeformCondition)
        {
            World world = new World(width, height, Generation + 1, GenerationLength, _random);

            List<Lifeform> survivors = new List<Lifeform>();

            // Determine which lifeforms survived to be able to reproduce
            foreach (Lifeform lifeform in Lifeforms)
            {
                // Lifeform died in generation
                if (!lifeform.Alive) { continue; }
                if (!lifeformCondition.Invoke(lifeform, this)) { continue; }

                survivors.Add(lifeform);
            }

            if (survivors.Count == 0)
            {
                Console.WriteLine("Everyone died.");
                world.Lifeforms = Array.Empty<Lifeform>();
                return world;
            }

            Vector2I[] posHierarchy = NoiseMap(width, height, _random.Generate(0, int.MaxValue));
            int childCount = (int)Math.Floor((double)lifeCount / survivors.Count);
            Console.WriteLine($"Generation {Generation} - {(((double)survivors.Count / Lifeforms.Length) * 100):F2}% survived");
            world.Lifeforms = new Lifeform[lifeCount];
            // Make sure there are no position overlaps
            int count = 0;

            for (int i = 0; i < survivors.Count; i++)
            {
                for (int c = 0; c < childCount; c++)
                {
                    Vector2I pos = posHierarchy[count];

                    Lifeform life = survivors[i].CreateChild(pos, world);
                    world.LifeformGrid[pos.X, pos.Y] = life;
                    world.Lifeforms[count] = life;

                    count++;
                }
            }
            
            // Fill in missing lifeforms
            int currentLifeform = 0;
            while (count < lifeCount)
            {
                Vector2I pos = posHierarchy[count];

                Lifeform life = survivors[currentLifeform].CreateChild(pos, world);
                world.LifeformGrid[pos.X, pos.Y] = life;
                world.Lifeforms[count] = life;

                count++;
            }

            return world;
        }
        public World NextGeneration(int lifeCount, LifeformCondition lifeformCondition) => NextGeneration(Width, Height, lifeCount, lifeformCondition);
        public World NextGeneration(LifeformCondition lifeformCondition) => NextGeneration(Width, Height, Lifeforms.Length, lifeformCondition);

        public Lifeform GetLifeform(Vector2I location)
        {
            return LifeformGrid[location.X, location.Y];
        }
        public Lifeform GetLifeform(int x, int y)
        {
            return LifeformGrid[x, y];
        }

        public static Vector2I[] NoiseMap(int w, int h, int seed)
        {
            static int Compare(NoiseValue x, NoiseValue y)
            {
                return x.Value.CompareTo(y.Value);
            }

            List<NoiseValue> ptList = new List<NoiseValue>();

            Noise.NoiseGenerator oSimplexNoise = new Noise.NoiseGenerator(seed);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    double value = oSimplexNoise.Evaluate(x, y);

                    ptList.Add(new NoiseValue()
                    {
                        Location = new Vector2I(x, y),
                        Value = value
                    });
                }
            }

            ptList.Sort(Compare);

            Vector2I[] output = new Vector2I[ptList.Count];

            for (int i = 0; i < ptList.Count; i++)
            {
                output[i] = ptList[i].Location;
            }

            return output;
        }
        internal struct NoiseValue
        {
            public Vector2I Location { get; set; }
            public double Value { get; set; }
        }
    }
}
