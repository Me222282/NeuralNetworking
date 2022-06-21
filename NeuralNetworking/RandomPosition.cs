using System.Linq;
using Zene.Structs;

namespace Zene.NeuralNetworking
{
    public class RandomPosition
    {
        public RandomPosition(int width, int height, PRNG random)
        {
            _random = random;

            _count = width * height;
            _positions = new Vector2I[_count];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _positions[(x * width) + y] = new Vector2I(x, y);
                }
            }

            _reference = Enumerable.Range(0, _count).ToArray();
            _count--;
        }

        private readonly Vector2I[] _positions;
        private readonly PRNG _random;

        private int[] _reference;
        private int _count;

        public Vector2I Next()
        {
            int index = _random.Generate(0, _count);

            int posI = _reference[index];
            _reference[index] = _reference[_count];
            _count--;

            return _positions[posI];
        }

        public void Reset()
        {
            _count = _reference.Length;
            _reference = Enumerable.Range(0, _count).ToArray();
            _count--;
        }
    }
}
