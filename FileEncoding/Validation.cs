using System;
using System.IO;

namespace FileEncoding
{
    public struct Validation
    {
        public Validation(string str)
        {
            if (str.Length != 8)
            {
                throw new Exception();
            }

            One = (byte)str[0];
            Two = (byte)str[1];
            Three = (byte)str[2];
            Four = (byte)str[3];
            Five = (byte)str[4];
            Six = (byte)str[5];
            Seven = (byte)str[6];
            Eight = (byte)str[7];
        }

        internal byte One;
        internal byte Two;
        internal byte Three;
        internal byte Four;
        internal byte Five;
        internal byte Six;
        internal byte Seven;
        internal byte Eight;

        public override bool Equals(object obj)
        {
            return obj is Validation v &&
                v.One == One &&
                v.Two == Two &&
                v.Three == Three &&
                v.Four == Four &&
                v.Five == Five &&
                v.Six == Six &&
                v.Seven == Seven &&
                v.Eight == Eight;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(One, Two, Three, Four, Five, Six, Seven, Eight);
        }

        public static bool operator ==(Validation left, Validation right) => left.Equals(right);
        public static bool operator !=(Validation left, Validation right) => !left.Equals(right);

        public static Validation Get(string file)
        {
            Stream stream = new FileStream(file, FileMode.Open);

            Validation v = stream.Read<Validation>();

            stream.Close();

            return v;
        }
    }
}
