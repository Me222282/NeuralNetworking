using System;
using FileEncoding;
using Zene.Structs;

namespace NetworkProgram
{
    public class WindowOpen : BaseWindow
    {
        public WindowOpen(int width, int height, string[] titles, FramePart[][,] frames, int[] counts, int[] lives, int[] size, int[] gens)
            : base(width, height, titles[0], new Vector2I(size[0]))
        {
            _frames = frames;
            _frameCount = counts;
            _lifeCount = lives;
            _worldSize = size;
            _generation = gens;
            _titles = titles;

            Console.WriteLine($"Generation {_generation[_vidCounter]}");
        }

        private int _vidCounter = 0;

        private readonly int[] _worldSize;
        private readonly int[] _frameCount;
        private readonly int[] _lifeCount;
        private readonly int[] _generation;
        private readonly FramePart[][,] _frames;
        private readonly string[] _titles;

        protected override void Update()
        {
            if (Counter >= _frameCount[_vidCounter])
            {
                Counter = 0;
                _vidCounter++;

                if (_vidCounter >= _frames.Length)
                {
                    _vidCounter = 0;
                }

                Console.WriteLine($"Generation {_generation[_vidCounter]}");

                Title = _titles[_vidCounter];
                ReferenceSize = new Vector2I(_worldSize[_vidCounter]);
            }
        }
        protected override void Render()
        {
            base.Render();

            for (int l = 0; l < _lifeCount[_vidCounter]; l++)
            {
                FramePart fp = _frames[_vidCounter][Counter, l];

                if (!fp.Alive) { continue; }

                DrawLifeform(fp.Position, fp.Colour);
            }
        }
    }
}
