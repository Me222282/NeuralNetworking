using System;
using System.IO;

namespace FileEncoding
{
    public unsafe struct Validation
    {
        public Validation(string str)
        {
            if (str.Length != 8)
            {
                throw new Exception();
            }

            _data[0] = (byte)str[0];
            _data[1] = (byte)str[1];
            _data[2] = (byte)str[2];
            _data[3] = (byte)str[3];
            _data[4] = (byte)str[4];
            _data[5] = (byte)str[5];
            _data[6] = (byte)str[6];
            _data[7] = (byte)str[7];
        }

        private fixed byte _data[8];

        public override bool Equals(object obj)
        {
            return obj is Validation v &&
                v._data[0] == _data[0] &&
                v._data[1] == _data[1] &&
                v._data[2] == _data[2] &&
                v._data[3] == _data[3] &&
                v._data[4] == _data[4] &&
                v._data[5] == _data[5] &&
                v._data[6] == _data[6] &&
                v._data[7] == _data[7];
        }
        public override int GetHashCode() => HashCode.Combine(_data[0], _data[1], _data[2], _data[3], _data[4], _data[5], _data[6], _data[7]);

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
