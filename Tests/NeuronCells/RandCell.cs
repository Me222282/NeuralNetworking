using System;
using Zene.NeuralNetworking;

namespace NeuralNetworkingTest
{
    public struct RandCell : INeuronCell
    {
        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();

        public double GetValue(Lifeform lifeform) => Lifeform.Random.Generate();

        public void SetValue(Lifeform lifeform, double value)
        {
            throw new NotSupportedException();
        }

        public void Activate(Lifeform lifeform)
        {
            throw new NotSupportedException();
        }
    }
}
