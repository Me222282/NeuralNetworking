using Zene.Graphics;
using Zene.Windowing;
using Zene.Windowing.Base;
using Zene.Graphics.Shaders;
using Zene.Structs;
using System;
using System.IO;
using FileEncoding;

namespace NeuralNetworkingTest
{
    public class WindowW : Window
    {
        public WindowW(int width, int height, string[] titles, FramePart[][,] frames, int[] counts, int[] lives, int[] size, int[] gens)
            : base(width, height, titles[0], 4.3, new WindowInitProperties()
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

            _frames = frames;
            _frameCount = counts;
            _lifeCount = lives;
            _worldSize = size;
            _generation = gens;
            _titles = titles;

            // Set Framebuffer's clear colour to light-grey
            BaseFramebuffer.ClearColour = new Colour(225, 225, 225);

            OnSizePixelChange(new SizeChangeEventArgs(width, height));
        }

        private readonly BasicShader _shader;
        private readonly DrawObject<Vector2, byte> _lifeGraphics;

        private int _vidCounter;

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

            int frameCounter = 0;
            _vidCounter = 0;

            Console.WriteLine($"Generation {_generation[_vidCounter]}");

            while (GLFW.WindowShouldClose(Handle) == 0) // While window shouldn't close
            {
                if (frameCounter >= _frameCount[_vidCounter])
                {
                    frameCounter = 0;
                    _vidCounter++;

                    if (_vidCounter >= _frames.Length)
                    {
                        _vidCounter = 0;
                    }

                    Console.WriteLine($"Generation {_generation[_vidCounter]}");

                    Title = _titles[_vidCounter];
                }

                DrawFrame(frameCounter);

                // Manage window input and output
                GLFW.SwapBuffers(Handle);
                GLFW.PollEvents();

                if (Program.Settings.Delay != 0)
                {
                    System.Threading.Thread.Sleep(Program.Settings.Delay);
                }

                frameCounter++;
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

        private readonly int[] _worldSize;
        private readonly int[] _frameCount;
        private readonly int[] _lifeCount;
        private readonly int[] _generation;
        private readonly FramePart[][,] _frames;
        private readonly string[] _titles;

        private void DrawFrame(int frame)
        {
            _shader.Bind();
            _shader.SetColourSource(ColourSource.UniformColour);

            // Shift so (0, 0) is in the bottom-left corner and increase the size of the drawn objects
            _shader.Matrix2 = Matrix4.CreateTranslation((_worldSize[_vidCounter] / -2) + 0.5, (_worldSize[_vidCounter] / -2) + 0.5, 0);

            // Clear screen light-grey
            Framebuffer.Clear(BufferBit.Colour);

            for (int l = 0; l < _lifeCount[_vidCounter]; l++)
            {
                DrawLifeform(_frames[_vidCounter][frame, l]);
            }
        }

        private void DrawLifeform(FramePart lifeform)
        {
            _shader.SetDrawColour((Colour)lifeform.Colour);
            _shader.Matrix1 = Matrix4.CreateTranslation(lifeform.Position.X, lifeform.Position.Y, 0);
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
                w = _worldSize[_vidCounter];
                h = (int)((e.Height / (double)e.Width) * _worldSize[_vidCounter]);
            }
            else // Width is bigger
            {
                h = _worldSize[_vidCounter];
                w = (int)((e.Width / (double)e.Height) * _worldSize[_vidCounter]);
            }

            _shader.Matrix3 = Matrix4.CreateOrthographic(w, h, -10, 10);
        }
    }
}
