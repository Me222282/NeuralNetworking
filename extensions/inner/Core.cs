namespace inner
{
    public static class Core
    {
        private static readonly string[] _cellNames = new string[]
        {
            nameof(InnerCell)
        };

        public static string[] GetCellNames() => _cellNames;
    }
}