namespace sense
{
    public static class Core
    {
        private static readonly string[] _cellNames = new string[]
        {
            typeof(LFLCell).FullName,
            typeof(LFRCell).FullName,
            typeof(LFUCell).FullName,
            typeof(LFDCell).FullName
        };

        public static string[] GetCellNames() => _cellNames;
    }
}