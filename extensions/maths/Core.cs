namespace maths
{
    public static class Core
    {
        private static readonly string[] _cellNames = new string[]
        {
            typeof(SubCell).FullName,
            typeof(MultiCell).FullName,
            typeof(DivCell).FullName,
            typeof(Const1Cell).FullName,
            typeof(Const_1Cell).FullName
        };

        public static string[] GetCellNames() => _cellNames;
    }
}