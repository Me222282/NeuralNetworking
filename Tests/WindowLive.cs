using Zene.NeuralNetworking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileEncoding;

namespace NetworkProgram
{
    public class WindowLive : BaseWindow
    {
        public WindowLive(int width, int height, string title, Settings settings)
            : base(width, height, title, settings)
        {
            _world = new World(
                Settings.LifeForms,
                Settings.BrainSize,
                Settings.WorldSize,
                Settings.WorldSize,
                Settings.GenLength);

            ExportGenSetup();
        }
        public WindowLive(int width, int height, string title, Settings settings, Lifeform[] lifeforms, int genStart = 0)
            : base(width, height, title, settings)
        {
            if (lifeforms.Length != Settings.LifeForms)
            {
                throw new Exception();
            }

            _world = new World(
                Settings.WorldSize,
                Settings.WorldSize,
                lifeforms,
                Settings.GenLength,
                genStart);

            ExportGenSetup();
        }
        public WindowLive(int width, int height, string title, Settings settings, Gene[][] genes, int genStart = 0)
            : base(width, height, title, settings)
        {
            _world = new World(
                Settings.WorldSize,
                Settings.WorldSize,
                Lifeform.FromGenes(Lifeform.Random, genes,
                    Settings.LifeForms,
                    Settings.WorldSize,
                    Settings.WorldSize),
                Settings.GenLength,
                genStart);

            ExportGenSetup();
        }
        public WindowLive(int width, int height, string title, Settings settings, NetFile netFile)
            : base(width, height, title, settings)
        {
            _world = new World(
                Settings.WorldSize,
                Settings.WorldSize,
                Lifeform.FromGenes(Lifeform.Random, netFile.Genes,
                    Settings.WorldSize,
                    Settings.WorldSize),
                Settings.GenLength,
                netFile.Generation);

            ExportGenSetup();
        }

        private int _exporting = 0;
        private readonly object _exportRef = new object();

        private World _world;
        private bool _exportGen;
        private List<int> _exportGens;
        private FramePart[,] _frames = null;

        private void ExportGenSetup()
        {
            _exportGens = new List<int>(Settings.ExportGens);
            _exportGen = _exportGens.Contains(0);

            if (_exportGen)
            {
                _frames = new FramePart[Settings.GenLength, _world.Lifeforms.Length];
            }
        }

        protected override void UpdateData()
        {
            if (_world.Generation >= Settings.Gens && Settings.Gens > 0)
            {
                Close();
                return;
            }

            // End of generation
            if (Counter >= Settings.GenLength)
            {
                // Export generation
                if (_exportGen)
                {
                    int generation = _world.Generation;

                    lock (_exportRef) { _exporting++; }

                    Task.Run(() =>
                    {
                        Program.Export(generation, _frames, _world.Lifeforms, Settings);

                        lock (_exportRef) { _exporting--; }
                    });
                }

                Counter = 0;
                _world = _world.NextGeneration(Settings.LifeForms, Settings.CheckLifeform);

                _exportGen = _exportGens.Contains(_world.Generation);

                if (_exportGen)
                {
                    _frames = new FramePart[Settings.GenLength, _world.Lifeforms.Length];
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
