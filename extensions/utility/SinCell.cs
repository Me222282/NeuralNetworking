using System;
using Zene.NeuralNetworking;

namespace utility
{
    public struct SinCell : INeuronCell
    {
        public string Name => "SIN";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();

        public double GetValue(Lifeform lifeform) => Math.Sin(lifeform.CurrentWorld.Time);

        public void SetValue(Lifeform lifeform, double value)
        {
            throw new NotSupportedException();
        }

        public void Activate(Lifeform lifeform)
        {
            throw new NotSupportedException();
        }
        
        public void Setup(NeuralNetwork network) { return; }

        public static void Add()
        {
            NeuralNetwork.PosibleGetCells.Add(new SinCell());
        }
    }
}
