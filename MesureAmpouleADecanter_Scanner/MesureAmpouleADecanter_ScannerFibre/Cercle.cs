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
        }

    }

}
