namespace sense
{
    public static class Core
    {
        private static readonly string[] _cellNames = new string[]
        {
            nameof(LFLCell),
            nameof(LFRCell),
            nameof(LFUCell),
            nameof(LFDCell)
        };

        public static string[] GetCellNames() => _cellNames;
    }
}