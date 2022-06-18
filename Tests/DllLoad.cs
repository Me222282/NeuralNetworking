using System;
using System.Reflection;
using Zene.NeuralNetworking;

namespace NetworkProgram
{
    public class DllLoad
    {
        private DllLoad(LifeformCondition checkLF, Assembly asm, string[] cellNames, Action[] cellAdds)
        {
            _assembly = asm;

            CanCheckLifeform = checkLF is not null;
            CheckLifeform = checkLF;

            ContainsCells = cellNames is not null && cellAdds is not null;
            if (ContainsCells &&
                cellAdds.Length != cellNames.Length)
            {
                throw new ArgumentException($"The length of {nameof(cellNames)} must be equal to the length of {nameof(cellAdds)}.");
            }

            _cellAdds = cellAdds;
            CellNames = cellNames;
        }

        private readonly Assembly _assembly;

        public string Path => _assembly.Location;

        public string Name => _assembly.GetName().Name;

        public bool CanCheckLifeform { get; }
        public LifeformCondition CheckLifeform { get; }

        public bool ContainsCells { get; }
        private readonly Action[] _cellAdds;

        public string[] CellNames { get; }

        public void AddCell(int index)
        {
            if (!ContainsCells)
            {
                throw new Exception($"This assembly doesn't contain an {nameof(INeuronCell)}.");
            }
            if (index >= _cellAdds.Length)
            {
                throw new IndexOutOfRangeException();
            }

            _cellAdds[index].Invoke();
        }

        public static DllLoad LoadDll(string path)
        {
            Assembly assembly = Assembly.LoadFrom(path);
            Type coreT = Extensions.GetType(assembly,"Core");

            if (coreT is null)
            {
                throw new Exception("Assembly must contain Type \"Core\".");
            }

            LifeformCondition checkLF = LoadCheckLF(coreT);

            string[] cells = GetCellNames(coreT);
            Action[] cellAdds = null;

            if (cells is not null)
            {
                cellAdds = new Action[cells.Length];

                for (int i = 0; i < cells.Length; i++)
                {
                    cellAdds[i] = GetCellAddMethod(assembly.GetType(cells[i]));
                }
            }

            return new DllLoad(checkLF, assembly, cells, cellAdds);
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
        private static string[] GetCellNames(Type core)
        {
            if (core is null)
            {
                throw new ArgumentNullException(nameof(core));
            }

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
        private static Action GetCellAddMethod(Type type)
        {
            if (type is null)
            {
                throw new Exception($"{type.FullName} doesn't exist.");
            }

            MethodInfo mi = type.GetMethod("Add");

            if (mi is null)
            {
                throw new Exception($"{nameof(INeuronCell)} doesn't contain \"Add\" method.");
            }

            if (mi.ReturnType != typeof(void))
            {
                throw new Exception("Invalid \"Add\" method. It must return void.");
            }

            if (mi.GetGenericArguments().Length > 0)
            {
                throw new Exception($"\"Add\" method must not have any arguments.");
            }

            return mi.CreateDelegate<Action>();
        }
    }
}
