using System;
using Zene.NeuralNetworking;

namespace maths
{
    public struct MultiCell : INeuronCell
    {
        private static int _count = 0;
        
        public MultiCell(int neuronAllocant)
        {
            NeuronAllocant = neuronAllocant;
            
            GetOrder = neuronAllocant;
            SetOrder = -neuronAllocant;

            Name = $"MUT{_count}";
            _count++;
        }

        // The allocated position in the LifeformProperties.NeuronValues array.
        public readonly int NeuronAllocant;

        public string Name { get; }

        public int GetOrder { get; }
        public int SetOrder { get; }
        
        public static NeuronType NeuronType => NeuronType.Inner;
        public static bool UseNeuronValue => true;
        
        public double GetValue(Lifeform lifeform)
        {
            if (!lifeform.GetNeuron<NeuronValue>(NeuronAllocant).Active)
            {
                return 0;
            }
            
            return lifeform.GetNeuron<NeuronValue>(NeuronAllocant).Value;
        }

        public void SetValue(Lifeform lifeform, double value)
        {
            if (!lifeform.GetNeuron<NeuronValue>(NeuronAllocant).Active)
            {
                lifeform.GetNeuron<NeuronValue>(NeuronAllocant).Active = true;
                lifeform.GetNeuron<NeuronValue>(NeuronAllocant).Value = value;
                return;
            }
            
            lifeform.GetNeuron<NeuronValue>(NeuronAllocant).Value *= value;
        }

        public void Activate(Lifeform lifeform) { return; }
        
        public void Setup(NeuralNetwork network)
        {
            network.NeuronData[NeuronAllocant] = new NeuronValue();
        }
    }
}