using DirectShowLib;
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

namespace WebCamParameters_UC
{
    /// <summary>
    /// Logique d'interaction pour _Parameter.xaml
    /// </summary>
    public partial class _Parameter : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        VideoProcAmpProperty videoProcAmpProperty;
        CameraControlProperty cameraControlProperty;

        public string _name { get; set; }
        public int _value
        {
            get => val; set
            {
                val = value;
                OnPropertyChanged();
                if (cameraControl_SetValue != null)
                    cameraControl_SetValue(cameraControlProperty, val);
                if (videoProcAmp_SetValue != null)
                    videoProcAmp_SetValue(videoProcAmpProperty, val);
            }
        }
        int val;

        Action<CameraControlProperty, int> cameraControl_SetValue;
        Action<VideoProcAmpProperty, int> videoProcAmp_SetValue;

        public int _maximum
        {
            get => maximum; set
            {
                maximum = value;
                OnPropertyChanged();
            }
        }
        int maximum;

        public int _minimum
        {
            get => minimum; set
            {
                minimum = value;
                OnPropertyChanged();
            }
        }
        int minimum;

        public int _step
        {
            get => step; set
            {
                step = value;
                OnPropertyChanged();
            }
        }
        int step;

        public _Parameter()
        {
            InitializeComponent();
            DataContext = this;
        }

        internal void _Link(KeyValuePair<CameraControlProperty, WebCamParameters_CameraControl> item, Action<CameraControlProperty, int> cameraControl_SetValue)
        {
            cameraControlProperty = item.Key;
            _name = cameraControlProperty.ToString();
            _ckb_auto.Visibility = item.Value.auto_enabled ? Visibility.Visible : Visibility.Collapsed;
            _minimum = int.MinValue; 
            _maximum = int.MaxValue; 
            _minimum = item.Value.minValue;
            _maximum = item.Value.maxValue;
            _step = item.Value.stepSize;
            _value = item.Value.currentValue;
            this.cameraControl_SetValue = cameraControl_SetValue;
        }

        internal void _Link(KeyValuePair<VideoProcAmpProperty, WebCamParameters_VideoProcAmp> item, Action<VideoProcAmpProperty, int> videoProcAmp_SetValue)
        {
            videoProcAmpProperty = item.Key;
            _name = videoProcAmpProperty.ToString();
            _ckb_auto.Visibility = item.Value.auto_enabled ? Visibility.Visible : Visibility.Collapsed;
            _minimum = int.MinValue;
            _maximum = int.MaxValue;
            _minimum = item.Value.minValue;
            _maximum = item.Value.maxValue;
            _step = item.Value.stepSize;
            _value = item.Value.currentValue;
            this.videoProcAmp_SetValue = videoProcAmp_SetValue;
        }
    }
}
