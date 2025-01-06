using DirectShowLib;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WebCamParameters_UC
{
    public partial class _Formulaire : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        IBaseFilter captureFilter = null;
        IFilterGraph2? graphBuilder = null;

        Dictionary<int, DsDevice> dsDevices;
        DsDevice device_current;

        WebCamConfig _webCamConfig;// = new WebCamConfig();

        Dictionary<VideoProcAmpProperty, WebCamParameter_VideoProcAmp> videoProcAmpParameters;
        Dictionary<CameraControlProperty, WebCamParameter_CameraControl> cameraControlParameters;

        public _Formulaire()
        {
            InitializeComponent();
            DataContext = this;
            Unloaded += _Formulaire_Unloaded;
            Loaded += _Formulaire_Loaded;
        }

        void _Formulaire_Unloaded(object sender, RoutedEventArgs e)
        {
            // Nettoyage
            if (graphBuilder != null)
                Marshal.ReleaseComObject(graphBuilder);
            if (captureFilter != null)
                Marshal.ReleaseComObject(captureFilter);
        }

        void _Formulaire_Loaded(object sender, RoutedEventArgs e)
        {
            captureFilter = null;
            graphBuilder = new FilterGraph() as IFilterGraph2;
            WebcamsToCombobox(_cbx_devices);
        }

        void WebcamsToCombobox(ComboBox cbx)
        {
            List<DsDevice> webcams = _GetWebcams();
            dsDevices = new Dictionary<int, DsDevice>();
            cbx.Items.Clear();
            foreach (var webcam in webcams)
            {
                dsDevices.Add(dsDevices.Count, webcam);
                cbx.Items.Add(webcam.Name);
            }
        }

        public static List<DsDevice> _GetWebcams()
        {
            List<DsDevice> devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice).ToList();
            return devices;
        }

        internal DsDevice _GetWebcam(WebCamConfig wcc)
        {
            return _GetWebcam(wcc.device_name);
        }

        internal DsDevice _GetWebcam(string device_name)
        {
            foreach (var device in _GetWebcams())
                if (device.Name == device_name)
                    return device;
            return null;
        }

        void _cbx_devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as ComboBox).SelectedIndex;
            device_current = dsDevices[index];

            //codec & résolution & framerate : TODO ?

            WebCamParameters_Full wcs = _GetWebCamSettings(device_current);
            Fill(wcs);
        }

        Dictionary<string, _Parameter> ucs = new Dictionary<string, _Parameter>();

        void Fill(WebCamParameters_Full wcs)
        {
            ucs.Clear();
            _spcc.Children.Clear();
            foreach (var item in wcs.webCamParameters_CameraControl)
            {
                _Parameter p = new _Parameter();
                p._Link(item, CameraControl_SetValue);
                _spcc.Children.Add(p);
                ucs.Add(item.Value.name, p);
            }

            _spapv.Children.Clear();
            foreach (var item in wcs.webCamParameters_VideoProcAmp)
            {
                _Parameter p = new _Parameter();
                p._Link(item, VideoProcAmp_SetValue);
                _spapv.Children.Add(p);
                ucs.Add(item.Value.name, p);
            }
        }

        public void VideoProcAmp_SetValue(VideoProcAmpProperty vpa, int value, VideoProcAmpFlags flag, bool updateUC = false)
        {
            var videoProcAmp = captureFilter as IAMVideoProcAmp;
            videoProcAmp.Set(vpa, value, flag);
            if (updateUC)
            {
                _webCamConfig.videoProcAmpProperties[vpa.ToString()].value = value;
                _webCamConfig.videoProcAmpProperties[vpa.ToString()].auto = flag == VideoProcAmpFlags.Auto;
            }
        }

        public void CameraControl_SetValue(CameraControlProperty cc, int value, CameraControlFlags flag, bool updateUC = false)
        {
            var cameraControl = captureFilter as IAMCameraControl;
            cameraControl.Set(cc, value, flag);
            if (updateUC)
            {
                _webCamConfig.cameraControlProperties[cc.ToString()].value = value;
                _webCamConfig.cameraControlProperties[cc.ToString()].auto = flag == CameraControlFlags.Auto;
            }
        }

        public WebCamParameters_Full _GetWebCamSettings()
        {
            return _GetWebCamSettings(device_current);
        }

        public WebCamParameters_Full _GetWebCamSettings(DsDevice device)
        {
            videoProcAmpParameters = new Dictionary<VideoProcAmpProperty, WebCamParameter_VideoProcAmp>();
            cameraControlParameters = new Dictionary<CameraControlProperty, WebCamParameter_CameraControl>();

            // Initialisation du filtre de capture
            graphBuilder.AddSourceFilterForMoniker(device.Mon, null, device.Name, out captureFilter);

            #region Accéder à l'interface IAMCameraControl
            var cameraControl = captureFilter as IAMCameraControl;
            if (cameraControl != null)
            {
                foreach (CameraControlProperty cc in (CameraControlProperty[])Enum.GetValues(typeof(CameraControlProperty)))
                {
                    int currentValue, minValue, maxValue, stepSize, defaultValue;
                    CameraControlFlags flags;
                    cameraControl.Get(cc, out currentValue, out flags);
                    bool auto = flags == CameraControlFlags.Auto;
                    cameraControl.GetRange(cc, out minValue, out maxValue, out stepSize, out defaultValue, out flags);
                    WebCamParameter_CameraControl cc_val =
                        new WebCamParameter_CameraControl(cc.ToString(), currentValue, minValue, maxValue, stepSize, defaultValue, auto, flags);
                    cameraControlParameters.Add(cc, cc_val);
                }
            }
            #endregion

            #region Accès à l'interface IAMVideoProcAmp
            var videoProcAmp = captureFilter as IAMVideoProcAmp;
            if (videoProcAmp != null)
            {
                foreach (VideoProcAmpProperty vpa in (VideoProcAmpProperty[])Enum.GetValues(typeof(VideoProcAmpProperty)))
                {
                    int currentValue, minValue, maxValue, stepSize, defaultValue;
                    VideoProcAmpFlags flags;
                    videoProcAmp.Get(vpa, out currentValue, out flags);
                    bool auto = flags == VideoProcAmpFlags.Auto;
                    videoProcAmp.GetRange(vpa, out minValue, out maxValue, out stepSize, out defaultValue, out flags);
                    WebCamParameter_VideoProcAmp vpa_val =
                        new WebCamParameter_VideoProcAmp(vpa.ToString(), currentValue, minValue, maxValue, stepSize, defaultValue, auto, flags);
                    videoProcAmpParameters.Add(vpa, vpa_val);
                }
            }
            #endregion

            _webCamConfig = new WebCamConfig(device.Name, cameraControlParameters, videoProcAmpParameters);

            return new WebCamParameters_Full()
            {
                webCamParameters_VideoProcAmp = videoProcAmpParameters,
                webCamParameters_CameraControl = cameraControlParameters
            };
        }

        void _DevicesListRefresh_Click(object sender, MouseButtonEventArgs e)
        {
            WebcamsToCombobox(_cbx_devices);
        }

        void _DeviceParametersSave_Click(object sender, MouseButtonEventArgs e)
        {
            if (device_current == null) return;

            _GetWebCamSettings(device_current);

            //json
            string jsonString = JsonConvert.SerializeObject(_webCamConfig, Formatting.Indented);

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = device_current.Name;
            dlg.Filter = "WebCamConfig|*.wcc|All files |*.*";
            if (dlg.ShowDialog() != true)
                return;

            System.IO.File.WriteAllText(dlg.FileName, jsonString);
        }

        public void _Load(WebCamConfig webCamConfig, bool updateIHM)
        {
            if (graphBuilder == null)
                graphBuilder = new FilterGraph() as IFilterGraph2;
            // Initialisation du filtre de capture
            if (captureFilter == null)
                graphBuilder.AddSourceFilterForMoniker(_GetWebcam(webCamConfig).Mon, null, webCamConfig.device_name, out captureFilter);

            //set values & update IHM
            foreach (KeyValuePair<string, WebCamParameter> item in webCamConfig.cameraControlProperties)
            {
                CameraControlProperty name = WebCamParameter._GetCameraControlProperty(item.Key);
                CameraControl_SetValue(name, item.Value.value, item.Value.auto ? CameraControlFlags.Auto : CameraControlFlags.Manual);
                if (updateIHM)
                {
                    ucs[item.Key]._value = item.Value.value;
                    ucs[item.Key]._ckb_auto.IsChecked = item.Value.auto;
                }
            }

            foreach (KeyValuePair<string, WebCamParameter> item in webCamConfig.videoProcAmpProperties)
            {
                VideoProcAmpProperty name = WebCamParameter._GetVideoProcAmpProperty(item.Key);
                VideoProcAmp_SetValue(name, item.Value.value, item.Value.auto ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual);
                if (updateIHM)
                {
                    ucs[item.Key]._value = item.Value.value;
                    ucs[item.Key]._ckb_auto.IsChecked = item.Value.auto;
                }
            }
        }

        void _DeviceParametersLoad_Click(object sender, MouseButtonEventArgs e)
        {
            if (device_current == null) return;

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "WebCamConfig|*.wcc|All files |*.*";
            if (dlg.ShowDialog() != true)
                return;

            WebCamConfig? wcc = WebCamConfig.FromFile(dlg.FileName);

            //même device ?
            if (device_current.Name != wcc.device_name)
            {
                MessageBox.Show("Incompatible configuration", "Loaded configuration is only suitable for cameras named \"" +
                   wcc.device_name + "\"", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _Load(wcc, true);
        }

        internal WebCamConfig _GetWebCamConfig()
        {
            return _webCamConfig;
        }
    }
}
