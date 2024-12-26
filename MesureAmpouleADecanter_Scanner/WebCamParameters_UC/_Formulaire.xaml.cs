using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebCamParameters_UC
{
    /*
    Brightness val = 128[0;255:1] default=128 manual only
    Contrast val = 128[0;255:1] default=128 manual only
    Hue val = 128[0;255:1] default=128 manual only
    Saturation val = 128[0;255:1] default=128 manual only
    Sharpness val = 128[0;255:1] default=128 manual only
    Gamma val = 128[0;255:1] default=128 manual only
    ColorEnable val = 128[0;255:1] default=128 manual only
    WhiteBalance val = 4000[2000;6500:1] default=4000 auto allowed
    BacklightCompensation val = 0[0;1:1] default=0 manual only
    Gain val = 0[0;255:1] default=0 manual only

    Camera Control :
    Pan val = 0[-10;10:1] default=0 manual only
    Tilt val = 0[-10;10:1] default=0 manual only
    Roll val = 0[-10;10:1] default=0 manual only
    Zoom val = 100[100;500:1] default=100 manual only
    Exposure val = -5[-11;-2:1] default=-5 auto allowed
    Iris val = -5[-11;-2:1] default=-5 auto allowed
    Focus val = 10[0;250:5] default=0 auto allowed
    */

    public partial class _Formulaire : UserControl
    {
        IBaseFilter captureFilter = null;
        IFilterGraph2? graphBuilder = null;

        Dictionary<int, DsDevice> dsDevices;
        DsDevice device_current;

        Dictionary<VideoProcAmpProperty, WebCamParameters_VideoProcAmp> videoProcAmpParameters;
        Dictionary<CameraControlProperty, WebCamParameters_CameraControl> cameraControlParameters;

        //Dictionary<VideoProcAmpProperty, WebCamParameters_VideoProcAmp> videoProcAmpParameters;
        //Dictionary<CameraControlProperty, WebCamParameters_CameraControl> cameraControlParameters;

        public _Formulaire()
        {
            InitializeComponent();
            Unloaded += _Formulaire_Unloaded;
            Loaded += _Formulaire_Loaded;
        }

        void _Formulaire_Unloaded(object sender, RoutedEventArgs e)
        {
            // Nettoyage
            Marshal.ReleaseComObject(graphBuilder);
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

        public List<DsDevice> _GetWebcams()
        {
            List<DsDevice> devices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice).ToList();
            return devices;
        }

        void _cbx_devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as ComboBox).SelectedIndex;
            device_current = dsDevices[index];

            //codec & résolution & framerate 

            WebCamParameters_Full wcs = _GetWebCamSettings(device_current);

            Fill(wcs);

            string ligne = "";
            string webcam_parameters = "Video Proc Amp :\n";
            foreach (var vpa in wcs.webCamParameters_VideoProcAmp)
            {
                ligne = vpa.Value.ToString();
                webcam_parameters += ligne + "\n";
            }

            webcam_parameters += "\nCamera Control :\n";
            foreach (var cc in wcs.webCamParameters_CameraControl)
            {
                ligne = cc.Value.ToString();
                webcam_parameters += ligne + "\n";
            }

            _tbk.Text = webcam_parameters;
        }

        void Fill(WebCamParameters_Full wcs)
        {
            _sp.Children.Clear();

            foreach (var item in wcs.webCamParameters_CameraControl)
            {
                _Parameter p = new _Parameter();
                p._Link(item, CameraControl_SetValue);
                _sp.Children.Add(p);
            }

            foreach (var item in wcs.webCamParameters_VideoProcAmp)
            {
                _Parameter p = new _Parameter();
                p._Link(item, VideoProcAmp_SetValue);
                _sp.Children.Add(p);
            }
        }

        public void VideoProcAmp_SetValue(VideoProcAmpProperty vpa, int value)
        {

        }

        public void CameraControl_SetValue(CameraControlProperty cc, int value)
        {
            var cameraControl = captureFilter as IAMCameraControl;
            cameraControl.Set(cc, value, CameraControlFlags.Manual);
        }

        public WebCamParameters_Full _GetWebCamSettings(DsDevice device)
        {
            videoProcAmpParameters = new Dictionary<VideoProcAmpProperty, WebCamParameters_VideoProcAmp>();
            cameraControlParameters = new Dictionary<CameraControlProperty, WebCamParameters_CameraControl>();

            // Initialisation du filtre de capture
            graphBuilder.AddSourceFilterForMoniker(device.Mon, null, device.Name, out captureFilter);

            // Accès à l'interface IAMVideoProcAmp
            var videoProcAmp = captureFilter as IAMVideoProcAmp;
            if (videoProcAmp != null)
            {
                foreach (VideoProcAmpProperty vpa in (VideoProcAmpProperty[])Enum.GetValues(typeof(VideoProcAmpProperty)))
                {
                    int currentValue, minValue, maxValue, stepSize, defaultValue;
                    VideoProcAmpFlags flags;
                    videoProcAmp.Get(vpa, out currentValue, out flags);
                    videoProcAmp.GetRange(vpa, out minValue, out maxValue, out stepSize, out defaultValue, out flags);
                    //Console.WriteLine($"Luminosité actuelle : {currentValue}, Min : {minValue}, Max : {maxValue}");
                    //videoProcAmp.Set(VideoProcAmpProperty.Brightness, defaultValue, VideoProcAmpFlags.Manual);
                    WebCamParameters_VideoProcAmp vpa_val = new WebCamParameters_VideoProcAmp(vpa.ToString(), currentValue, minValue, maxValue, stepSize, defaultValue, flags);
                    videoProcAmpParameters.Add(vpa, vpa_val);
                }
            }

            // Accéder à l'interface IAMCameraControl
            var cameraControl = captureFilter as IAMCameraControl;
            if (cameraControl != null)
            {
                foreach (CameraControlProperty cc in (CameraControlProperty[])Enum.GetValues(typeof(CameraControlProperty)))
                {
                    int currentValue, minValue, maxValue, stepSize, defaultValue;
                    CameraControlFlags flags;
                    cameraControl.Get(cc, out currentValue, out flags);
                    cameraControl.GetRange(cc, out minValue, out maxValue, out stepSize, out defaultValue, out flags);
                    //Console.WriteLine($"Focus actuel : {currentFocus} Min = {minValue}, Max = {maxValue}, Par défaut = {defaultValue}, Pas = {stepSize}");
                    //cameraControl.Set(CameraControlProperty.Focus, 1, CameraControlFlags.Manual);
                    WebCamParameters_CameraControl cc_val = new WebCamParameters_CameraControl(cc.ToString(), currentValue, minValue, maxValue, stepSize, defaultValue, flags);
                    cameraControlParameters.Add(cc, cc_val);
                }
            }

            return new WebCamParameters_Full()
            {
                webCamParameters_VideoProcAmp = videoProcAmpParameters,
                webCamParameters_CameraControl = cameraControlParameters
            };
        }

        public static void _ShowDialog()
        {
            _Formulaire f = new _Formulaire();

            Window w = new Window();
            w.Content = f;

            w.ShowDialog();

        }
    }
}
