using System;
using Zene.NeuralNetworking;

namespace movement
{
    public struct XPCell : INeuronCell
    {
        public XPCell(int neuron) { }
        
        public string Name => "XP_";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();
        
        public static NeuronType NeuronType => NeuronType.Getter;
        public static bool UseNeuronValue => false;
        
        public double GetValue(Lifeform lifeform)
        {
            return (((double)lifeform.Location.X / lifeform.CurrentWorld.Width) * 2) - 1;
            //return (double)lifeform.Location.X / lifeform.CurrentWorld.Width;
        }

        public void SetValue(Lifeform lifeform, double value) => throw new NotSupportedException();
        public void Activate(Lifeform lifeform) => throw new NotSupportedException();
        
        public void Setup(NeuralNetwork network) { return; }
    }
}
