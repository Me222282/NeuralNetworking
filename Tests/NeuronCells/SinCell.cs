using System;
using Zene.NeuralNetworking;
using Zene.Structs;

namespace NetworkProgram
{
    public struct SinCell : INeuronCell
    {
        public string Name => "SIN";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();

        public double GetValue(Lifeform lifeform) => Math.Sin(lifeform.CurrentWorld.Time);

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
