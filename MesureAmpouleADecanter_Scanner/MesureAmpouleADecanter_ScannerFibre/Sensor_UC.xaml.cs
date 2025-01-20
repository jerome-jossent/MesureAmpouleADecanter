using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MesureAmpouleADecanter_ScannerFibre
{
    public partial class Sensor_UC : UserControl, INotifyPropertyChanged
    {
        void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public SolidColorBrush _color
        {
            get => color;
            set
            {
                if (color == value) return;
                color = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(_color_string));
            }
        }
        SolidColorBrush color = new SolidColorBrush(Colors.Magenta);

        public string _color_string { get => _color.Color.R + ", " + _color.Color.G + ", " + _color.Color.B +"[" +


                s.normalisation_a[0].ToString("0.00") + ";" + s.normalisation_a[1].ToString("0.00") + ";" + s.normalisation_a[2].ToString("0.00") + ";" + "|" + 
                s.normalisation_b[0].ToString("0.00") + ";" + s.normalisation_b[1].ToString("0.00") + ";" + s.normalisation_b[2].ToString("0.00") + ";"  +                 
               "]"; }

        public string _index
        {
            get => index;
            set
            {
                index = value;
                OnPropertyChanged();
            }
        }
        string index;

        public float _intensity
        {
            get => intensity;
            set
            {
                if (intensity == value) return;
                intensity = value;
                OnPropertyChanged();
            }
        }
        float intensity;


        public float _intensity_min { get => s.intensity_min; }
        public float _intensity_max { get => s.intensity_max; }
        public bool _ON { get => s.ON; }

        public Sensor s;

        public int _x { get { return s.x; } set { s.x = value; } }
        public int _y { get { return s.y; } set { s.y = value; } }

        public bool Selected;

        public Sensor_UC()
        {
            InitializeComponent();
            DataContext = this;
        }

        internal void _Link(Sensor s)
        {
            this.s = s;
            s.uc = this;
            _SetIndexName((int)s.numero);
        }

        internal void _SetIndexName(int numero)
        {
            _index = numero.ToString("00") + " : ";
        }

        internal void _Update(Vec3b pixelValue)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //_color = new SolidColorBrush(Color.FromRgb(
                //    pixelValue.Item2,
                //    pixelValue.Item1,
                //    pixelValue.Item0));

                OnPropertyChanged(nameof(_intensity_min));
                OnPropertyChanged(nameof(_intensity_max));
                _intensity = s.intensity;
                OnPropertyChanged(nameof(_ON));

                _tbk_val.Text = (s.intensity * 100).ToString("00");
            }));
        }

        internal void _UpdateNormalized(Vec3b pixelValueNormalized)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _color = new SolidColorBrush(Color.FromRgb(
                    pixelValueNormalized.Item2,
                    pixelValueNormalized.Item1,
                    pixelValueNormalized.Item0));
            }));
        }

        internal void _Selected()
        {
            Selected = !Selected;
            Background = new SolidColorBrush(Selected ? Colors.DarkGray : Colors.Transparent);
        }

    }
}
