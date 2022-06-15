using Zene.NeuralNetworking;

namespace DllPreset
{
    public static class Core
    {
        private static readonly string[] _cellNames = new string[]
        {
            typeof(CosCell).FullName,
            typeof(PDCell).FullName,
            typeof(PLCell).FullName,
            typeof(PRCell).FullName,
            typeof(PUCell).FullName,
            typeof(RandCell).FullName,
            typeof(SinCell).FullName,
            typeof(TimeCell).FullName,
            typeof(XMCell).FullName,
            typeof(XPCell).FullName,
            typeof(YMCell).FullName,
            typeof(YPCell).FullName
        };

        public static string[] GetCellNames() => _cellNames;

        public static bool CheckLifeform(Lifeform lifeform)
        {
            // Get to the centre
            return (lifeform.Location.X > (lifeform.CurrentWorld.Width / 4)) &&
                (lifeform.Location.X < (lifeform.CurrentWorld.Width - (lifeform.CurrentWorld.Width / 4))) &&
                (lifeform.Location.Y > (lifeform.CurrentWorld.Height / 4)) &&
                (lifeform.Location.Y < (lifeform.CurrentWorld.Height - (lifeform.CurrentWorld.Height / 4)));
        }
    }
}
