using System;
using Zene.NeuralNetworking;

namespace maths
{
    public struct AddCell : INeuronCell
    {
        private static int _count = 0;
        
        public AddCell(int neuronAllocant)
        {
            NeuronAllocant = neuronAllocant;
            
            GetOrder = neuronAllocant;
            SetOrder = -neuronAllocant;

            Name = $"ADD{_count}";
            _count++;
        }

        // The allocated position in the LifeformProperties.NeuronValues array.
        public readonly int NeuronAllocant;

        public string Name { get; }

        public int GetOrder { get; }
        public int SetOrder { get; }
        
        public static NeuronType NeuronType => NeuronType.Inner;
        public static bool UseNeuronValue => true;
        
        public double GetValue(Lifeform lifeform) => lifeform.GetNeuron<double>(NeuronAllocant);

        public void SetValue(Lifeform lifeform, double value)
        {
            lifeform.SetNeuron(
                NeuronAllocant,
                lifeform.GetNeuron<double>(NeuronAllocant) + value
            );
        }

        public void Activate(Lifeform lifeform) { return; }
        
        public void Setup(NeuralNetwork network)
        {
            network.NeuronData[NeuronAllocant] = new double();
        }
    }
}