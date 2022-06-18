using Zene.NeuralNetworking;

namespace centre
{
    public static class Core
    {
        public static bool CheckLifeform(Lifeform lifeform)
        {
            // Get to corners
            //return (lifeform.Location.X < (lifeform.CurrentWorld.Width / 4) && (lifeform.Location.Y < (lifeform.CurrentWorld.Height / 4))) ||
            //    (lifeform.Location.X > (lifeform.CurrentWorld.Width - (lifeform.CurrentWorld.Width / 4)) && (lifeform.Location.Y > (lifeform.CurrentWorld.Height - (lifeform.CurrentWorld.Height / 4)))) ||
            //    (lifeform.Location.X > (lifeform.CurrentWorld.Width - (lifeform.CurrentWorld.Width / 4)) && (lifeform.Location.Y < (lifeform.CurrentWorld.Height / 4))) ||
            //    (lifeform.Location.X < (lifeform.CurrentWorld.Width / 4) && (lifeform.Location.Y > (lifeform.CurrentWorld.Height - (lifeform.CurrentWorld.Height / 4))));
            
            // Get to the centre
            return (lifeform.Location.X > (lifeform.CurrentWorld.Width / 4)) &&
                (lifeform.Location.X < (lifeform.CurrentWorld.Width - (lifeform.CurrentWorld.Width / 4))) &&
                (lifeform.Location.Y > (lifeform.CurrentWorld.Height / 4)) &&
                (lifeform.Location.Y < (lifeform.CurrentWorld.Height - (lifeform.CurrentWorld.Height / 4)));
        }
    }
}