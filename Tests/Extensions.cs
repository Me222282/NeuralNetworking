using System;

namespace NetworkProgram
{
    internal static class Extensions
    {
        public static bool Implements(this Type t, Type @base)
        {
            return @base.IsAssignableFrom(t);
        }
    }
}
