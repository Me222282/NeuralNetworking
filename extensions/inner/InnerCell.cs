﻿using System;
using Zene.NeuralNetworking;

namespace inner
{
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
            network.NeuronData[NeuronAllocant] = new double();
        }

        public double GetValue(Lifeform lifeform) => Math.Tanh(lifeform.GetNeuron<double>(NeuronAllocant));

        public void SetValue(Lifeform lifeform, double value)
        {
            lifeform.SetNeuron(NeuronAllocant, lifeform.GetNeuron<double>(NeuronAllocant) + value);
        }

        public void Activate(Lifeform lifeform) { return; }
    }
}