using Zene.NeuralNetworking;
using Zene.Structs;

namespace neighbours
{
    public static class Core
    {
        public static bool CheckLifeform(Lifeform lifeform)
        {
            // Have an odd number of neighbours
            World world = lifeform.CurrentWorld;
            Vector2I pos = lifeform.Location;

            int neighbours = 0;

            if (pos.X + 1 < world.Width)
            {
                neighbours += world.LifeformGrid[pos.X + 1, pos.Y] is null ? 0 : 1;
            }
            if (pos.Y + 1 < world.Height)
            {
                neighbours += world.LifeformGrid[pos.X, pos.Y + 1] is null ? 0 : 1;
            }
            if (pos.X - 1 >= 0)
            {
                neighbours += world.LifeformGrid[pos.X - 1, pos.Y] is null ? 0 : 1;
            }
            if (pos.Y - 1 >= 0)
            {
                neighbours += world.LifeformGrid[pos.X, pos.Y - 1] is null ? 0 : 1;
            }

            return neighbours % 2 == 1;
        }
    }
}