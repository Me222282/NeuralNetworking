using System;
using Zene.NeuralNetworking;

namespace maths
{
    public struct SubCell : INeuronCell
    {
        private static int _count = 0;
        
        public SubCell(int neuronAllocant)
        {
            NeuronAllocant = neuronAllocant;

            Name = $"SUB{_count}";
            _count++;
        }

        // The allocated position in the LifeformProperties.NeuronValues array.
        public readonly int NeuronAllocant;

        public string Name { get; }

        public int GetOrder => 10;
        public int SetOrder => 0;

        public double GetValue(Lifeform lifeform)
        {
            if (!lifeform.GetNeuron<NeuronValue>(NeuronAllocant).Active)
            {
                return 0;
            }
            
            return Math.Tanh(
                lifeform.GetNeuron<NeuronValue>(NeuronAllocant).Value
            );
        }

        public void SetValue(Lifeform lifeform, double value)
        {
            if (!lifeform.GetNeuron<NeuronValue>(NeuronAllocant).Active)
            {
                lifeform.GetNeuron<NeuronValue>(NeuronAllocant).Active = true;
                lifeform.GetNeuron<NeuronValue>(NeuronAllocant).Value = value;
                return;
            }
            
            lifeform.GetNeuron<NeuronValue>(NeuronAllocant).Value -= value;
        }

        public void Activate(Lifeform lifeform) { return; }
        
        public void Setup(NeuralNetwork network)
        {
            network.NeuronData[NeuronAllocant] = new NeuronValue();
        }
        
        public static void Add()
        {
            SubCell cell = new SubCell(NeuralNetwork.NeuronValueCount);
            
            NeuralNetwork.PosibleGetCells.Add(cell);
            NeuralNetwork.PosibleSetCells.Add(cell);
            NeuralNetwork.NeuronValueCount += 1;
        }
    }
}