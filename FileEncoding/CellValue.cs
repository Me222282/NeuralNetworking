namespace FileEncoding
{
    public struct CellValue
    {
        public CellValue(int dllIndex, int cellIndex, int count)
        {
            DllIndex = dllIndex;
            CellIndex = cellIndex;
            Count = count;
        }

        public int DllIndex { get; }
        public int CellIndex { get; }
        public int Count { get; }
    }
}
