﻿namespace Zene.NeuralNetworking
{
    public enum NeuronType
    {
        Getter,
        Setter,
        Inner
    }

    public interface INeuronCell
    {
        public string Name { get; }

        public int GetOrder { get; }
        public int SetOrder { get; }

        public void Setup(NeuralNetwork network);

        public double GetValue(Lifeform lifeform);
        public void SetValue(Lifeform lifeform, double value);

        public void Activate(Lifeform lifeform);
    }
}
