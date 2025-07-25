using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace AntiScan
{
    public class HoverButton : Control
    {
        private Timer _timer;
        private Timer _fadeTimer;
        private float _angle = 0;
        private float _hoverAngle = 15;
        private int _rotationSpeed = 20;
        private bool _isHovered = false;
        private float _textOpacity = 1.0f;
        private float _imageOpacity = 0f;
        private bool _isFadingToImage = false;

        private Image _hoverImage;
        private string _centerText = "Scan";
        private Color _centerColor1 = Color.DarkGray;
        private Color _centerColor2 = Color.FromArgb(30, 25, 56);
        private Color _surroundColor1 = Color.Green;
        private Color _surroundColor2 = Color.Green;

        private Color _bigStrokeStartColor = Color.FromArgb(30, 35, 70);
        private Color _bigStrokeEndColor = Color.FromArgb(30, 35, 70);
        private int _bigStrokeThickness = 25;

        private Color _rotationStrokeStartColor = Color.DarkRed;
        private Color _rotationStrokeEndColor = Color.LimeGreen;
        private int _rotationStrokeRadius = 10;

        private Color _surroundStrokeColor = Color.White;
        private int _surroundStrokeThickness = 10;

        public HoverButton()
        {
            this.DoubleBuffered = true;
            this.Size = new Size(200, 200);
            this.Cursor = Cursors.Hand;
            InitializeTimers();
        }

        // Add this missing property
        [Category("Custom Settings"), Description("Rotating stroke start color")]
        public Color RotationStrokeStartColor
        {
            get { return _rotationStrokeStartColor; }
            set
            {
                _rotationStrokeStartColor = value;
                Invalidate();
            }
        }

        // ... [Keep all your other properties exactly as they were] ...

        private void InitializeTimers()
        {
            _timer = new Timer();
            _timer.Interval = _rotationSpeed;
            _timer.Tick += Timer_Tick;
            _timer.Start();

            _fadeTimer = new Timer();
            _fadeTimer.Interval = 30;
            _fadeTimer.Tick += Fade_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_isHovered)
            {
                _angle = (_angle + 1) % 360;
            }
            this.Invalidate();
        }

        private void Fade_Tick(object sender, EventArgs e)
        {
            float step = 0.05f;

            if (_isFadingToImage)
            {
                _imageOpacity = Math.Min(1, _imageOpacity + step);
                _textOpacity = Math.Max(0, _textOpacity - step);
            }
            else
            {
                _imageOpacity = Math.Max(0, _imageOpacity - step);
                _textOpacity = Math.Min(1, _textOpacity + step);
            }

            if ((_isFadingToImage && _imageOpacity >= 1) ||
                (!_isFadingToImage && _imageOpacity <= 0))
            {
                _fadeTimer.Stop();
            }
            this.Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            _isFadingToImage = true;
            _fadeTimer.Start();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isFadingToImage = false;
            _fadeTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawRadialGradientBackground(g);
            DrawBigOuterStroke(g);
            DrawSurroundStroke(g);
            DrawRotatingSegments(g);

            if (_imageOpacity > 0 && _hoverImage != null)
            {
                DrawCenterImage(g, _imageOpacity);
            }

            if (_textOpacity > 0)
            {
                DrawCenterText(g, _textOpacity);
            }
        }

        private void DrawRadialGradientBackground(Graphics g)
        {
            int circleDiameter = this.Width - _bigStrokeThickness - 20;
            Rectangle rect = new Rectangle(
                10 + (_bigStrokeThickness / 2),
                10 + (_bigStrokeThickness / 2),
                circleDiameter,
                circleDiameter);

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(rect);
                using (PathGradientBrush radialBrush = new PathGradientBrush(path))
                {
                    radialBrush.CenterColor = _centerColor1;
                    radialBrush.SurroundColors = new Color[] { _centerColor2 };
                    g.FillPath(radialBrush, path);
                }
            }
        }

        private void DrawBigOuterStroke(Graphics g)
        {
            int circleDiameter = this.Width - _bigStrokeThickness - 20;
            Rectangle rect = new Rectangle(
                10 + (_bigStrokeThickness / 2),
                10 + (_bigStrokeThickness / 2),
                circleDiameter,
                circleDiameter);

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(rect);
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    rect,
                    _bigStrokeStartColor,
                    _bigStrokeEndColor,
                    LinearGradientMode.ForwardDiagonal))
                using (Pen outerPen = new Pen(brush, _bigStrokeThickness))
                {
                    g.DrawPath(outerPen, path);
                }
            }
        }

        private void DrawSurroundStroke(Graphics g)
        {
            int circleDiameter = this.Width - _bigStrokeThickness - 20;
            Rectangle rect = new Rectangle(
                10 + (_bigStrokeThickness / 2),
                10 + (_bigStrokeThickness / 2),
                circleDiameter,
                circleDiameter);

            using (Pen strokePen = new Pen(_surroundStrokeColor, _surroundStrokeThickness))
            {
                g.DrawEllipse(strokePen, rect);
            }
        }

        private void DrawRotatingSegments(Graphics g)
        {
            int circleDiameter = this.Width - _bigStrokeThickness - 20;
            Rectangle rect = new Rectangle(10, 10, circleDiameter, circleDiameter);

            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(rect);
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    rect,
                    _rotationStrokeStartColor,
                    _rotationStrokeEndColor,
                    LinearGradientMode.ForwardDiagonal))
                using (Pen rotationPen = new Pen(brush, _rotationStrokeRadius))
                {
                    g.TranslateTransform(this.Width / 2, this.Height / 2);
                    g.RotateTransform(_angle);
                    g.TranslateTransform(-this.Width / 2, -this.Height / 2);
                    g.DrawPath(rotationPen, path);
                    g.ResetTransform();
                }
            }
        }

        private void DrawCenterImage(Graphics g, float opacity)
        {
            if (_hoverImage == null) return;

            int centerX = this.Width / 2;
            int centerY = this.Height / 2;
            int imageSize = Math.Min(this.Width, this.Height) / 2;

            Rectangle destRect = new Rectangle(
                centerX - imageSize / 2,
                centerY - imageSize / 2,
                imageSize,
                imageSize);

            ColorMatrix matrix = new ColorMatrix();
            matrix.Matrix33 = opacity;

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            g.DrawImage(
                _hoverImage,
                destRect,
                0, 0, _hoverImage.Width, _hoverImage.Height,
                GraphicsUnit.Pixel,
                attributes);
        }

        private void DrawCenterText(Graphics g, float opacity)
        {
            using (Font font = new Font("Arial", 24, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb((int)(opacity * 255), _surroundStrokeColor)))
            {
                SizeF textSize = g.MeasureString(_centerText, font);
                PointF position = new PointF(
                    (this.Width - textSize.Width) / 2,
                    (this.Height - textSize.Height) / 2);

                g.DrawString(_centerText, font, brush, position);
            }
        }
    }
}