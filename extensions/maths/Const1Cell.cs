using System;
using Zene.NeuralNetworking;

namespace maths
{
    public struct Const1Cell : INeuronCell
    {
        public Const1Cell(int neuron) { }
        
        public string Name => "CONST1";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();
        
        public static NeuronType NeuronType => NeuronType.Getter;
        public static bool UseNeuronValue => false;
        
        public double GetValue(Lifeform lifeform) => 1d;

        public void SetValue(Lifeform lifeform, double value) => throw new NotSupportedException();
        public void Activate(Lifeform lifeform) => throw new NotSupportedException();
        
        public void Setup(NeuralNetwork network) { return; }
    }
}