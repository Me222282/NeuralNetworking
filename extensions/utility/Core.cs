namespace utility
{
    public static class Core
    {
        private static readonly string[] _cellNames = new string[]
        {
            nameof(CosCell),
            nameof(RandCell),
            nameof(SinCell),
            nameof(TimeCell)
        };

        public static string[] GetCellNames() => _cellNames;
    }
}