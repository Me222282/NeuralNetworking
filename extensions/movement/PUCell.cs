using System;
using Zene.NeuralNetworking;

namespace movement
{
    public struct PUCell : INeuronCell
    {
        public string Name => "PU_";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();

        public double GetValue(Lifeform lifeform)
        {
            return (double)lifeform.Location.Y / lifeform.CurrentWorld.Height;
        }

        public void SetValue(Lifeform lifeform, double value) => throw new NotSupportedException();
        public void Activate(Lifeform lifeform) => throw new NotSupportedException();
        
        public void Setup(NeuralNetwork network) { return; }
        
        public static void Add()
        {
            NeuralNetwork.PosibleGetCells.Add(new PUCell());
        }
    }
}
