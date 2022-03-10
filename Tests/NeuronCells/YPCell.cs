using System;
using Zene.NeuralNetworking;

namespace NeuralNetworkingTest
{
    public struct YPCell : INeuronCell
    {
        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();

        public double GetValue(Lifeform lifeform)
        {
            return (((double)lifeform.Location.Y / lifeform.CurrentWorld.Height) * 2) - 1;
            //return (double)lifeform.Location.Y / lifeform.CurrentWorld.Height;
        }

        public void SetValue(Lifeform lifeform, double value) => throw new NotSupportedException();
        public void Activate(Lifeform lifeform) => throw new NotSupportedException();
    }
}
