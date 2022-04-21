using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Standard_UC_JJO
{
    public partial class ColorPickerJJO : UserControl
    {
        System.Drawing.Bitmap bitmap;
        bool colorpickerON = false;

        public int _Target_Length { get; set; }

        public Color _Color
        {
            get { return color; }
            set
            {
                if (color != value)
                {
                    color = value;
                    _SetColor(color);
                }
            }
        }
        Color color;

        public class NewColorEventArgs : EventArgs
        {
            public Color color { get; private set; }
            public NewColorEventArgs(Color color)
            {
                this.color = color;
            }
        }
        public event EventHandler<NewColorEventArgs> _ColorNew;

        SolidColorBrush white = new SolidColorBrush(Colors.White);
        SolidColorBrush black = new SolidColorBrush(Colors.Black);

        /// <summary>
        /// ex : 
        /// _colorPickerJJO._ColorNew += ColorNew;
        /// _colorPickerJJO._SetMouseSelection(0.5, 0.5);
        /// _colorPickerJJO._SetColor(Colors.Magenta);
        /// </summary>
        public ColorPickerJJO()
        {
            InitializeComponent();
            ComputeColors();
        }

        private void ComputeColors()
        {
            //HSV, hue range is [0, 359], saturation range is [0, 1], and value range is [0, 1]
            int hue = 360;
            int sat = 256; // intensité de couleur 0=blanc, 1=couleur max
            int val = 256; // intensité lumineuse  0=noir, 1=blanc
            int nbrlignesBW = 20;
            bitmap = new System.Drawing.Bitmap(hue, sat + val + nbrlignesBW + nbrlignesBW);

            for (int x = 0; x < hue; x++)
            {
                //5 lignes en haut
                for (int y = 0; y < nbrlignesBW; y++)
                {
                    bitmap.SetPixel(x, y, System.Drawing.Color.White);
                }

                //saturation
                for (int y = 0; y < sat; y++)
                {
                    HSV hsv = new HSV(h: x,
                                      s: (double)y / sat,
                                      v: 1
                                      );

                    RGB rgb = HSVToRGB(hsv);

                    bitmap.SetPixel(x, y + nbrlignesBW, System.Drawing.Color.FromArgb(rgb.R, rgb.G, rgb.B));
                }

                //value
                for (int y = 0; y < val; y++)
                {
                    HSV hsv = new HSV(h: x,
                                      s: 1,
                                      v: (double)(val - y) / val
                                      );

                    RGB rgb = HSVToRGB(hsv);

                    //offset spatial ici !!
                    bitmap.SetPixel(x, y + nbrlignesBW + 256, System.Drawing.Color.FromArgb(rgb.R, rgb.G, rgb.B));
                }

                //5 lignes en bas
                for (int y = 0; y < nbrlignesBW; y++)
                {
                    bitmap.SetPixel(x, y + nbrlignesBW + 256 + 256, System.Drawing.Color.Black);
                }

            }
            _Colors.Source = Convert(bitmap);
        }

        public BitmapImage Convert(System.Drawing.Bitmap src)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        #region HSV & RGB
        public struct RGB
        {
            byte _r;
            byte _g;
            byte _b;
            public byte R
            {
                get { return _r; }
                set { _r = value; }
            }
            public byte G
            {
                get { return _g; }
                set { _g = value; }
            }
            public byte B
            {
                get { return _b; }
                set { _b = value; }
            }

            public RGB(byte r, byte g, byte b)
            {
                _r = r;
                _g = g;
                _b = b;
            }

            public bool Equals(RGB rgb)
            {
                return (R == rgb.R) && (G == rgb.G) && (B == rgb.B);
            }
        }

        public struct HSV
        {
            double _h;
            double _s;
            double _v;
            public double H
            {
                get { return _h; }
                set { _h = value; }
            }
            public double S
            {
                get { return _s; }
                set { _s = value; }
            }
            public double V
            {
                get { return _v; }
                set { _v = value; }
            }

            public HSV(double h, double s, double v)
            {
                _h = h;
                _s = s;
                _v = v;
            }

            public bool Equals(HSV hsv)
            {
                return (H == hsv.H) && (S == hsv.S) && (V == hsv.V);
            }
        }

        public static RGB HSVToRGB(HSV hsv)
        {
            double r = 0, g = 0, b = 0;

            if (hsv.S == 0)
            {
                r = hsv.V;
                g = hsv.V;
                b = hsv.V;
            }
            else
            {
                int i;
                double f, p, q, t;

                if (hsv.H == 360)
                    hsv.H = 0;
                else
                    hsv.H = hsv.H / 60;

                i = (int)Math.Truncate(hsv.H);
                f = hsv.H - i;

                p = hsv.V * (1.0 - hsv.S);
                q = hsv.V * (1.0 - (hsv.S * f));
                t = hsv.V * (1.0 - (hsv.S * (1.0 - f)));

                switch (i)
                {
                    case 0:
                        r = hsv.V;
                        g = t;
                        b = p;
                        break;

                    case 1:
                        r = q;
                        g = hsv.V;
                        b = p;
                        break;

                    case 2:
                        r = p;
                        g = hsv.V;
                        b = t;
                        break;

                    case 3:
                        r = p;
                        g = q;
                        b = hsv.V;
                        break;

                    case 4:
                        r = t;
                        g = p;
                        b = hsv.V;
                        break;

                    default:
                        r = hsv.V;
                        g = p;
                        b = q;
                        break;
                }
            }
            return new RGB((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }
        #endregion

        void Colors_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            colorpickerON = true;
        }

        void Colors_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            colorpickerON = false;
        }

        void Colors_MouseMove(object sender, MouseEventArgs e)
        {
            if (!colorpickerON) return;

            Point ui_pos = e.GetPosition(_Colors);

            double prop_x = ui_pos.X / _ColorsGrid.ActualWidth;
            double prop_y = ui_pos.Y / _ColorsGrid.ActualHeight;

            _SetMouseSelection(prop_x, prop_y);
        }

        public void _SetMouseSelection(double prop_x, double prop_y)
        {
            //convert to colors Mat dimensions
            int x = (int)(prop_x * bitmap.Width);
            int y = (int)(prop_y * bitmap.Height);
            // read pixel 
            System.Drawing.Color px = bitmap.GetPixel(x, y);
            // make color
            color = Color.FromArgb(255, px.R, px.G, px.B);
            //raise event new color
            _ColorNew?.Invoke(null, new NewColorEventArgs(color));
            if (_Target_Length > 0)
                _SetTarget(prop_x, prop_y);
        }

        void _SetTarget(double prop_x, double prop_y)
        {
            double x = prop_x * _ColorsGrid.ActualWidth;
            double y = prop_y * _ColorsGrid.ActualHeight;

            //positions
            _lineNO.Points = new PointCollection(new List<Point> { new Point(x - _Target_Length, y - 1), new Point(x - 1, y - 1), new Point(x - 1, y - _Target_Length) });
            _lineNE.Points = new PointCollection(new List<Point> { new Point(x + _Target_Length, y - 1), new Point(x + 1, y - 1), new Point(x + 1, y - _Target_Length) });
            _lineSE.Points = new PointCollection(new List<Point> { new Point(x - _Target_Length, y + 1), new Point(x - 1, y + 1), new Point(x - 1, y + _Target_Length) });
            _lineSO.Points = new PointCollection(new List<Point> { new Point(x + _Target_Length, y + 1), new Point(x + 1, y + 1), new Point(x + 1, y + _Target_Length) });

            //color
            bool isDark = color.R < 120 && color.G < 120 && color.B < 120;
            if (isDark)
                _lineNO.Stroke = white;
            else
                _lineNO.Stroke = black;

            _lineSO.Stroke = _lineSE.Stroke = _lineNE.Stroke = _lineNO.Stroke;
        }

        public void _SetColor(Color color)
        {
            Point point = FindColor(color);
            _SetMouseSelection(point.X / bitmap.Width, point.Y / bitmap.Height);
        }

        Point FindColor(Color color)
        {
            Point point = new Point();
            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                {
                    // read pixel 
                    System.Drawing.Color px = bitmap.GetPixel(x, y);
                    if (px.B == color.B &&
                        px.G == color.G &&
                        px.R == color.R)
                        return new Point(x, y);
                }
            return point;
        }
    }
}