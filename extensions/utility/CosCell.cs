using System;
using Zene.NeuralNetworking;

namespace utility
{
    public struct CosCell : INeuronCell
    {
        public CosCell(int neuron) { }
        
        public string Name => "COS";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();
        
        public static NeuronType NeuronType => NeuronType.Getter;
        public static bool UseNeuronValue => false;
        
        public double GetValue(Lifeform lifeform) => Math.Cos(lifeform.CurrentWorld.Time);

        public void SetValue(Lifeform lifeform, double value)
        {
            throw new NotSupportedException();
        }

        public void Activate(Lifeform lifeform)
        {
            throw new NotSupportedException();
        }
        
        public void Setup(NeuralNetwork network) { return; }
    }
}
