using System;
using FileEncoding;
using Zene.Structs;

namespace NetworkProgram
{
    public class WindowOpen : BaseWindow
    {
        public WindowOpen(int width, int height, string[] titles, Settings settings, GenFile[] genFiles)
            : base(width, height, titles[0], settings, new Vector2I(genFiles[0].WorldSize))
        {
            _genFiles = genFiles;
            _titles = titles;

            _current = genFiles[0];
            Console.WriteLine($"Generation {_current.Generation}");
        }

        private int _fileIndex = 0;

        private readonly GenFile[] _genFiles;
        private readonly string[] _titles;

        private GenFile _current;

        protected override void UpdateData()
        {
            if (Counter >= _current.FrameCount)
            {
                Counter = 0;
                _fileIndex++;

                if (_fileIndex >= _genFiles.Length)
                {
                    _fileIndex = 0;
                }

                _current = _genFiles[_fileIndex];

                Console.WriteLine($"Generation {_current.Generation}");

                Title = _titles[_fileIndex];
                ReferenceSize = new Vector2I(_current.WorldSize);
                CalculateViewMat();
            }
        }
        protected override void Render()
        {
            base.Render();

            for (int l = 0; l < _current.LifeCount; l++)
            {
                FramePart fp = _current.Frames[Counter, l];

                if (!fp.Alive) { continue; }

                DrawLifeform(fp.Position, fp.Colour);
            }
        }
    }
}
