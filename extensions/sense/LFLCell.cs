using System;
using Zene.NeuralNetworking;
using Zene.Structs;

namespace sense
{
    public struct LFLCell : INeuronCell
    {
        public string Name => "LFL";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();

        internal const int SenseDistance = 10;

        public double GetValue(Lifeform lifeform)
        {
            World w = lifeform.CurrentWorld;
            Vector2I pos = lifeform.Location;

            int count = SenseDistance;
            int stop = pos.X - SenseDistance;
            // Doesn't go out of bounds
            if (stop < 0)
            {
                stop = 0;
            }
            for (int x = pos.X - 1; x >= stop; x--)
            {
                if (ContainsLifeform(w, x, pos.Y)) { break; }

                count--;
            }

            return (double)count / SenseDistance;
        }

        internal static bool ContainsLifeform(World w, int x, int y)
        {
            if (w.Width <= x || x < 0 ||
                w.Height <= y || y < 0)
            {
                return false;
            }

            return w.LifeformGrid[x, y] is not null;
        }

        public void SetValue(Lifeform lifeform, double value) => throw new NotSupportedException();
        public void Activate(Lifeform lifeform) => throw new NotSupportedException();

        public static void Add()
        {
            NeuralNetwork.PosibleGetCells.Add(new LFLCell());
        }
    }
}
