using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCamParameters_UC
{
    public class WebCamParameters_Full
    {
        public Dictionary<VideoProcAmpProperty, WebCamParameters_VideoProcAmp> webCamParameters_VideoProcAmp { get; set; }
        public Dictionary<CameraControlProperty, WebCamParameters_CameraControl> webCamParameters_CameraControl { get; set; }
    }

    public abstract class WebCamParameters_abstract
    {
        public string name;
        public int currentValue, minValue, maxValue, stepSize, defaultValue;
        public bool auto_enabled;

        public override string ToString()
        {
            return name + " val=" + currentValue + " [" + minValue + ";" + maxValue + ":" + stepSize + "] default=" + defaultValue + (auto_enabled? " auto allowed": " manual only");
        }
    }

    public class WebCamParameters_VideoProcAmp : WebCamParameters_abstract
    {
        public VideoProcAmpFlags flags_vpa;

        public WebCamParameters_VideoProcAmp(string name,
            int currentValue,
            int minValue, int maxValue, int stepSize,
            int defaultValue,
            VideoProcAmpFlags flags_vpa)
        {
            this.name = name;
            this.currentValue = currentValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.stepSize = stepSize;
            this.defaultValue = defaultValue;
            this.flags_vpa = flags_vpa;
            auto_enabled = flags_vpa == (VideoProcAmpFlags.Manual | VideoProcAmpFlags.Auto);
        }

        //public override string ToString()
        //{
        //    return name + " : " + currentValue + " [" + minValue + ";" + maxValue + ":" + stepSize + "] def : " + defaultValue + " " + flags_vpa.ToString();
        //}
    }

    public class WebCamParameters_CameraControl : WebCamParameters_abstract
    {
        public CameraControlFlags flags_cc;

        public WebCamParameters_CameraControl(string name,
            int currentValue,
            int minValue, int maxValue, int stepSize,
            int defaultValue,
            CameraControlFlags flags_cc)
        {
            this.name = name;
            this.currentValue = currentValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.stepSize = stepSize;
            this.defaultValue = defaultValue;
            this.flags_cc = flags_cc;
            auto_enabled = flags_cc == (CameraControlFlags.Manual | CameraControlFlags.Auto);
        }
        //public override string ToString()
        //{
        //    return name + " : " + currentValue + " [" + minValue + ";" + maxValue + ":" + stepSize + "] def : " + defaultValue + " " + flags_cc.ToString();
        //}
    }
}
