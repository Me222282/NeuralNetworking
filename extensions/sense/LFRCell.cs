using System;
using Zene.NeuralNetworking;
using Zene.Structs;

namespace sense
{
    public struct LFRCell : INeuronCell
    {
        public LFRCell(int neuron) { }
        
        public string Name => "LFR";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();
        
        public static NeuronType NeuronType => NeuronType.Getter;
        public static bool UseNeuronValue => false;
        
        public double GetValue(Lifeform lifeform)
        {
            World w = lifeform.CurrentWorld;
            Vector2I pos = lifeform.Location;

            int count = LFLCell.SenseDistance;
            int stop = pos.X + LFLCell.SenseDistance;
            // Doesn't go out of bounds
            if (stop >= w.Width)
            {
                stop = w.Width - 1;
            }
            for (int x = pos.X + 1; x <= stop; x++)
            {
                if (LFLCell.ContainsLifeform(w, x, pos.Y)) { break; }

                count--;
            }

            return (double)count / LFLCell.SenseDistance;
        }

        public void SetValue(Lifeform lifeform, double value) => throw new NotSupportedException();
        public void Activate(Lifeform lifeform) => throw new NotSupportedException();
        
        public void Setup(NeuralNetwork network) { return; }
    }
}
