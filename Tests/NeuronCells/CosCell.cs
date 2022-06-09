﻿using System;
using Zene.NeuralNetworking;
using Zene.Structs;

namespace NeuralNetworkingTest
{
    public struct CosCell : INeuronCell
    {
        public string Name => "COS";

        public int GetOrder => 0;
        public int SetOrder => throw new NotSupportedException();

        public double GetValue(Lifeform lifeform) => Math.Cos(lifeform.CurrentWorld.Time);

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
