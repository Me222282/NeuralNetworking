using System;
using System.Collections.Generic;
using System.Reflection;
using Zene.NeuralNetworking;

namespace NetworkProgram
{
    public delegate bool CheckLifeform(Lifeform lifeform, World world);

    public class DllLoad
    {
        private DllLoad(CheckLifeform checkLF, Dictionary<string, Action> addCells)
        {
            CanCheckLifeform = checkLF is not null;
            CheckLifeform = checkLF;

            CanAddCell = addCells is not null;
            _addCells = addCells;
        }

        public bool CanCheckLifeform { get; }
        public CheckLifeform CheckLifeform { get; }

        public bool CanAddCell { get; }
        private readonly Dictionary<string, Action> _addCells;

        public void AddCell(string typeName)
        {
            if (!CanAddCell)
            {
                throw new Exception($"This assembly doesn't contain an {nameof(INeuronCell)}.");
            }

            bool exists = _addCells.TryGetValue(typeName, out Action method);

            if (!exists)
            {
                throw new ArgumentException($"No cell named {nameof(typeName)} exists in this assembly.");
            }

            method.Invoke();
        }

        public static DllLoad LoadDll(string path)
        {
            Assembly assembly = Assembly.LoadFrom(path);
            Type coreT = assembly.GetType("Core");

            if (coreT is null)
            {
                throw new Exception("Assembly must contain Type \"Core\".");
            }

            CheckLifeform checkLF = LoadCheckLF(coreT);

            string[] cells = GetCellNames(coreT);
            Dictionary<string, Action> cellAdds = null;

            if (cells is not null)
            {
                KeyValuePair<string, Action>[] cellsArray = new KeyValuePair<string, Action>[cells.Length];

                for (int i = 0; i < cells.Length; i++)
                {
                    cellsArray[i] = GetCellAddMethod(assembly.GetType(cells[i]));
                }

                cellAdds = new Dictionary<string, Action>(cellsArray);
            }

            return new DllLoad(checkLF, cellAdds);
        }
        private static CheckLifeform LoadCheckLF(Type core)
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

            Type[] args = mi.GetGenericArguments();

            if (args.Length != 2 ||
                args[0] != typeof(Lifeform) ||
                args[1] != typeof(World))
            {
                throw new Exception($"\"CheckLifeform\" method must have {nameof(Lifeform)} and {nameof(World)} as the arguments.");
            }

            return mi.CreateDelegate<CheckLifeform>();
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
        private static KeyValuePair<string, Action> GetCellAddMethod(Type type)
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

            return new KeyValuePair<string, Action>(type.Name, mi.CreateDelegate<Action>());
        }
    }
}
