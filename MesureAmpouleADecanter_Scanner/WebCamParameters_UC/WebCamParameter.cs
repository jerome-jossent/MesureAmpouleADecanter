using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCamParameters_UC
{
    public class WebCamParameter
    {
        public static CameraControlProperty _GetCameraControlProperty(string name)
        {
            return (CameraControlProperty)Enum.Parse(typeof(CameraControlProperty), name, true);
        }
        public static VideoProcAmpProperty _GetVideoProcAmpProperty(string name)
        {
            return (VideoProcAmpProperty)Enum.Parse(typeof(VideoProcAmpProperty), name, true);
        }

        //public enum WebCamParameterType { CameraControlProperty, VideoProcAmpProperty }
        //public WebCamParameterType type { get; set; }
        //public string name { get; set; }
        public int value { get; set; }
        public bool auto { get; set; }

        public WebCamParameter() { }//pour JSON

        public WebCamParameter(WebCamParameter_CameraControl item)
        {
            //name = item.name;
            //type = WebCamParameterType.CameraControlProperty;
            value = item.currentValue;
            auto = item.auto;
        }

        public WebCamParameter(WebCamParameter_VideoProcAmp item)
        {
            //name = item.name;
            //type = WebCamParameterType.VideoProcAmpProperty;
            value = item.currentValue;
            auto = item.auto;
        }
    }
}
