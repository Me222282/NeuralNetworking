using System;
using Zene.NeuralNetworking;

namespace maths
{
    public struct DivCell : INeuronCell
    {
        private static int _count = 0;
        
        public DivCell(int neuronAllocant)
        {
            NeuronActiveAllocant = neuronAllocant;
            NeuronDivAllocant = neuronAllocant + 2;

            Name = $"DIV{_count}";
            _count++;
        }

        // The allocated position in the LifeformProperties.NeuronValues array.
        public readonly int NeuronDivAllocant;
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
            
            return Math.Tanh(lifeform.Properties.NeuronValues[NeuronDivAllocant]);
        }

        public void SetValue(Lifeform lifeform, double value)
        {
            if (lifeform.Properties.NeuronValues[NeuronActiveAllocant] == 0d)
            {
                lifeform.Properties.NeuronValues[NeuronActiveAllocant] = 1d;
                lifeform.Properties.NeuronValues[NeuronDivAllocant] = value;
                return;
            }
            
            lifeform.Properties.NeuronValues[NeuronDivAllocant] /= value;
        }

        public void Activate(Lifeform lifeform) { return; }
        
        public static void Add()
        {
            DivCell cell = new DivCell(LifeProperties.NeuronValueNumber);
            
            NeuralNetwork.PosibleGetCells.Add(cell);
            NeuralNetwork.PosibleSetCells.Add(cell);
            LifeProperties.NeuronValueNumber += 2;
        }
    }
}