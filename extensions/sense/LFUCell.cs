using System;
using Zene.NeuralNetworking;
using Zene.Structs;

namespace sense
{
    public struct LFUCell : INeuronCell
    {
        public LFUCell(int neuron) { }
        
        public string Name => "LFU";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();
        
        public static NeuronType NeuronType => NeuronType.Getter;
        public static bool UseNeuronValue => false;
        
        public double GetValue(Lifeform lifeform)
        {
            World w = lifeform.CurrentWorld;
            Vector2I pos = lifeform.Location;

            int count = LFLCell.SenseDistance;
            int stop = pos.Y + LFLCell.SenseDistance;
            // Doesn't go out of bounds
            if (stop >= w.Height)
            {
                stop = w.Height - 1;
            }
            for (int y = pos.Y + 1; y <= stop; y++)
            {
                if (LFLCell.ContainsLifeform(w, pos.X, y)) { break; }

                count--;
            }

            return (double)count / LFLCell.SenseDistance;
        }

        public void SetValue(Lifeform lifeform, double value) => throw new NotSupportedException();
        public void Activate(Lifeform lifeform) => throw new NotSupportedException();
        
        public void Setup(NeuralNetwork network) { return; }
    }
}
