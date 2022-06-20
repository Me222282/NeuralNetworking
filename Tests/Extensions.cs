using System;
using System.IO;
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

        public static int FindDll(this DllLoad[] dlls, string name)
            => Array.FindIndex(dlls, d => d.Name == name);

        public static bool IsDirectory(this string dir) => File.GetAttributes(dir).HasFlag(FileAttributes.Directory);
    }
}
