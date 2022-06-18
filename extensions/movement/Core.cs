namespace movement
{
    public static class Core
    {
        private static readonly string[] _cellNames = new string[]
        {
            typeof(PDCell).FullName,
            typeof(PLCell).FullName,
            typeof(PRCell).FullName,
            typeof(PUCell).FullName,
            typeof(XMCell).FullName,
            typeof(XPCell).FullName,
            typeof(YMCell).FullName,
            typeof(YPCell).FullName
        };

        public static string[] GetCellNames() => _cellNames;
    }
}