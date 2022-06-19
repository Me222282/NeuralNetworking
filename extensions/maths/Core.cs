namespace maths
{
    public static class Core
    {
        private static readonly string[] _cellNames = new string[]
        {
            nameof(AddCell),
            nameof(SubCell),
            nameof(MultiCell),
            nameof(DivCell),
            nameof(Const1Cell),
            nameof(Const_1Cell)
        };

        public static string[] GetCellNames() => _cellNames;
    }
}