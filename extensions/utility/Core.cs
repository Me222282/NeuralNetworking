namespace utility
{
    public static class Core
    {
        private static readonly string[] _cellNames = new string[]
        {
            typeof(CosCell).FullName,
            typeof(RandCell).FullName,
            typeof(SinCell).FullName,
            typeof(TimeCell).FullName
        };

        public static string[] GetCellNames() => _cellNames;
    }
}