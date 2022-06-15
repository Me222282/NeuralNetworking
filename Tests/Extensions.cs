using System;
using System.Reflection;

namespace NetworkProgram
{
    internal static class Extensions
    {
        public static bool Implements(this Type t, Type @base)
        {
            return @base.IsAssignableFrom(t);
        }

        /// <summary>
        /// Gets a <see cref="Type"/> from an <see cref="Assembly"/> with its class name.
        /// </summary>
        /// <param name="asm"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type GetType(this Assembly asm, string typeName)
        {
            Type[] types = asm.GetTypes();

            if (types.Length == 0)
            {
                return null;
            }

            int i = Array.FindIndex(types, t => t.Name == typeName);

            return types[i];
        }
    }
}
