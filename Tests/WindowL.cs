using Zene.Graphics;
using Zene.Windowing;
using Zene.Windowing.Base;
using Zene.Graphics.Shaders;
using Zene.Structs;
using Zene.NeuralNetworking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using FileEncoding;

namespace NeuralNetworkingTest
{
    public class WindowL : Window
    {
        public WindowL(int width, int height, string title)
            : base(width, height, title, 4.3, new WindowInitProperties()
                {
                    // Anti aliasing
                    Samples = 4
                })
        {
            _shader = new BasicShader();

            _lifeGraphics = new DrawObject<Vector2, byte>(new Vector2[]
                {
                    new Vector2(-0.5, 0.25),
                    new Vector2(-0.25, 0.5),
                    new Vector2(0.25, 0.5),
                    new Vector2(0.5, 0.25),
                    new Vector2(0.5, -0.25),
                    new Vector2(0.25, -0.5),
                    new Vector2(-0.25, -0.5),
                    new Vector2(-0.5, -0.25)
                }, new byte[]
                {
                    0, 1, 2,
                    0, 2, 3,
                    0, 3, 4,
                    0, 4, 7,
                    4, 5, 6,
                    4, 6, 7
                }, 1, 0, AttributeSize.D2, BufferUsage.DrawFrequent);

            _world = new World(
                Program.Settings.LifeForms,
                Program.Settings.BrainSize,
                Program.Settings.WorldSize,
                Program.Settings.WorldSize,
                Program.Settings.GenLength);

            // Set Framebuffer's clear colour to light-grey
            BaseFramebuffer.ClearColour = new Colour(225, 225, 225);

            OnSizePixelChange(new SizeChangeEventArgs(width, height));

            // Setup propper alpha channel support
            //Zene.Graphics.GL4.GL.Enable(Zene.Graphics.GL4.GLEnum.Blend);
            //Zene.Graphics.GL4.GL.BlendFunc(Zene.Graphics.GL4.GLEnum.SrcAlpha, Zene.Graphics.GL4.GLEnum.OneMinusSrcAlpha);
        }
        public WindowL(int width, int height, string title, Lifeform[] lifeforms)
            : base(width, height, title, 4.3, new WindowInitProperties()
            {
                // Anti aliasing
                Samples = 4
            })
        {
            if (lifeforms.Length != Program.Settings.LifeForms)
            {
                throw new Exception();
            }

            _shader = new BasicShader();

            _lifeGraphics = new DrawObject<Vector2, byte>(new Vector2[]
                {
                    new Vector2(-0.5, 0.25),
                    new Vector2(-0.25, 0.5),
                    new Vector2(0.25, 0.5),
                    new Vector2(0.5, 0.25),
                    new Vector2(0.5, -0.25),
                    new Vector2(0.25, -0.5),
                    new Vector2(-0.25, -0.5),
                    new Vector2(-0.5, -0.25)
                },
                new byte[]
                {
                    0, 1, 2,
                    0, 2, 3,
                    0, 3, 4,
                    0, 4, 7,
                    4, 5, 6,
                    4, 6, 7
                }, 1, 0, AttributeSize.D2, BufferUsage.DrawFrequent);

            _world = new World(
                Program.Settings.WorldSize,
                Program.Settings.WorldSize,
                lifeforms,
                Program.Settings.GenLength);

            // Set Framebuffer's clear colour to light-grey
            BaseFramebuffer.ClearColour = new Colour(225, 225, 225);

            OnSizePixelChange(new SizeChangeEventArgs(width, height));

            // Setup propper alpha channel support
            //Zene.Graphics.GL4.GL.Enable(Zene.Graphics.GL4.GLEnum.Blend);
            //Zene.Graphics.GL4.GL.BlendFunc(Zene.Graphics.GL4.GLEnum.SrcAlpha, Zene.Graphics.GL4.GLEnum.OneMinusSrcAlpha);
        }
        public WindowL(int width, int height, string title, Gene[][] genes)
            : base(width, height, title, 4.3, new WindowInitProperties()
            {
                // Anti aliasing
                Samples = 4
            })
        {
            _shader = new BasicShader();

            _lifeGraphics = new DrawObject<Vector2, byte>(new Vector2[]
                {
                    new Vector2(-0.5, 0.25),
                    new Vector2(-0.25, 0.5),
                    new Vector2(0.25, 0.5),
                    new Vector2(0.5, 0.25),
                    new Vector2(0.5, -0.25),
                    new Vector2(0.25, -0.5),
                    new Vector2(-0.25, -0.5),
                    new Vector2(-0.5, -0.25)
                },
                new byte[]
                {
                    0, 1, 2,
                    0, 2, 3,
                    0, 3, 4,
                    0, 4, 7,
                    4, 5, 6,
                    4, 6, 7
                }, 1, 0, AttributeSize.D2, BufferUsage.DrawFrequent);

            _world = new World(
                Program.Settings.WorldSize,
                Program.Settings.WorldSize,
                Lifeform.FromGenes(Lifeform.Random, genes,
                    Program.Settings.LifeForms,
                    Program.Settings.WorldSize,
                    Program.Settings.WorldSize),
                Program.Settings.GenLength);

            // Set Framebuffer's clear colour to light-grey
            BaseFramebuffer.ClearColour = new Colour(225, 225, 225);

            OnSizePixelChange(new SizeChangeEventArgs(width, height));

            // Setup propper alpha channel support
            //Zene.Graphics.GL4.GL.Enable(Zene.Graphics.GL4.GLEnum.Blend);
            //Zene.Graphics.GL4.GL.BlendFunc(Zene.Graphics.GL4.GLEnum.SrcAlpha, Zene.Graphics.GL4.GLEnum.OneMinusSrcAlpha);
        }

        private readonly BasicShader _shader;
        private readonly DrawObject<Vector2, byte> _lifeGraphics;

        public void Run()
        {
            if (Program.Settings.VSync)
            {
                GLFW.SwapInterval(1);
            }
            else
            {
                GLFW.SwapInterval(0);
            }

            FramePart[,] frames = null;

            List<int> exportGens = new List<int>(Program.Settings.ExportGens);
            bool exportGen = exportGens.Contains(_world.Generation);
            if (exportGen)
            {
                frames = new FramePart[Program.Settings.GenLength, _world.Lifeforms.Length];
            }

            int exporting = 0;
            object exportRef = new object();

            int counter = 0;

            while (GLFW.WindowShouldClose(Handle) == 0 && // While window shouldn't close
                // Or have not finished simulating generations
                (_world.Generation < Program.Settings.Gens || Program.Settings.Gens <= 0))
            {
                if (counter >= Program.Settings.GenLength)
                {
                    // Export generation
                    if (exportGen)
                    {
                        int generation = _world.Generation;

                        lock (exportRef) { exporting++; }

                        Task.Run(() =>
                        {
                            Gen.ExportFrames(
                                new FileStream($"{Program.Settings.ExportPath}/{Program.Settings.ExportName}{generation}.gen", FileMode.Create),
                                frames,
                                Program.Settings.WorldSize,
                                generation,
                                Program.Settings.BrainSize,
                                Program.Settings.InnerCells,
                                Lifeform.ColourGrade);

                            Network.ExportLifeforms($"{Program.Settings.ExportPath}/{Program.Settings.ExportName}-lf{generation}.txt", _world.Lifeforms);

                            lock (exportRef) { exporting--; }
                        });
                    }

                    counter = 0;
                    _world = _world.NextGeneration(Program.Settings.LifeForms, Program.CheckLifeform);

                    exportGen = exportGens.Contains(_world.Generation);

                    if (exportGen)
                    {
                        frames = new FramePart[Program.Settings.GenLength, _world.Lifeforms.Length];
                    }
                }

                Update();

                if (exportGen)
                {
                    for (int i = 0; i < _world.Lifeforms.Length; i++)
                    {
                        frames[counter, i] = new FramePart(_world.Lifeforms[i]);
                    }
                }

                // Manage window input and output
                GLFW.SwapBuffers(Handle);
                GLFW.PollEvents();

                if (Program.Settings.Delay != 0)
                {
                    System.Threading.Thread.Sleep(Program.Settings.Delay);
                }

                counter++;
            }

            Dispose();
        }

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);

            if (dispose)
            {
                _shader.Dispose();
                _lifeGraphics.Dispose();
            }
        }

        private World _world;

        private void Update()
        {
            _shader.Bind();
            _shader.SetColourSource(ColourSource.UniformColour);

            // Shift so (0, 0) is in the bottom-left corner and increase the size of the drawn objects
            _shader.Matrix2 = Matrix4.CreateTranslation((Program.Settings.WorldSize / -2) + 0.5, (Program.Settings.WorldSize / -2) + 0.5, 0);

            // Clear screen light-grey
            Framebuffer.Clear(BufferBit.Colour);

            _world.Update();
            // Update and draw all lifeforms
            //_world.UpdateDraw(DrawLifeform);

            foreach (Lifeform l in _world.Lifeforms)
            {
                DrawLifeform(l);
            }
        }

        private void DrawLifeform(Lifeform lifeform)
        {
            _shader.SetDrawColour(lifeform.Colour);
            _shader.Matrix1 = Matrix4.CreateTranslation(lifeform.Location.X, lifeform.Location.Y, 0);
            _lifeGraphics.Draw();
        }

        protected override void OnSizePixelChange(SizeChangeEventArgs e)
        {
            base.OnSizePixelChange(e);

            // Set drawing view
            Framebuffer.ViewSize = new Vector2I(e.Width, e.Height);

            int w;
            int h;

            if (e.Height > e.Width)
            {
                w = Program.Settings.WorldSize;
                h = (int)((e.Height / (double)e.Width) * Program.Settings.WorldSize);
            }
            else // Width is bigger
            {
                h = Program.Settings.WorldSize;
                w = (int)((e.Width / (double)e.Height) * Program.Settings.WorldSize);
            }

            _shader.Matrix3 = Matrix4.CreateOrthographic(w, h, -10, 10);
        }
    }
}
