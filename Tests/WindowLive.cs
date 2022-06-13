using Zene.NeuralNetworking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using FileEncoding;

namespace NetworkProgram
{
    public class WindowLive : BaseWindow
    {
        public WindowLive(int width, int height, string title)
            : base(width, height, title)
        {
            _exportGens = new List<int>(Program.Settings.ExportGens);

            _world = new World(
                Program.Settings.LifeForms,
                Program.Settings.BrainSize,
                Program.Settings.WorldSize,
                Program.Settings.WorldSize,
                Program.Settings.GenLength);
        }
        public WindowLive(int width, int height, string title, Lifeform[] lifeforms)
            : base(width, height, title)
        {
            if (lifeforms.Length != Program.Settings.LifeForms)
            {
                throw new Exception();
            }

            _exportGens = new List<int>(Program.Settings.ExportGens);

            _world = new World(
                Program.Settings.WorldSize,
                Program.Settings.WorldSize,
                lifeforms,
                Program.Settings.GenLength);
        }
        public WindowLive(int width, int height, string title, Gene[][] genes)
            : base(width, height, title)
        {
            _exportGens = new List<int>(Program.Settings.ExportGens);

            _world = new World(
                Program.Settings.WorldSize,
                Program.Settings.WorldSize,
                Lifeform.FromGenes(Lifeform.Random, genes,
                    Program.Settings.LifeForms,
                    Program.Settings.WorldSize,
                    Program.Settings.WorldSize),
                Program.Settings.GenLength);
        }

        private int _exporting = 0;
        private readonly object _exportRef = new object();

        private World _world;
        private bool _exportGen;
        private readonly List<int> _exportGens;
        private FramePart[,] _frames = null;

        protected override void Update()
        {
            // End of generation
            if (Counter >= Program.Settings.GenLength)
            {
                // Export generation
                if (_exportGen)
                {
                    int generation = _world.Generation;

                    lock (_exportRef) { _exporting++; }

                    Task.Run(() =>
                    {
                        FileStream stream = new FileStream($"{Program.Settings.ExportPath}/{Program.Settings.ExportName}{generation}.gen", FileMode.Create);

                        Gen.ExportFrames(
                            stream,
                            _frames,
                            Program.Settings.WorldSize,
                            generation,
                            Program.Settings.BrainSize,
                            Program.Settings.InnerCells,
                            Lifeform.ColourGrade);

                        stream.Close();

                        Network.ExportLifeforms($"{Program.Settings.ExportPath}/{Program.Settings.ExportName}-lf{generation}.txt", _world.Lifeforms);

                        lock (_exportRef) { _exporting--; }
                    });
                }

                Counter = 0;
                _world = _world.NextGeneration(Program.Settings.LifeForms, Program.CheckLifeform);

                _exportGen = _exportGens.Contains(_world.Generation);

                if (_exportGen)
                {
                    _frames = new FramePart[Program.Settings.GenLength, _world.Lifeforms.Length];
                }
            }

            _world.Update();

            if (_exportGen)
            {
                for (int i = 0; i < _world.Lifeforms.Length; i++)
                {
                    _frames[Counter, i] = new FramePart(_world.Lifeforms[i]);
                }
            }
        }
        protected override void Render()
        {
            base.Render();

            foreach (Lifeform l in _world.Lifeforms)
            {
                if (!l.Alive) { continue; }

                DrawLifeform(l.Location, l.Colour);
            }
        }

        protected override void OnClosing(EventArgs e)
        {
            base.OnClosing(e);

            // while (_exporting > 0)
            Loop:
            lock (_exportRef)
            {
                if (_exporting <= 0) { return; }
            }

            goto Loop;
        }
    }
}
