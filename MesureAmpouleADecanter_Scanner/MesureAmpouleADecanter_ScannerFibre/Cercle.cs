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
        public int numero;
        public int x { get => (int)circleSegment.Center.X; }
        public int y { get => (int)circleSegment.Center.Y; }
        public int nbr;
        public int nbr_fois_centre_repere = 1;

        public bool actif = false;

        public Scalar couleur;
        public CircleSegment circleSegment;
        internal static Scalar couleurTexte = new Scalar(0, 0, 0);

        public Sensor sensor;

        public Cercle(CircleSegment circleSegment, int index)
        {
            this.circleSegment = circleSegment;
            SetNumero(index);
        }

        public void SetNumero(int index)
        {
            numero = index;
            couleur = ColorFromHSV(250 - 2.5 * numero, 1, 1);
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
