using System;
using Zene.NeuralNetworking;

namespace inner
{
    internal class DataValue
    {
        public double Total;
        public int Count;
    }
    
    public struct InnerCell : INeuronCell
    {
        private static int _count = 0;

        public static NeuronType NeuronType => NeuronType.Inner;
        public static bool UseNeuronValue => true;

        public InnerCell(int neuronAllocant)
        {
            NeuronAllocant = neuronAllocant;

            Name = $"IN{_count}";
            _count++;
        }

        // The allocated position in the NeuralNetwork.NeuronData array.
        public readonly int NeuronAllocant;

        public string Name { get; }

        public int GetOrder => 10;
        public int SetOrder => 0;

        public void Setup(NeuralNetwork network)
        {
            network.NeuronData[NeuronAllocant] = new DataValue();
        }

        public double GetValue(Lifeform lifeform)
        {
            DataValue dv = lifeform.GetNeuron<DataValue>(NeuronAllocant);
            
            return dv.Total / dv.Count;
        }

        public void SetValue(Lifeform lifeform, double value)
        {
            DataValue dv = lifeform.GetNeuron<DataValue>(NeuronAllocant);
            dv.Total += value;
            dv.Count++;
        }

        public void Activate(Lifeform lifeform) { return; }
    }
}
