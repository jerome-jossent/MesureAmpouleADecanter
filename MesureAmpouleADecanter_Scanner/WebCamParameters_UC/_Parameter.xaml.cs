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
    public partial class _Parameter : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        VideoProcAmpProperty videoProcAmpProperty;
        CameraControlProperty cameraControlProperty;
        bool setbycode;

        Action<CameraControlProperty, int, CameraControlFlags, bool> cameraControl_SetValue;
        Action<VideoProcAmpProperty, int, VideoProcAmpFlags, bool> videoProcAmp_SetValue;

        public string _name { get; set; }
        public int _value
        {
            get => val; set
            {
                val = value;
                OnPropertyChanged();
                if (cameraControl_SetValue != null)
                    cameraControl_SetValue(cameraControlProperty, val, _ckb_auto.IsChecked == true ? CameraControlFlags.Auto : CameraControlFlags.Manual, true);
                if (videoProcAmp_SetValue != null)
                    videoProcAmp_SetValue(videoProcAmpProperty, val, _ckb_auto.IsChecked == true ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual, true);
            }
        }
        int val;

        public int _maximum { get => maximum; set { maximum = value; OnPropertyChanged(); } }
        int maximum;

        public int _minimum { get => minimum; set { minimum = value; OnPropertyChanged(); } }
        int minimum;

        public int _step { get => step; set { step = value; OnPropertyChanged(); } }
        int step;

        public int _default { get => default_value; set { default_value = value; OnPropertyChanged(); } }
        int default_value;

        public _Parameter()
        {
            InitializeComponent();
            DataContext = this;
        }

        internal void _Link(KeyValuePair<CameraControlProperty, WebCamParameter_Full_CameraControl> item,
            Action<CameraControlProperty, int, CameraControlFlags, bool> cameraControl_SetValue)
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
            _default = item.Value.defaultValue;
            setbycode = true;
            _ckb_auto.IsChecked = item.Value.auto;
            setbycode = false;

            this.cameraControl_SetValue = cameraControl_SetValue;
        }

        internal void _Link(KeyValuePair<VideoProcAmpProperty, WebCamParameter_Full_VideoProcAmp> item,
            Action<VideoProcAmpProperty, int, VideoProcAmpFlags, bool> videoProcAmp_SetValue)
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
            _default = item.Value.defaultValue;
            setbycode = true;
            _ckb_auto.IsChecked = item.Value.auto;
            setbycode = false;

            this.videoProcAmp_SetValue = videoProcAmp_SetValue;
        }

        void _SetDefaultValue_Click(object sender, MouseButtonEventArgs e)
        {
            _value = _default;
        }

        void _SetAutoManu_CheckUncheck(object sender, RoutedEventArgs e)
        {
            if (setbycode) return;
            _value = val;
        }
    }}