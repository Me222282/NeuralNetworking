using System;
using Zene.NeuralNetworking;
using Zene.Structs;

namespace sense
{
    public struct LFDCell : INeuronCell
    {
        public LFDCell(int neuron) { }
        
        public string Name => "LFD";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();
        
        public static NeuronType NeuronType => NeuronType.Getter;
        public static bool UseNeuronValue => false;
        
        public double GetValue(Lifeform lifeform)
        {
            World w = lifeform.CurrentWorld;
            Vector2I pos = lifeform.Location;

            int count = LFLCell.SenseDistance;
            int stop = pos.Y - LFLCell.SenseDistance;
            // Doesn't go out of bounds
            if (stop < 0)
            {
                stop = 0;
            }
            for (int y = pos.Y - 1; y >= stop; y--)
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
