using Zene.NeuralNetworking;
using Zene.Structs;

namespace FileEncoding
{
    public struct FramePart
    {
        public FramePart(Lifeform l)
        {
            Colour = (Colour3)l.Colour;
            Position = l.Location;
            Alive = l.Alive;
        }
        public FramePart(byte r, byte g, byte b, int x, int y, bool a)
        {
            Colour = new Colour3(r, g, b);
            Position = new Vector2I(x, y);
            Alive = a;
        }
        public FramePart(Colour c, int x, int y, bool a)
        {
            Colour = (Colour3)c;
            Position = (x, y);
            Alive = a;
        }

        public Colour3 Colour { get; }
        public Vector2I Position { get; }
        public bool Alive { get; }
    }
}
