using System;
using Zene.NeuralNetworking;

namespace movement
{
    public struct PRCell : INeuronCell
    {
        public string Name => "PR_";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();

        public double GetValue(Lifeform lifeform)
        {
            return (double)lifeform.Location.X / lifeform.CurrentWorld.Width;
        }

        public void SetValue(Lifeform lifeform, double value) => throw new NotSupportedException();
        public void Activate(Lifeform lifeform) => throw new NotSupportedException();

        public static void Add()
        {
            NeuralNetwork.PosibleGetCells.Add(new PRCell());
        }
    }
}
