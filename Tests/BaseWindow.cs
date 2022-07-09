using System;
using Zene.Graphics;
using Zene.Graphics.Shaders;
using Zene.Structs;
using Zene.Windowing;

namespace NetworkProgram
{
    public abstract class BaseWindow : Window
    {
        public BaseWindow(int width, int height, string title, Settings settings)
            : base(width, height, title, 4.3, new WindowInitProperties()
            {
                // Anti aliasing
                Samples = 4
            })
        {
            _shader = new BasicShader();
            _shader.SetColourSource(ColourSource.UniformColour);

            _drawOctagon = new DrawObject<Vector2, byte>(new Vector2[]
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

            _drawBox = new DrawObject<Vector2, byte>(new Vector2[]
                {
                    (0.5, 0.5),
                    (-0.5, 0.5),
                    (-0.5, -0.5),
                    (0.5, -0.5),
                }, new byte[]
                {
                    0, 1, 2,
                    2, 3, 0
                }, 1, 0, AttributeSize.D2, BufferUsage.DrawFrequent);

            Settings = settings;
            ReferenceSize = new Vector2I(settings.WorldSize);

            // Set Framebuffer's clear colour to light-grey
            BaseFramebuffer.ClearColour = new Colour(225, 225, 225);

            OnSizePixelChange(new SizeChangeEventArgs(width, height));
        }
        public BaseWindow(int width, int height, string title, Settings settings, Vector2I refSize)
            : this(width, height, title, settings)
        {
            ReferenceSize = refSize;
        }

        protected Settings Settings { get; }

        private readonly BasicShader _shader;
        private readonly DrawObject<Vector2, byte> _drawBox;
        private readonly DrawObject<Vector2, byte> _drawOctagon;
        private DrawObject<Vector2, byte> _lifeGraphics;

        private bool _running = true;
        protected int Counter = 0;

        private Vector2 _drawScale = Vector2.One;
        private double _zoom = 1d;

        private Vector2 _offset = Vector2.Zero;
        private Vector2 _mouseOld;
        private bool _move;

        protected Vector2I ReferenceSize { get; set; }

        protected override void OnUpdate(EventArgs e)
        {
            base.OnUpdate(e);

            if (_running) { UpdateData(); }

            Render();

            if (Settings.Delay > 0 && _running)
            {
                System.Threading.Thread.Sleep(Settings.Delay);
            }

            if (_running) { Counter++; }
        }

        protected override void Dispose(bool dispose)
        {
            base.Dispose(dispose);

            if (dispose)
            {
                _shader.Dispose();
                _drawBox.Dispose();
                _drawOctagon.Dispose();
            }
        }

        protected abstract void UpdateData();
        protected virtual void Render()
        {
            _shader.Bind();

            Vector2 offset = _offset;

            if (_move)
            {
                offset += MouseChange();
            }

            // Shift so (0, 0) is in the bottom-left corner and increase the size of the drawn objects
            _shader.Matrix2 = Matrix4.CreateTranslation((ReferenceSize.X / -2) + 0.5, (ReferenceSize.Y / -2) + 0.5, 0) *
                Matrix4.CreateTranslation((Vector3)offset) * Matrix4.CreateScale(_zoom);

            // Clear screen light-grey
            Framebuffer.Clear(BufferBit.Colour);

            DrawBorder();

            _lifeGraphics = Settings.LowPoly ? _drawBox : _drawOctagon;
        }

        private void DrawBorder()
        {
            // Border too small to draw
            if (Settings.BorderSize <= 0) { return; }

            _shader.SetDrawColour(Settings.BorderColour);

            double border = Settings.BorderSize;

            double halfHeight = (ReferenceSize.Y * 0.5) - 0.5;
            double halfWidth = (ReferenceSize.X * 0.5) - 0.5;
            double borderOffset = (1 - border) * 0.5;
            double twoBorder = border * 2;

            // Left
            _shader.Matrix1 = Matrix4.CreateScale(border, ReferenceSize.Y + twoBorder, 1) *
                Matrix4.CreateTranslation(-1 + borderOffset, halfHeight, 0);
            _drawBox.Draw();

            // Right
            _shader.Matrix1 = Matrix4.CreateScale(border, ReferenceSize.Y + twoBorder, 1) *
                Matrix4.CreateTranslation(ReferenceSize.X - borderOffset, halfHeight, 0);
            _drawBox.Draw();

            // Top
            _shader.Matrix1 = Matrix4.CreateScale(ReferenceSize.X, border, 1) *
                Matrix4.CreateTranslation(halfWidth, ReferenceSize.Y - borderOffset, 0);
            _drawBox.Draw();

            // Bottom
            _shader.Matrix1 = Matrix4.CreateScale(ReferenceSize.X, border, 1) *
                Matrix4.CreateTranslation(halfWidth, -1 + borderOffset, 0);
            _drawBox.Draw();
        }

        protected void DrawLifeform(Vector2I pos, Colour colour)
        {
            _shader.SetDrawColour(colour);
            _shader.Matrix1 = Matrix4.CreateTranslation(pos.X, pos.Y, 0);
            _lifeGraphics.Draw();
        }
        protected void DrawLifeform(Vector2I pos, Colour3 colour)
        {
            _shader.SetDrawColour((Colour)colour);
            _shader.Matrix1 = Matrix4.CreateTranslation(pos.X, pos.Y, 0);
            _drawOctagon.Draw();
        }

        protected override void OnSizePixelChange(SizeChangeEventArgs e)
        {
            base.OnSizePixelChange(e);

            // Set drawing view
            Framebuffer.ViewSize = new Vector2I(e.Width, e.Height);

            CalculateViewMat();
        }

        protected void CalculateViewMat()
        {
            int winWidth = Width;
            int winHeight = Height;

            int w;
            int h;

            if (winHeight > winWidth)
            {
                w = ReferenceSize.X;
                h = (int)((winHeight / (double)winWidth) * ReferenceSize.X);
            }
            else // Width is bigger
            {
                h = ReferenceSize.Y;
                w = (int)((winWidth / (double)winHeight) * ReferenceSize.Y);
            }

            _drawScale = ((double)w / winWidth, (double)h / winHeight);

            _shader.Matrix3 = Matrix4.CreateOrthographic(w, h, -10, 10);
        }

        private Vector2 MouseChange()
        {
            Vector2 value = (MouseLocation - _mouseOld) * _drawScale / _zoom;

            value.Y = -value.Y;

            return value;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _mouseOld = e.Location;
            _move = true;
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _move = false;
            _offset += MouseChange();
        }
        protected override void OnScroll(ScrollEventArgs e)
        {
            base.OnScroll(e);

            if (_move)
            {
                _offset += MouseChange();
                _mouseOld = MouseLocation;
            }

            double newZoom = _zoom + (e.DeltaY * 0.1 * _zoom);

            if (newZoom < 0) { return; }

            double oldZoom = _zoom;
            _zoom = newZoom;

            // Zoom in on mouse

            Vector2 mouse = MouseLocation - ((Vector2)Size * 0.5);
            mouse.Y = -mouse.Y;

            Vector2 mouseRelOld = (mouse * _drawScale / oldZoom) - _offset;
            Vector2 mouseRelNew = (mouse * _drawScale / _zoom) - _offset;

            _offset += mouseRelNew - mouseRelOld;
        }

        private readonly double _arrowSpeed = 5;
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e[Keys.Escape])
            {
                Close();
                return;
            }
            if (e[Keys.Space])
            {
                _running = !_running;
                return;
            }
            if (e[Keys.Left])
            {
                _offset.X += _arrowSpeed;
                return;
            }
            if (e[Keys.Right])
            {
                _offset.X -= _arrowSpeed;
                return;
            }
            if (e[Keys.Up])
            {
                _offset.Y -= _arrowSpeed;
                return;
            }
            if (e[Keys.Down])
            {
                _offset.Y += _arrowSpeed;
                return;
            }
            if (e[Keys.C])
            {
                _offset = Vector2.Zero;
                _zoom = 1d;
                return;
            }
            if (e[Keys.F])
            {
                FullScreen = !FullScreen;
                return;
            }
        }
    }
}
