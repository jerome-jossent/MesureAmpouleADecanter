using DirectShowLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WebCamParameters_UC
{
    public class _Manager
    {
        public static WebCamConfig ShowDialog()
        {
            _Formulaire f = new _Formulaire();

            Window w = new Window();
            w.Title = "Camera settings";
            w.Content = f;
            w.SizeToContent = SizeToContent.WidthAndHeight;
            w.ShowDialog();

            return f._GetWebCamConfig();
        }

        public static void Set_WebCamConfig(string filepath)
        {
            WebCamConfig? wcc = WebCamConfig.FromFile(filepath);
            if (wcc == null) return;

            _Formulaire f = new _Formulaire();
            DirectShowLib.DsDevice device = f._GetWebcam(wcc);
            if (device == null) return;

            f._Load(wcc, false);
        }

        public static List<DsDevice> Get_WebCams()
        {
            return _Formulaire._GetWebcams();
        }

        public static void Set_WebCamParameter(int deviceIndex, VideoProcAmpProperty vpap, float _0to1, bool setAuto)
        {
            Set_WebCamParameter(Get_WebCams()[deviceIndex], vpap, _0to1, setAuto);
        }

        public static void Set_WebCamParameter(string deviceName, VideoProcAmpProperty vpap, float _0to1, bool setAuto)
        {
            foreach (DsDevice device in Get_WebCams())
                if (device.Name == deviceName)
                {
                    Set_WebCamParameter(device, vpap, _0to1, setAuto);
                    break;
                }
        }

        public static void Set_WebCamParameter(DsDevice device, VideoProcAmpProperty vpap, float _0to1, bool setAuto)
        {
            // Initialisation du filtre de capture
            IFilterGraph2? graphBuilder = new FilterGraph() as IFilterGraph2;
            IBaseFilter captureFilter = null;

            graphBuilder.AddSourceFilterForMoniker(device.Mon, null, device.Name, out captureFilter);

            var videoProcAmp = captureFilter as IAMVideoProcAmp;

            int currentValue, minValue, maxValue, stepSize, defaultValue;
            VideoProcAmpFlags flags;

            videoProcAmp.Get(vpap, out currentValue, out flags);
            bool auto = flags == VideoProcAmpFlags.Auto;
            videoProcAmp.GetRange(vpap, out minValue, out maxValue, out stepSize, out defaultValue, out flags);

            int value = (int)(_0to1 * (maxValue - minValue));
            VideoProcAmpFlags flag = setAuto ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual;

            videoProcAmp.Set(vpap, value, flag);
        }



        public static void Set_WebCamParameter(int deviceIndex, CameraControlProperty ccp, float _0to1, bool setAuto)
        {
            Set_WebCamParameter(Get_WebCams()[deviceIndex], ccp, _0to1, setAuto);
        }

        public static void Set_WebCamParameter(string deviceName, CameraControlProperty ccp, float _0to1, bool setAuto)
        {
            foreach (DsDevice device in Get_WebCams())
                if (device.Name == deviceName)
                {
                    Set_WebCamParameter(device, ccp, _0to1, setAuto);
                    break;
                }
        }

        public static void Set_WebCamParameter(DsDevice device, CameraControlProperty ccp, float _0to1, bool setAuto)
        {
            // Initialisation du filtre de capture
            IFilterGraph2? graphBuilder = new FilterGraph() as IFilterGraph2;
            IBaseFilter captureFilter = null;

            graphBuilder.AddSourceFilterForMoniker(device.Mon, null, device.Name, out captureFilter);

            var cameraControl = captureFilter as IAMCameraControl;

            int currentValue, minValue, maxValue, stepSize, defaultValue;
            CameraControlFlags flags;

            cameraControl.Get(ccp, out currentValue, out flags);
            bool auto = flags == CameraControlFlags.Auto;
            cameraControl.GetRange(ccp, out minValue, out maxValue, out stepSize, out defaultValue, out flags);

            int value = (int)(_0to1 * (maxValue - minValue));
            CameraControlFlags flag = setAuto ? CameraControlFlags.Auto : CameraControlFlags.Manual;

            cameraControl.Set(ccp, value, flag);
        }



        public static void Set_WebCamDefaultValues(int deviceIndex)
        {
            Set_WebCamDefaultValues(Get_WebCams()[deviceIndex]);
        }

        public static void Set_WebCamDefaultValues(string deviceName)
        {
            foreach (DsDevice device in Get_WebCams())
                if (device.Name == deviceName)
                {
                    Set_WebCamDefaultValues(device);
                    break;
                }
        }

        public static void Set_WebCamDefaultValues(DsDevice device)
        {
            // Initialisation du filtre de capture
            IFilterGraph2? graphBuilder = new FilterGraph() as IFilterGraph2;
            IBaseFilter captureFilter = null;

            graphBuilder.AddSourceFilterForMoniker(device.Mon, null, device.Name, out captureFilter);

            int minValue, maxValue, stepSize, defaultValue;

            var cameraControl = captureFilter as IAMCameraControl;
            foreach (CameraControlProperty ccp in (CameraControlProperty[])Enum.GetValues(typeof(CameraControlProperty)))
            {
                cameraControl.GetRange(ccp, out minValue, out maxValue, out stepSize, out defaultValue, out CameraControlFlags flags);
                cameraControl.Set(ccp, defaultValue, CameraControlFlags.Auto);
            }

            var videoProcAmp = captureFilter as IAMVideoProcAmp;
            foreach (VideoProcAmpProperty vpa in (VideoProcAmpProperty[])Enum.GetValues(typeof(VideoProcAmpProperty)))
            {
                videoProcAmp.GetRange(vpa, out minValue, out maxValue, out stepSize, out defaultValue,  out VideoProcAmpFlags flags_vpa);
                videoProcAmp.Set(vpa, defaultValue, VideoProcAmpFlags.Auto);
            }
        }
    }
}
