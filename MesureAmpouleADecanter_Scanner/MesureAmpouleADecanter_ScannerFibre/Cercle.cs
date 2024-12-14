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
        public int x;
        public int y;

        internal static Scalar couleur = new Scalar(0, 0, 255);
        public CircleSegment circleSegment;
        internal static Scalar couleurTexte = new Scalar(0, 128, 0);

        public Cercle(CircleSegment circleSegment, int index)
        {
            this.circleSegment = circleSegment;
            numero = index;
        }

        internal static bool IsDejaPresent(Point2f center, ref Mat mat)
        {
            int x = (int)center.X;
            int y = (int)center.Y;
            //read pixel
            Vec3b pixelValue = mat.At<Vec3b>(y, x);

            bool test = pixelValue.Item0 == Cercle.couleur.Val0 &&
                   pixelValue.Item1 == Cercle.couleur.Val1 &&
                   pixelValue.Item2 == Cercle.couleur.Val2;

            return test;
        }
    }

}
