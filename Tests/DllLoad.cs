using System;
using System.Reflection;
using Zene.NeuralNetworking;

namespace NetworkProgram
{
    public class DllLoad
    {
        private DllLoad(LifeformCondition checkLF, Assembly asm, string[] cellNames, Cell[] cells)
        {
            _assembly = asm;

            CanCheckLifeform = checkLF is not null;
            CheckLifeform = checkLF;

            ContainsCells = cellNames is not null && cells is not null;
            if (ContainsCells &&
                cells.Length != cellNames.Length)
            {
                throw new ArgumentException($"The length of {nameof(cellNames)} must be equal to the length of {nameof(cells)}.");
            }

            _cells = cells;
            CellNames = cellNames;
        }

        private readonly Assembly _assembly;

        public string Path => _assembly.Location;

        public string Name => _assembly.GetName().Name;

        public bool CanCheckLifeform { get; }
        public LifeformCondition CheckLifeform { get; }

        public bool ContainsCells { get; }
        private readonly Cell[] _cells;

        public string[] CellNames { get; }

        public void AddCell(int index)
        {
            if (!ContainsCells)
            {
                throw new Exception($"This assembly doesn't contain an {nameof(INeuronCell)}.");
            }
            if (index >= _cells.Length)
            {
                throw new IndexOutOfRangeException();
            }

            INeuronCell nc = (INeuronCell)_cells[index].ci.Invoke(new object[] { NeuralNetwork.NeuronValueCount });
            if (_cells[index].nv)
            {
                NeuralNetwork.NeuronValueCount++;
            }

            switch (_cells[index].nt)
            {
                case NeuronType.Getter:
                    NeuralNetwork.PosibleGetCells.Add(nc);
                    return;

                case NeuronType.Setter:
                    NeuralNetwork.PosibleSetCells.Add(nc);
                    return;

                case NeuronType.Inner:
                    NeuralNetwork.PosibleGetCells.Add(nc);
                    NeuralNetwork.PosibleSetCells.Add(nc);
                    return;

                default:
                    throw new Exception($"Invalid {nameof(NeuronType)} at index {index}.");
            }
        }

        public static DllLoad LoadDll(string path)
        {
            Assembly assembly = Assembly.LoadFrom(path);
            Type coreT = Extensions.GetType(assembly, "Core");

            if (coreT is null)
            {
                throw new Exception("Assembly must contain Type \"Core\".");
            }

            LifeformCondition checkLF = LoadCheckLF(coreT);

            string[] cellNames = GetCellNames(coreT, out string space);
            Cell[] cells = null;

            if (cellNames is not null)
            {
                cells = new Cell[cellNames.Length];

                for (int i = 0; i < cellNames.Length; i++)
                {
                    ConstructorInfo ci = GetCellData(assembly.GetType($"{space}.{cellNames[i]}"), out NeuronType type, out bool nv);
                    cells[i] = new Cell(ci, type, nv);
                }
            }

            return new DllLoad(checkLF, assembly, cellNames, cells);
        }
        private static LifeformCondition LoadCheckLF(Type core)
        {
            if (core is null)
            {
                throw new ArgumentNullException(nameof(core));
            }

            MethodInfo mi = core.GetMethod("CheckLifeform");

            if (mi == null) { return null; }

            if (mi.ReturnType != typeof(bool))
            {
                throw new Exception("Invalid \"CheckLifeform\" method. It must return a boolean.");
            }

            ParameterInfo[] args = mi.GetParameters();

            if (args.Length != 1 ||
                args[0].ParameterType != typeof(Lifeform))
            {
                throw new Exception($"\"CheckLifeform\" method must have {typeof(Lifeform).FullName} as the only argument.");
            }

            return mi.CreateDelegate<LifeformCondition>();
        }
        private static string[] GetCellNames(Type core, out string @namespace)
        {
            if (core is null)
            {
                throw new ArgumentNullException(nameof(core));
            }

            @namespace = core.Namespace;

            MethodInfo mi = core.GetMethod("GetCellNames");

            if (mi == null) { return null; }

            if (mi.ReturnType != typeof(string[]))
            {
                throw new Exception("Invalid \"GetCellNames\" method. It must return a string array.");
            }

            if (mi.GetGenericArguments().Length > 0)
            {
                throw new Exception($"\"GetCellNames\" method must not have any arguments.");
            }

            return (string[])mi.Invoke(null, null);
        }
        private static ConstructorInfo GetCellData(Type type, out NeuronType nt, out bool nv)
        {
            if (type is null)
            {
                throw new Exception($"Type doesn't exist.");
            }

            if (!typeof(INeuronCell).IsAssignableFrom(type))
            {
                throw new Exception($"{type.FullName} must inherit interface {nameof(INeuronCell)}.");
            }

            // NeuronType propery
            PropertyInfo piNt = type.GetProperty("NeuronType");

            if (piNt is null )
            {
                throw new Exception($"{nameof(INeuronCell)} doesn't contain property \"NeuronType\".");
            }

            if (piNt.PropertyType != typeof(NeuronType))
            {
                throw new Exception($"Invalid property \"NeuronType\". It must by of type {nameof(NeuronType)}.");
            }

            nt = (NeuronType)piNt.GetValue(null);

            // UseNeuronValue propery
            PropertyInfo piNv = type.GetProperty("UseNeuronValue");

            if (piNv is null)
            {
                throw new Exception($"{nameof(INeuronCell)} doesn't contain property \"UseNeuronValue\".");
            }

            if (piNv.PropertyType != typeof(bool))
            {
                throw new Exception($"Invalid property \"NeuronType\". It must by of type {nameof(Boolean)}.");
            }

            nv = (bool)piNv.GetValue(null);

            // Constructor
            ConstructorInfo ci = type.GetConstructor(_constructTypes);

            if (ci is null)
            {
                throw new Exception($"{nameof(INeuronCell)} doesn't contain a constructor with parameter {nameof(Int32)}.");
            }

            return ci;
        }

        private static readonly Type[] _constructTypes = new Type[] { typeof(int) };

        private struct Cell
        {
            public Cell(ConstructorInfo ci, NeuronType nt, bool nv)
            {
                this.ci = ci;
                this.nt = nt;
                this.nv = nv;
            }

            public ConstructorInfo ci;
            public NeuronType nt;
            public bool nv;
        }
    }
}
