using Newtonsoft.Json;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MesureAmpouleADecanter_ScannerFibre
{
    public class Cercle
    {
        [JsonIgnore]
        public int numero { get; set; }
        public Point2f center_abs { get; set; }
        public Point2f center { get => center_abs - roi_TopLeft; }
        [JsonIgnore]
        public Point roi_TopLeft { get; set; }
        public int roi_Left { get; set; }
        public int roi_Top { get; set; }

        [JsonIgnore]
        public Scalar couleur { get; set; }
        [JsonIgnore]
        public CircleSegment circleSegment { get; set; }
        [JsonIgnore]
        public static Scalar couleurTexte = new Scalar(0, 0, 0);

        [JsonIgnore]
        public int x { get => (int)circleSegment.Center.X; }
        [JsonIgnore]
        public int y { get => (int)circleSegment.Center.Y; }
        [JsonIgnore]
        public int x_abs { get => (int)circleSegment.Center.X + roi_Left; }
        [JsonIgnore]
        public int y_abs { get => (int)circleSegment.Center.Y + roi_Top; }


        [JsonIgnore]
        public int nbr_fois_centre_repere = 1;
        [JsonIgnore]
        public bool actif = false;

        [JsonIgnore]
        public Sensor sensor;

        public Cercle(CircleSegment circleSegment, int index, Rect roi)
        {
            this.circleSegment = circleSegment;
            center_abs = circleSegment.Center;
            SetNumero(index);
            roi_Top = roi.Top;
            roi_Left = roi.Left;
            roi_TopLeft = new Point(roi_Left, roi_Top);
        }

        public void SetNumero(int index)
        {
            numero = index;
            couleur = ColorFromHSV(255 - (255 / 160) * numero, 1, 1);
        }

        public static Scalar ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = (int)(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = (int)(value);
            int p = (int)(value * (1 - saturation));
            int q = (int)(value * (1 - f * saturation));
            int t = (int)(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Scalar.FromRgb(v, t, p);
            else if (hi == 1)
                return Scalar.FromRgb(q, v, p);
            else if (hi == 2)
                return Scalar.FromRgb(p, v, t);
            else if (hi == 3)
                return Scalar.FromRgb(p, q, v);
            else if (hi == 4)
                return Scalar.FromRgb(t, p, v);
            else
                return Scalar.FromRgb(v, p, q);
        }
    }

}
