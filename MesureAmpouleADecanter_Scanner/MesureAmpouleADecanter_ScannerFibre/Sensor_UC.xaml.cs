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

        public SolidColorBrush color
        {
            get => _color;
            set
            {
                if (_color == value) return;
                _color = value;
                OnPropertyChanged();
            }
        }
        SolidColorBrush _color = new SolidColorBrush(Colors.Magenta);

        public string index
        {
            get => _index;
            set
            {
                _index = value;
                OnPropertyChanged();
            }
        }
        string _index;

        public float intensity
        {
            get => _intensity;
            set
            {
                if (_intensity == value) return;
                _intensity = value;
                OnPropertyChanged();
            }
        }
        float _intensity;


        public Sensor s;

        public Sensor_UC()
        {
            InitializeComponent();
            DataContext = this;
        }

        internal void _Link(Sensor s)
        {
            this.s = s;
            s.uc = this;
            _index = s.numero + " : ";
        }

        internal void _Update(Vec3b pixelValue, float intensity)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                color = new SolidColorBrush(Color.FromRgb(
                    pixelValue.Item2,
                    pixelValue.Item1,
                    pixelValue.Item0));
                this.intensity = intensity;
                _tbk_val.Text = (intensity * 100).ToString("00");
            }));
        }
    }
}
