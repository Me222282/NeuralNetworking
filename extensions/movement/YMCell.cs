using System;
using Zene.NeuralNetworking;
using Zene.Structs;

namespace movement
{
    public struct YMCell : INeuronCell
    {
        public string Name => "YM_";

        public YMCell(int neuronAllocant)
        {
            NeuronAllocant = neuronAllocant;
        }

        // The allocated position in the LifeformProperties.NeuronValues array.
        public readonly int NeuronAllocant;

        public int GetOrder => throw new NotSupportedException();
        public int SetOrder => 1;

        public double GetValue(Lifeform lifeform) => throw new NotSupportedException();

        public void SetValue(Lifeform lifeform, double value)
        {
            lifeform.SetNeuron(NeuronAllocant, lifeform.GetNeuron<double>(NeuronAllocant) + value);
        }

        public void Activate(Lifeform lifeform)
        {
            double value = Math.Tanh(lifeform.GetNeuron<double>(NeuronAllocant));

            if (value == 0) { return; }

            if (value > 0)
            {
                if (Lifeform.OneInChance(value))
                {
                    lifeform.Shift(new Vector2I(0, 1));
                    return;
                }
            }

            if (Lifeform.OneInChance(-value))
            {
                lifeform.Shift(new Vector2I(0, -1));
            }
        }
        
        public void Setup(NeuralNetwork network)
        {
            network.NeuronData[NeuronAllocant] = new double();
        }

        public static void Add()
        {
            NeuralNetwork.PosibleSetCells.Add(new YMCell(NeuralNetwork.NeuronValueCount));
            NeuralNetwork.NeuronValueCount++;
        }
    }
}
