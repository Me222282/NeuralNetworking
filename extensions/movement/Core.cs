namespace movement
{
    public static class Core
    {
        private static readonly string[] _cellNames = new string[]
        {
            nameof(PDCell),
            nameof(PLCell),
            nameof(PRCell),
            nameof(PUCell),
            nameof(XMCell),
            nameof(XPCell),
            nameof(YMCell),
            nameof(YPCell)
        };

        public static string[] GetCellNames() => _cellNames;
    }
}