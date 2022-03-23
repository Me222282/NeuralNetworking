using System;
using System.Collections.Generic;

namespace Zene.NeuralNetworking
{
    public class NeuralNetwork
    {
        private NeuralNetwork(Neuron[] neurons)
        {
            List<Neuron> neuronList = new List<Neuron>(neurons);

            neuronList.Sort(CompareNeuron);

            Neurons = neuronList.ToArray();
        }

        private static int CompareNeuron(Neuron x, Neuron y)
        {
            if (x.Source.GetOrder > y.Source.GetOrder)
            {
                return 1;
            }
            else if (x.Source.GetOrder < y.Source.GetOrder)
            {
                return -1;
            }
            else
            {
                if (x.Destination.SetOrder > y.Destination.SetOrder)
                {
                    return 1;
                }
                else if (x.Destination.SetOrder < y.Destination.SetOrder)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// List of <see cref="Neuron"/> in process order.
        /// </summary>
        public Neuron[] Neurons { get; }

        /// <summary>
        /// Compute the actions of a <see cref="Lifeform"/> for this frame.
        /// </summary>
        /// <param name="lifeform"></param>
        public void Compute(Lifeform lifeform)
        {
            // Calculate values
            for (int i = 0; i < Neurons.Length; i++)
            {
                Neurons[i].Activate(lifeform);
            }
            // Apply outputs
            for (int i = 0; i < PosibleSetCells.Count; i++)
            {
                PosibleSetCells[i].Activate(lifeform);
            }
        }

        /// <summary>
        /// Create a <see cref="NeuralNetwork"/> from a list of <see cref="Gene"/>.
        /// </summary>
        /// <param name="genes">The list from which to create a <see cref="NeuralNetwork"/>.</param>
        public static NeuralNetwork Generate(Gene[] genes)
        {
            Neuron[] neurons = new Neuron[genes.Length];

            for (int i = 0; i < genes.Length; i++)
            {
                neurons[i] = new Neuron(genes[i]);
            }

            return new NeuralNetwork(neurons);
        }

        /// <summary>
        /// Creates an empty brain.
        /// </summary>
        /// <returns></returns>
        public static NeuralNetwork Empty()
        {
            return new NeuralNetwork(Array.Empty<Neuron>());
        }

        /// <summary>
        /// The list of posible <see cref="INeuronCell"/> that have get values.
        /// </summary>
        public static List<INeuronCell> PosibleGetCells { get; } = new List<INeuronCell>();
        /// <summary>
        /// The list of posible <see cref="INeuronCell"/> that have set values.
        /// </summary>
        public static List<INeuronCell> PosibleSetCells { get; } = new List<INeuronCell>();
    }

    public struct Neuron
    {
        public Neuron(Gene gene)
        {
            //DataSource = gene;

            // Source neuron
            int sN = gene.Source % NeuralNetwork.PosibleGetCells.Count;
            Source = NeuralNetwork.PosibleGetCells[sN];

            // Destination neuron
            int dN = gene.Destination % NeuralNetwork.PosibleSetCells.Count;
            Destination = NeuralNetwork.PosibleSetCells[dN];

            // Scale the strength value to a number between -MaxScale and +MaxScale
            Scale = GetScale(gene.Strength);
        }

        public INeuronCell Source { get; }
        public INeuronCell Destination { get; }
        public double Scale { get; }

        //public Gene DataSource { get; }

        static Neuron()
        {
            MaxScale = 5;
        }

        private static double _maxScale;
        /// <summary>
        /// The maximum scale factor of a neuron
        /// </summary>
        public static double MaxScale
        {
            get => _maxScale;
            set
            {
                _maxScale = value;

                StrengthPerScale = (uint)(uint.MaxValue / (value + value));
            }
        }
        /// <summary>
        /// The size change in <see cref="Gene.Strength"/> required to change <see cref="Scale"/> by 1.0.
        /// </summary>
        public static uint StrengthPerScale { get; private set; }

        public static ushort SourceModifier => (ushort)NeuralNetwork.PosibleGetCells.Count;
        public static ushort DestinationModifier => (ushort)NeuralNetwork.PosibleSetCells.Count;

        /// <summary>
        /// Perform the per frame calculation for this <see cref="Neuron"/>.
        /// </summary>
        /// <param name="lifeform">The lifeform this <see cref="Neuron"/> is a part of</param>
        public void Activate(Lifeform lifeform)
        {
            double value = Source.GetValue(lifeform);

            value *= Scale;

            Destination.SetValue(lifeform, value);
        }

        private const double _uintMax = uint.MaxValue;

        public static double GetScale(uint value)
        {
            // Find scale between uint bounds
            double scale = value / _uintMax;

            return ((MaxScale + MaxScale) * scale) - MaxScale;
        }
        public static uint GetStrength(double value)
        {
            // Make sure in bounds
            double boundVal = Math.Clamp(value, -MaxScale, MaxScale);

            // Make it so min value is now 0 and max relative
            boundVal += MaxScale;
            double max = MaxScale + MaxScale;

            double scale = boundVal / max;

            return (uint)Math.Round(scale * uint.MaxValue);
        }
    }
}
