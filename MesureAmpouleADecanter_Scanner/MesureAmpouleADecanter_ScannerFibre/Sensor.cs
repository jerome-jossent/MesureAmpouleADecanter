using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace MesureAmpouleADecanter_ScannerFibre
{
    public class Sensor : INotifyPropertyChanged
    {
        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public int numero { get; set; }
        public int x { get => (int)cercle.x; }
        public int y { get => (int)cercle.y; }

        public float intensity { get; set; }

        public Scalar color_scalar { get; set; }

        public Cercle cercle;
        internal Sensor_UC uc;

        public Sensor(Cercle c)
        {
            cercle = c;
            c.sensor = this;
            numero = c.numero;
        }

        public void SetColor(Vec3b pixelValue)
        {
            color_scalar = new Scalar(pixelValue.Item0, pixelValue.Item1, pixelValue.Item2);
            intensity = (float)(((float)pixelValue.Item0 + pixelValue.Item1 + pixelValue.Item2) / (255 * 3));
            uc?._Update(pixelValue, intensity);
        }

    }
}
