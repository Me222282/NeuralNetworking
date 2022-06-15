using System;
using Zene.NeuralNetworking;
using Zene.Structs;

namespace NetworkProgram
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

            int count = 0;
            int stop = pos.X - SenseDistance;
            // Doesn't go out of bounds
            if (stop < 0)
            {
                stop = 0;
            }
            for (int x = pos.X - 1; x >= stop; x--)
            {
                if (ContainsLifeform(w, x, pos.Y)) { break; }

                count++;
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

    public struct LFRCell : INeuronCell
    {
        public string Name => "LFR";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();

        public double GetValue(Lifeform lifeform)
        {
            World w = lifeform.CurrentWorld;
            Vector2I pos = lifeform.Location;

            int count = 0;
            int stop = pos.X + LFLCell.SenseDistance;
            // Doesn't go out of bounds
            if (stop >= w.Width)
            {
                stop = w.Width - 1;
            }
            for (int x = pos.X - 1; x <= stop; x++)
            {
                if (LFLCell.ContainsLifeform(w, x, pos.Y)) { break; }

                count++;
            }

            return (double)count / LFLCell.SenseDistance;
        }

        public void SetValue(Lifeform lifeform, double value) => throw new NotSupportedException();
        public void Activate(Lifeform lifeform) => throw new NotSupportedException();

        public static void Add()
        {
            NeuralNetwork.PosibleGetCells.Add(new LFRCell());
        }
    }

    public struct LFUCell : INeuronCell
    {
        public string Name => "LFU";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();

        public double GetValue(Lifeform lifeform)
        {
            World w = lifeform.CurrentWorld;
            Vector2I pos = lifeform.Location;

            int count = 0;
            int stop = pos.Y + LFLCell.SenseDistance;
            // Doesn't go out of bounds
            if (stop >= w.Height)
            {
                stop = w.Height - 1;
            }
            for (int y = pos.Y - 1; y <= stop; y++)
            {
                if (LFLCell.ContainsLifeform(w, pos.X, y)) { break; }

                count++;
            }

            return (double)count / LFLCell.SenseDistance;
        }

        public void SetValue(Lifeform lifeform, double value) => throw new NotSupportedException();
        public void Activate(Lifeform lifeform) => throw new NotSupportedException();

        public static void Add()
        {
            NeuralNetwork.PosibleGetCells.Add(new LFUCell());
        }
    }

    public struct LFDCell : INeuronCell
    {
        public string Name => "LFD";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();

        public double GetValue(Lifeform lifeform)
        {
            World w = lifeform.CurrentWorld;
            Vector2I pos = lifeform.Location;

            int count = 0;
            int stop = pos.Y - LFLCell.SenseDistance;
            // Doesn't go out of bounds
            if (stop < 0)
            {
                stop = 0;
            }
            for (int y = pos.Y - 1; y >= stop; y--)
            {
                if (LFLCell.ContainsLifeform(w, pos.X, y)) { break; }

                count++;
            }

            return (double)count / LFLCell.SenseDistance;
        }

        public void SetValue(Lifeform lifeform, double value) => throw new NotSupportedException();
        public void Activate(Lifeform lifeform) => throw new NotSupportedException();

        public static void Add()
        {
            NeuralNetwork.PosibleGetCells.Add(new LFDCell());
        }
    }
}
