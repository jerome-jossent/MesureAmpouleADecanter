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
        public int nbr;
        public int nbr_fois_centre_repere = 1;

        internal static Scalar couleur = new Scalar(0, 0, 255);
        public CircleSegment circleSegment;
        internal static Scalar couleurTexte = new Scalar(0, 128, 0);

        public Cercle(CircleSegment circleSegment, int index)
        {
            this.circleSegment = circleSegment;
            numero = index;
        }


    }

}
