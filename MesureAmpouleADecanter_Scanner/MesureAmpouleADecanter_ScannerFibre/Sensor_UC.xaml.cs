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

        public string _color_string
        {
            get =>                 
                _color.Color.R + ", " + _color.Color.G + ", " + _color.Color.B
               
                // _color.Color.R + ", " + _color.Color.G + ", " + _color.Color.B + "[" +
               // _s.normalisation_a[0].ToString("0.00") + ";" + _s.normalisation_a[1].ToString("0.00") + ";" + _s.normalisation_a[2].ToString("0.00") + ";" + "|" +
               // _s.normalisation_b[0].ToString("0.00") + ";" + _s.normalisation_b[1].ToString("0.00") + ";" + _s.normalisation_b[2].ToString("0.00") + ";" +
               //"]"                
                ;
        }

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


        public float _intensity_min { get => _s.intensity_min; }
        public float _intensity_max { get => _s.intensity_max; }
        public string _ON { get => _s.ON?"ON":"off"; }

        public Sensor _s;

        public int _x { get { return _s.x; } set { _s.x = value; } }
        public int _y { get { return _s.y; } set { _s.y = value; } }

        public bool Selected;

        public string _hauteur_mm { get { return " (" + _s.hauteur_mm.ToString("f1")+"mm)"; } }

        public Visibility _isVisible { get => Selected?Visibility.Visible:Visibility.Collapsed; }


        public Sensor_UC()
        {
            InitializeComponent();
            DataContext = this;
        }

        internal void _Link(Sensor s)
        {
            this._s = s;
            s.uc = this;
            if (s.numero != null)
                _SetIndexName();
        }

        internal void _SetIndexName()
        {
            _index = (_s.numero == null) ? "?" : ((int)_s.numero).ToString();
        }




        internal void _Update(Vec3b pixelValue)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _color = new SolidColorBrush(Color.FromRgb(
                    pixelValue.Item2,
                    pixelValue.Item1,
                    pixelValue.Item0));

                OnPropertyChanged(nameof(_intensity_min));
                OnPropertyChanged(nameof(_intensity_max));
                _intensity = _s.intensity;
                OnPropertyChanged(nameof(_ON));

                _tbk_val.Text = (_s.intensity * 100).ToString("00");
            }));
        }

        internal void _UpdateNormalized(Vec3b pixelValueNormalized)
        {
            return;
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
            OnPropertyChanged(nameof(_isVisible));
        }

    }
}
