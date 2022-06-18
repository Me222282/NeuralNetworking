using System;
using Zene.NeuralNetworking;

namespace maths
{
    public struct SubCell : INeuronCell
    {
        private static int _count = 0;
        
        public SubCell(int neuronAllocant)
        {
            NeuronActiveAllocant = neuronAllocant;
            NeuronBaseAllocant = neuronAllocant + 1;
            NeuronSubAllocant = neuronAllocant + 2;

            Name = $"SUB{_count}";
            _count++;
        }

        // The allocated position in the LifeformProperties.NeuronValues array.
        public readonly int NeuronBaseAllocant;
        public readonly int NeuronSubAllocant;
        public readonly int NeuronActiveAllocant;

        public string Name { get; }

        public int GetOrder => 10;
        public int SetOrder => 0;

        public double GetValue(Lifeform lifeform)
        {
            if (lifeform.Properties.NeuronValues[NeuronActiveAllocant] == 0d)
            {
                return 0;
            }
            
            return Math.Tanh(
                lifeform.Properties.NeuronValues[NeuronBaseAllocant] - 
                lifeform.Properties.NeuronValues[NeuronSubAllocant]
            );
        }

        public void SetValue(Lifeform lifeform, double value)
        {
            if (lifeform.Properties.NeuronValues[NeuronActiveAllocant] == 0d)
            {
                lifeform.Properties.NeuronValues[NeuronActiveAllocant] = 1d;
                lifeform.Properties.NeuronValues[NeuronBaseAllocant] = value;
                return;
            }
            
            lifeform.Properties.NeuronValues[NeuronSubAllocant] += value;
        }

        public void Activate(Lifeform lifeform) { return; }
        
        public static void Add()
        {
            SubCell cell = new SubCell(LifeProperties.NeuronValueNumber);
            
            NeuralNetwork.PosibleGetCells.Add(cell);
            NeuralNetwork.PosibleSetCells.Add(cell);
            LifeProperties.NeuronValueNumber += 3;
        }
    }
}